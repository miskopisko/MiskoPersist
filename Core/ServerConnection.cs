using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Reflection;
using System.Threading;
using System.Web;
using System.Windows.Forms;
using Message;
using MiskoPersist.Data;
using MiskoPersist.Enums;
using MiskoPersist.Interfaces;
using MiskoPersist.Message.Request;
using MiskoPersist.Message.Response;
using MiskoPersist.Resources;

namespace MiskoPersist.Core
{
	public class ServerConnection
	{
		#region Delegates

		[EditorBrowsable(EditorBrowsableState.Never)]
        public delegate void MessageCompleteHandler(ResponseMessage Response);

        #endregion
		
		#region Fields
		
		private static Int32 mActive_ = 0;
		private static IOController mIOController_;
		private static Uri mUrl_;
		
		private Thread mThread_;
		private MessageCompleteHandler mSuccessHandler_;
        private MessageCompleteHandler mErrorHandler_;
        private RequestMessage mRequest_;        
		
		#endregion
		
		#region Properties
		
		public static IOController IOController 
		{
			get
			{
				if (mIOController_ == null)
	            {
	                throw new MiskoException(ErrorStrings.errIOControllerIsNull);
	            }

				return mIOController_;
			}
			set
			{
				mIOController_ = value;
                Application.ThreadException += mIOController_.Exception;
			}
		}
		
		private static Uri Url
		{
			get
			{
				if(mUrl_ == null)
				{
					String url = IOController.UseSSL ? "https://" : "http://";
		            url += IOController.Host;
		            url += ":" + IOController.Port;
		            url += IOController.Script;
		            
		            mUrl_ = new Uri(url);
				}
				
				return mUrl_;
			}
		}
		
		#endregion
		
		public ServerConnection(RequestMessage request, MessageCompleteHandler successHandler, MessageCompleteHandler errorHandler)
		{
			mRequest_ = request;
            mSuccessHandler_ = successHandler;
            mErrorHandler_ = errorHandler;
		}
		
		#region Private Methods
		
		private void Send()
		{
			Interlocked.Increment(ref mActive_);
			
			MethodInvoker method = delegate 
			{
				IOController.MessageSent();	
			};
			Invoke(method);

			Start();            
		}
		
		private void Start()
		{
			if(mThread_ == null)
			{
				mThread_ = new Thread(new ThreadStart(Run));
            	mThread_.Name = mRequest_.GetType().Name;
            	mThread_.IsBackground = true;
			}
			
			mThread_.Start();
		}
		
		private void Run()
		{
			MethodInvoker method = delegate
			{
				IOController.Status(MessageStatus.Processing);
			};
			Invoke(method);
			
			#if DEBUG
                Debug.WriteLine(mRequest_.Write(mIOController_.SerializationType));
            #endif
			
            ResponseMessage response = IOController.ServerLocation.Equals(ServerLocation.Local) ? MessageProcessor.Process(mRequest_) : SendToServer();

			#if DEBUG
                Debug.WriteLine(response.Write(mIOController_.SerializationType));               
            #endif
                
            if(HandleErrors(response.ErrorMessages))
            {
            	// No errors in the message; call the successfulHandler
	            if (!response.HasErrors && !response.HasUnconfirmed && mSuccessHandler_ != null)
	            {
	            	method = delegate 
	            	{ 
	            		mSuccessHandler_(response); 
	            	};
            		Invoke(method);
	            }
            }
            
            // If errors in the message; call errorHandler            
            if ((response.HasErrors || response.HasUnconfirmed) && mErrorHandler_ != null)
            {
            	method = delegate 
            	{ 
            		mErrorHandler_(response); 
            	};
            	Invoke(method);
            }
			
			Done();
		}
		
