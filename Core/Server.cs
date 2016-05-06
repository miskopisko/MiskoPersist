using System;
using System.ComponentModel;
using System.IO;
using System.Net;
using System.Reflection;
using System.Threading;
using System.Windows.Forms;
using log4net;
using Message;
using MiskoPersist.Data;
using MiskoPersist.Enums;
using MiskoPersist.Message.Request;
using MiskoPersist.Message.Response;
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
		[EditorBrowsable(EditorBrowsableState.Never)]
		public delegate void DebugHandler(CoreMessage message);
		
		#endregion
		
		#region Events
		
		public static event MessageSentHandler MessageSent;
		public static event MessageReceivedHandler MessageReceived;
		public static event StatusHandler Status;
		public static event ErrorHandler Error;
		public static event WarningHandler Warning;
		public static event InfoHandler Info;
		public static event ConfirmHandler Confirm;
		public static event DebugHandler Debug;
		
		#endregion
		
		#region Fields
		
		private static Int32 mActive_ = 0;
		
		private Thread mThread_;
		private MessageCompleteHandler mSuccessHandler_;
		private MessageCompleteHandler mErrorHandler_;
		private RequestMessage mRequest_;
		
		#endregion
		
		#region Properties
		
		public static SerializationType SerializationType { get; set; }
		public static ServerLocation Location { get; set; }
		public static String Host { get; set; }
		public static Int16 Port { get; set; }
		public static String Script { get; set; }
		public static Boolean UseSSL { get; set; }
		
		public static String Datasource
		{
			get
			{
				return Location.Equals(ServerLocation.Local) ? DatabaseConnections.Connections["Default"].Datasource : Uri.Host;
			}
		}
		
		private static Uri Uri
		{
			get
			{
				return new Uri((UseSSL ? "https://" : "http://") + Host + ":" + Port + Script);
			}
		}
		
		#endregion
		
		#region Constructors
		
		public Server(RequestMessage request, MessageCompleteHandler successHandler, MessageCompleteHandler errorHandler)
		{
			mRequest_ = request;
			mSuccessHandler_ = successHandler;
			mErrorHandler_ = errorHandler;
			
			// Do some validations
			if (Location == null || Location.IsNotSet)
			{
				throw new MiskoException("Server location is not defined");
			}
			
			if (Location.Equals(ServerLocation.Online))
			{
				if (String.IsNullOrEmpty(Host))
				{
					throw new MiskoException("Server location is online but host is not defined");
				}
				
				if (Port == 0)
				{
					Port = 80;
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
			
			Invoke(Debug, mRequest_);
			
			ResponseMessage response = Location.Equals(ServerLocation.Local) ? MessageProcessor.Process(mRequest_) : SendToServer();
			
			Invoke(Debug, response);
			
			if (HandleErrors(response.ErrorMessages))
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
		
		private Boolean HandleErrors(ErrorMessages errorMessages)
		{
			Boolean continueProcessing = true;
			
			foreach (ErrorMessage message in errorMessages)
			{
				if (message.ErrorLevel.Equals(ErrorLevel.Error))
				{
					Invoke(Status, MessageStatus.Error);
					Invoke(Error, message);
				}
				else if (message.ErrorLevel.Equals(ErrorLevel.Warning))
				{
					Invoke(Status, MessageStatus.Warning);
					Invoke(Warning, message);
				}
				else if (message.ErrorLevel.Equals(ErrorLevel.Information))
				{
					Invoke(Status, MessageStatus.Information);
					Invoke(Info, message);
				}
				else if (message.ErrorLevel.Equals(ErrorLevel.Confirmation))
				{
					if (message.Confirmed.HasValue && !message.Confirmed.Value)
					{
						Invoke(Status, MessageStatus.Confirmation);
						ConfirmationEventArgs args = new ConfirmationEventArgs();
						Invoke(Confirm, message, args);
						
						if (args.Confirmed)
						{
							message.Confirmed = true;
							mRequest_.Confirmations.Add(message);
							SendRequest(mRequest_, mSuccessHandler_, mErrorHandler_);
							continueProcessing = false;
							break;
						}
						Invoke(Status, MessageStatus.Error);
						break;
					}
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
			catch (Exception e)
			{
				while (e.InnerException != null)
				{
					e = e.InnerException;
				}
				
				Log.Error("Error invoking method " + d.Method.DeclaringType.Name + "." + d.Method.Name, e);
				Invoke(Error, new ErrorMessage("Error invoking method " + d.Method.DeclaringType.Name + "." + d.Method.Name, e));
			}
		}
		
		private ResponseMessage SendToServer()
		{
			ResponseMessage responseMessage = new ResponseMessage();

			try
			{
				HttpWebRequest webRequest = (HttpWebRequest)WebRequest.Create(Uri);
				webRequest.Proxy.Credentials = CredentialCache.DefaultNetworkCredentials;
				webRequest.Method = "POST";
				webRequest.KeepAlive = true;
				webRequest.ContentType = SerializationType.ToHttpContentType();
				
				using (StreamWriter sw = new StreamWriter(webRequest.GetRequestStream()))
				{
					sw.Write(Serializer.Serialize(mRequest_, SerializationType));
				}
				
				responseMessage = (ResponseMessage)Serializer.Deserialize(webRequest.GetResponse().GetResponseStream(), SerializationType);
			}
			catch (Exception ex)
			{
				if (ex is TargetInvocationException)
				{
					ex = ex.InnerException;
				}
				
				responseMessage = new ResponseMessage();
				responseMessage.Status = ErrorLevel.Error;
				responseMessage.Errors.Add(new ErrorMessage(ex));
				
				while (ex.InnerException != null)
				{
					ex = ex.InnerException;
					responseMessage.Errors.Add(new ErrorMessage(ex));
				}
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
