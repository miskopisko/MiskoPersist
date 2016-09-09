using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Threading;
using System.Windows.Forms;
using log4net;
using MiskoPersist.Data.Viewed;
using MiskoPersist.Enums;
using MiskoPersist.Message.Requests;
using MiskoPersist.Message.Responses;
using MiskoPersist.Serialization;

namespace MiskoPersist.Core
{
	public class Server
	{
		private static ILog Log = LogManager.GetLogger(typeof(Server));
		
		#region Delegates

		[EditorBrowsable(EditorBrowsableState.Never)]
		public delegate void MessageCompleteHandler(ResponseMessage Response);
		[EditorBrowsable(EditorBrowsableState.Never)]
		public delegate void MessageSentHandler();
		[EditorBrowsable(EditorBrowsableState.Never)]
		public delegate void MessageReceivedHandler();
		[EditorBrowsable(EditorBrowsableState.Never)]
		public delegate void StatusHandler(MessageStatus status);
		[EditorBrowsable(EditorBrowsableState.Never)]
		public delegate void ErrorHandler(ErrorMessage message);
		[EditorBrowsable(EditorBrowsableState.Never)]
		public delegate void WarningHandler(ErrorMessage message);
		[EditorBrowsable(EditorBrowsableState.Never)]
		public delegate void InfoHandler(ErrorMessage message);
		[EditorBrowsable(EditorBrowsableState.Never)]
		public delegate void ConfirmHandler(ErrorMessage message, ConfirmationEventArgs args);
		
		#endregion
		
		#region Events
		
		public static event MessageSentHandler MessageSent;
		public static event MessageReceivedHandler MessageReceived;
		public static event StatusHandler Status;
		public static event ErrorHandler Error;
		public static event WarningHandler Warning;
		public static event InfoHandler Info;
		public static event ConfirmHandler Confirm;
		
		#endregion
		
		#region Fields
		
		private static Int32 mActive_ = 0;
		
		private Thread mThread_;
		private MessageCompleteHandler mSuccessHandler_;
		private MessageCompleteHandler mErrorHandler_;
		private RequestMessage mRequest_;
		private Guid? mSessionToken_;
		
		#endregion
		
		#region Properties
		
		public static SerializationType SerializationType
		{
			get;
			set; 
		}
		
		public static ServerLocation Location
		{
			get;
			set; 
		}
		
		public static String Host
		{
			get;
			set; 
		}
		
		public static Int16 Port
		{
			get;
			set; 
		}
		
		public static String Script
		{
			get;
			set; 
		}
		
		public static Boolean UseSSL
		{
			get;
			set; 
		}
		
		public static Int16 Timeout
		{
			get;
			set; 
		}
		
		public static Boolean WriteMessagesToLog
		{
			get;
			set; 
		}

        #endregion
		
		#region Constructors
		
		public Server(RequestMessage request, MessageCompleteHandler successHandler, MessageCompleteHandler errorHandler)
		{
			mRequest_ = request;
			mRequest_.SessionToken = mSessionToken_;
			
			mSuccessHandler_ = successHandler;
			mErrorHandler_ = errorHandler;
			
			if (Location != null && Location.IsSet && Location.Equals(ServerLocation.Online))
			{
				if (String.IsNullOrEmpty(Host))
				{
					throw new MiskoException("Server location is online but host is not defined");
				}
				
				if (Port == 0)
				{
					Port = 80;
				}
				
				if (Timeout == 0)
				{
					Timeout = 10;
				}
				
				if (String.IsNullOrEmpty(Script))
				{
					throw new MiskoException("Server location is online but script is not defined");
				}
				
				if (SerializationType == null || SerializationType.IsNotSet)
				{
					throw new MiskoException("Serialization type is not defined");
				}
			}
		}
		
		#endregion
		
		#region Private Methods
		
		private void Send()
		{
			Interlocked.Increment(ref mActive_);
			Invoke(MessageSent);
			
			mThread_ = new Thread(new ThreadStart(Run));
			mThread_.Name = mRequest_.GetType().Name;
			mThread_.IsBackground = true;
			mThread_.Start();
		}
		
		private void Run()
		{
			Invoke(Status, MessageStatus.Processing);
			
			if (WriteMessagesToLog && (SerializationType != null && SerializationType.IsSet))
			{
				Log.Debug(Environment.NewLine + Serializer.Serialize(mRequest_, SerializationType));
			}
			
            ResponseMessage response = (Location != null && Location.IsSet && Location.Equals(ServerLocation.Online)) ? SendToServer() : MessageProcessor.Process(mRequest_);
			
            if (WriteMessagesToLog && (SerializationType != null && SerializationType.IsSet))
			{
				Log.Debug(Environment.NewLine + Serializer.Serialize(response, SerializationType));
			}
			
			if (HandleErrors(response))
			{
				// No errors in the message; call the successfulHandler
				if (!response.HasErrors && !response.HasUnconfirmed)
				{
					Invoke(mSuccessHandler_, response);
				}
			}
			
			// If errors in the message; call errorHandler
			if (response.HasErrors || response.HasUnconfirmed)
			{
				Invoke(mErrorHandler_, response);
			}
			
			Done();
		}
		
