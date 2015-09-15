using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Threading;
using System.Web;
using System.Windows.Forms;
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
		
		private static int mActive_ = 0;
		private static IOController mIOController_;
		
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
                Debug.WriteLine(mRequest_.Serialize());
            #endif
			
            ResponseMessage response = null;
            if(IOController.ServerLocation.Equals(ServerLocation.Local))
            {
                response = MessageProcessor.Process(mRequest_);
            }
            else
            {
                response = SendToServer();
            }

			#if DEBUG
                Debug.WriteLine(response.Serialize());
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
                    	bool confirmed = false;
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
            String url = IOController.UseSSL ? "https://" : "http://";
            url += IOController.Host;
            url += IOController.Port != 80 ? ":" + IOController.Port : "";
            url += IOController.Script;

            ResponseMessage responseMessage = null;

            try
            {
            	HttpWebRequest httpRequest = (HttpWebRequest)WebRequest.Create(url);                
                httpRequest.Proxy.Credentials = CredentialCache.DefaultNetworkCredentials;
                httpRequest.Method = "POST";
                httpRequest.KeepAlive = true;
                httpRequest.ContentType = "application/x-www-form-urlencoded";

                using(StreamWriter writer = new StreamWriter(httpRequest.GetRequestStream()))
                {
                    String postData = "request=" + HttpUtility.UrlEncode(mRequest_.Serialize());
                    writer.Write(postData);
                }

				using (StreamReader reader = new StreamReader(((HttpWebResponse)httpRequest.GetResponse()).GetResponseStream()))
				{ 
					responseMessage = ResponseMessage.Deserialize(reader.ReadToEnd());
                }                
            }
            catch(Exception ex)
            {
                while(ex.InnerException != null)
                {
                    ex = ex.InnerException;
                }

                responseMessage = new ResponseMessage();
                responseMessage.Status = ErrorLevel.Error;
                responseMessage.Errors.Add(new ErrorMessage(ex));
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
