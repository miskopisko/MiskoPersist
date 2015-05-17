using System;
using System.Net.Mime;
using System.Reflection;
using System.Threading;
using System.Windows.Forms;
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

        public delegate void MessageCompleteHandler(ResponseMessage Response);

        #endregion
		
		#region Fields
		
		private static int mActive_ = 0;
		
		private Thread mThread_;
		private readonly MessageCompleteHandler mSuccessHandler_;
        private readonly MessageCompleteHandler mErrorHandler_;
        private readonly RequestMessage mRequest_;
		
		#endregion
		
		#region Properties
		
		public static IOController IOController 
		{
			get;
			set;
		}
		
		#endregion
		
		public ServerConnection(RequestMessage request, MessageCompleteHandler successHandler, MessageCompleteHandler errorHandler)
		{
			if (IOController == null)
            {
                throw new MiskoException(ErrorStrings.errIOControllerIsNull);
            }

			mRequest_ = request;
            mSuccessHandler_ = successHandler;
            mErrorHandler_ = errorHandler;           
		}
		
		#region Private Methods
		
		private static void Send(ServerConnection serverConnection)
		{
			Interlocked.Increment(ref mActive_);
			
			IOController.MessageSent();			
			IOController.Debug(serverConnection.mRequest_);

			serverConnection.Start();            
		}
		
		private void Start()
		{
			if(mThread_ == null)
			{
				mThread_ = new Thread(new ThreadStart(Run));
            	mThread_.Name = mRequest_.GetType().Name;
            	mThread_.IsBackground = true;
            	mThread_.Start();
			}
		}
		
		private void Run()
		{
			ResponseMessage response = null;
			
			ConnectionProvider connectionProvider = ServerConnection.IOController.GetConnectionProvider();			
			
			IOController.Status(Strings.strPorcessing);
			
			response = connectionProvider.Send(mRequest_);
				
			IOController.Debug(response);
                
            if(HandleErrors(response.AllMessages))
            {
            	// No errors in the message; call the successfulHandler
	            if (!response.HasErrors && !response.HasUnconfirmed && mSuccessHandler_ != null)
	            {
					Control control = mSuccessHandler_.Target as System.Windows.Forms.Control;
	            	if(control != null && control.InvokeRequired)
	            	{
	            		control.BeginInvoke(mSuccessHandler_, response);
	            	}
	            	else
	            	{
	            		mSuccessHandler_(response);
	            	}
	            }
            }
            
            // If errors in the message; call errorHandler            
            if ((response.HasErrors || response.HasUnconfirmed) && mErrorHandler_ != null)
            {
				Control control = mErrorHandler_.Target as System.Windows.Forms.Control;
            	if(control != null && control.InvokeRequired)
            	{
            		control.BeginInvoke(mErrorHandler_, response);	
            	}
            	else
            	{
            		mErrorHandler_(response);
            	}
            }          
			
			Done();
		}
		
		private Boolean HandleErrors(ErrorMessages errorMessages)
		{
			Boolean continueProcessing = true;
			
			for (int i = 0; i < errorMessages.Count; i++)
            {
                ErrorMessage errorMessage = errorMessages[i];

                if (errorMessage.ErrorLevel.Equals(ErrorLevel.Error))
                {
                	IOController.Status(Strings.strError);
                    IOController.Error(errorMessage.Message);
                }
                else if (errorMessage.ErrorLevel.Equals(ErrorLevel.Warning))
                {
                	IOController.Status(Strings.strWarning);
                    IOController.Warning(errorMessage.Message);
                }
                else if (errorMessage.ErrorLevel.Equals(ErrorLevel.Info))
                {
                	IOController.Status(Strings.strInformation);
                    IOController.Info(errorMessage.Message);
                }
                else if (errorMessage.ErrorLevel.Equals(ErrorLevel.Confirmation))
                {
                    if (errorMessage.Confirmed.HasValue && !errorMessage.Confirmed.Value)
                    {
                    	IOController.Status(Strings.strConfirm);
                    	
                        if (IOController.Confirm(errorMessage.Message))
                        {
                            errorMessage.Confirmed = true;
                            
                            if(mRequest_.Confirms == null)
                            {
                            	mRequest_.Confirms = new ErrorMessages();
                            }
                            
                            mRequest_.Confirms.Add(errorMessage);
                            ServerConnection.SendRequest(mRequest_, mSuccessHandler_, mErrorHandler_);
                            continueProcessing = false;
                            break;
                        }
                        else
                        {
                        	IOController.Status(Strings.strError);
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
			
			IOController.Status(mActive_ == 0 ? Strings.strSuccess : Strings.strPorcessing);			
			IOController.MessageReceived();
		}
		
		#endregion
		
		#region Public Methods

        public static void SendRequest(RequestMessage request, MessageCompleteHandler successHandler)
        {
            SendRequest(request, successHandler, null);
        }

        public static void SendRequest(RequestMessage request, MessageCompleteHandler successHandler, MessageCompleteHandler errorHandler)
        {
        	ServerConnection.Send(new ServerConnection(request, successHandler, errorHandler));
        }

        #endregion
	}
}