		private Boolean HandleErrors(ResponseMessage response)
		{
			Boolean continueProcessing = true;
			
			foreach (ErrorMessage error in response.Errors) 
			{
				Invoke(Status, MessageStatus.Error);
				Invoke(Error, error);
			}
			
			foreach (ErrorMessage warning in response.Warnings) 
			{
				Invoke(Status, MessageStatus.Warning);
				Invoke(Warning, warning);
			}
			
			foreach (ErrorMessage info in response.Infos) 
			{
				Invoke(Status, MessageStatus.Information);
				Invoke(Info, info);
			}
			
			foreach (ErrorMessage confirmation in response.Confirmations) 
			{
				if (confirmation.Confirmed.HasValue && !confirmation.Confirmed.Value)
				{
					Invoke(Status, MessageStatus.Confirmation);
					ConfirmationEventArgs args = new ConfirmationEventArgs();
					Invoke(Confirm, confirmation, args);
					
					if (args.Confirmed)
					{
						confirmation.Confirmed = true;
						mRequest_.Confirmations.Add(confirmation);
						SendRequest(mRequest_, mSuccessHandler_, mErrorHandler_);
						continueProcessing = false;
						break;
					}
					
					Invoke(Status, MessageStatus.Error);
					break;
				}
			}
			
			return continueProcessing;
		}
		
		private void Done()
		{
			Interlocked.Decrement(ref mActive_);
			Invoke(Status, mActive_ == 0 ? MessageStatus.Success : MessageStatus.Processing);
			Invoke(MessageReceived);
		}

		private void Invoke(Delegate d, params Object[] args)
		{
			try
			{
				if (d != null)
				{
					Control control = d.Target as System.Windows.Forms.Control;
					
					if (control != null && control.InvokeRequired)
					{
						control.Invoke(d, args);
					}
					else
					{
						d.DynamicInvoke(args);
					}
				}
			}
			catch (Exception ex)
			{
                do
                {
                    Log.Error("Unexpected Error: (" + ex.Message + ") @ " + d.Method.DeclaringType.Name + "." + d.Method.Name, ex);
                    Invoke(Error, new ErrorMessage(ex));
                    ex = ex.InnerException;
                }
                while (ex != null);
			}
		}
		
		private ResponseMessage SendToServer()
		{
			ResponseMessage responseMessage = null;

            try
            {
            	Uri uri = new Uri((UseSSL ? "https://" : "http://") + Host + ":" + Port + Script);
                String request = Serializer.Serialize(mRequest_, SerializationType);

                WebRequest webRequest = WebRequest.Create(uri);
                webRequest.Proxy.Credentials = CredentialCache.DefaultNetworkCredentials;
                webRequest.Method = "POST";
                webRequest.ContentType = SerializationType.ToHttpContentType();
                webRequest.Timeout = Timeout * 1000;
                webRequest.ContentLength = Serializer.Encoding.GetByteCount(request);

                Stopwatch stopwatch = Stopwatch.StartNew();
                using (StreamWriter writer = new StreamWriter(webRequest.GetRequestStream(), Serializer.Encoding))
                {
                    writer.Write(request);
				}

                using (MemoryStream ms = new MemoryStream())
                {
                    webRequest.GetResponse().GetResponseStream().CopyTo(ms);
                    responseMessage = (ResponseMessage)Serializer.Deserialize(ms);
                }

                stopwatch.Stop();
                Log.Debug(String.Format("{0} round trip from {1} : {2}", mRequest_.WrapperClass.Name, uri, stopwatch.Elapsed));
			}
			catch (Exception ex)
			{
				responseMessage = (ResponseMessage)Activator.CreateInstance(mRequest_.ResponseClass);
				responseMessage.Status = ErrorLevel.Error;

                do
                {
                    Log.Error("Unexpected Error: (" + ex.Message + ")", ex);
                    responseMessage.Errors.Add(new ErrorMessage(ex));
                    ex = ex.InnerException;
                }
                while (ex != null);
			}
			
			return responseMessage;
		}
		
		#endregion
		
		#region Public Methods

		public static void SendRequest(RequestMessage request)
		{
			SendRequest(request, null, null);
		}
		
		public static void SendRequest(RequestMessage request, MessageCompleteHandler successHandler)
		{
			SendRequest(request, successHandler, null);
		}

		public static void SendRequest(RequestMessage request, MessageCompleteHandler successHandler, MessageCompleteHandler errorHandler)
		{
			new Server(request, successHandler, errorHandler).Send();
		}

		#endregion
	}
	
	#region ConfirmationEventArgs Class
	
	public class ConfirmationEventArgs : EventArgs
	{
		#region Fields
		
		private Boolean mConfirmed_ = false;
		
		#endregion
		
		#region Properties
		
		public Boolean Confirmed
		{
			get
			{
				return mConfirmed_;
			}
			set
			{
				mConfirmed_ = value;
			}
		}
		
		#endregion
	}
	
	#endregion
}