		private Boolean HandleErrors(ErrorMessages errorMessages)
		{
			Boolean continueProcessing = true;
			
			foreach(ErrorMessage errorMessage in errorMessages)				
            {
                MethodInvoker method = delegate {};
                
                if (errorMessage.ErrorLevel.Equals(ErrorLevel.Error))
                {
                	method = delegate
        			{
                		IOController.Status(MessageStatus.Error);
	                    IOController.Error(errorMessage);
                	};
                	Invoke(method);
                }
                else if (errorMessage.ErrorLevel.Equals(ErrorLevel.Warning))
                {
                	method = delegate
        			{
                		IOController.Status(MessageStatus.Warning);
                    	IOController.Warning(errorMessage);
                	};
                	Invoke(method);
                }
                else if (errorMessage.ErrorLevel.Equals(ErrorLevel.Information))
                {
                	method = delegate
        			{
                		IOController.Status(MessageStatus.Information);
                    	IOController.Info(errorMessage);
                	};
                	Invoke(method);
                }
                else if (errorMessage.ErrorLevel.Equals(ErrorLevel.Confirmation))
                {
                    if (errorMessage.Confirmed.HasValue && !errorMessage.Confirmed.Value)
                    {
                    	Boolean confirmed = false;
                    	method = delegate
        				{
                    		IOController.Status(MessageStatus.Confirmation);
                    		confirmed = IOController.Confirm(errorMessage);
                    	};     
                    	Invoke(method);
                    	
                        if (confirmed)
                        {
                            errorMessage.Confirmed = true;
                            
                            if(mRequest_.Confirmations == null)
                            {
                                mRequest_.Confirmations = new ErrorMessages();
                            }
                            
                            mRequest_.Confirmations.Add(errorMessage);
                            ServerConnection.SendRequest(mRequest_, mSuccessHandler_, mErrorHandler_);
                            continueProcessing = false;
                            break;
                        }
                        else
                        {
                        	method = delegate
        					{
                				IOController.Status(MessageStatus.Error);
                        	};
                        	Invoke(method);
                            break;
                        }
                    }
                }               
            }
			
			return continueProcessing;
		}
		
		private void Done()
		{
			Interlocked.Decrement(ref mActive_);
			
			MethodInvoker method = delegate
        	{
            	IOController.Status(mActive_ == 0 ? MessageStatus.Success : MessageStatus.Processing);
				IOController.MessageReceived();
        	};			
			Invoke(method);
		}
		
		private void Invoke(MethodInvoker method)
		{
			Control control = IOController as System.Windows.Forms.Control;
			
			try
			{
	    		if(control != null && control.InvokeRequired)
	    		{
	    			control.Invoke(method);
	    		}
	    		else
	    		{
	    			method.Invoke();
	    		}
			}
			catch(Exception e)
			{
				MethodInvoker exceptionMethod = delegate
    			{
					IOController.Exception(method.Target, new ThreadExceptionEventArgs(e));
            	};
				Invoke(exceptionMethod);
			}
		}

        private ResponseMessage SendToServer()
        {   
            ResponseMessage responseMessage = new ResponseMessage();

            try
            {
            	HttpWebRequest webRequest = (HttpWebRequest)WebRequest.Create(Url);
                webRequest.Proxy.Credentials = CredentialCache.DefaultNetworkCredentials;
                webRequest.Method = "POST";
                webRequest.KeepAlive = true;
                webRequest.ContentType = "application/x-www-form-urlencoded";
            	
            	using(StreamWriter writer = new StreamWriter(webRequest.GetRequestStream()))
                {
            		writer.Write("message=" + HttpUtility.UrlEncode(mRequest_.Write(mIOController_.SerializationType)) + "&serializationType=" + mIOController_.SerializationType.Value);
                }

				using(StreamReader reader = new StreamReader(((HttpWebResponse)webRequest.GetResponse()).GetResponseStream()))
				{
					responseMessage = (ResponseMessage)CoreMessage.Read(reader.ReadToEnd(), mIOController_.SerializationType);
                }                
            }
            catch(Exception ex)
            {
            	if(ex is TargetInvocationException)
            	{
            		ex = ex.InnerException;
            	}
            	
            	responseMessage = new ResponseMessage();
            	responseMessage.Status = ErrorLevel.Error;
            	responseMessage.Errors.Add(new ErrorMessage(ex));
            	
                while(ex.InnerException != null)
                {
                    ex = ex.InnerException;
                    responseMessage.Errors.Add(new ErrorMessage(ex));
                }
            }
            
            return responseMessage;
        }
		
		#endregion
		
		#region Public Methods

        public static void SendRequest(RequestMessage request, MessageCompleteHandler successHandler)
        {
            SendRequest(request, successHandler, null);
        }

        public static void SendRequest(RequestMessage request, MessageCompleteHandler successHandler, MessageCompleteHandler errorHandler)
        {
        	new ServerConnection(request, successHandler, errorHandler).Send();
        }

        #endregion
	}
}
