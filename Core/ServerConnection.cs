using System;
using System.Diagnostics;
using System.Threading;
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
		
		private ConnectionProvider ConnectionProvider
		{
			get
			{
				if(IOController.DataSource.Equals(DataSource.Local))
				{
					return new LocalConnectionProvider();
				}
				else if(IOController.DataSource.Equals(DataSource.Online))
				{
					return new OnlineConnectionProvider();
				}
				else
				{
					throw new MiskoException("Unknown connection provider");
				}
			}
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
            	mThread_.Start();
			}
		}
		
		private void Run()
		{
			MethodInvoker method = delegate
			{
				IOController.Status(MessageStatus.Processing);
			};
			Invoke(method);
			
			#if DEBUG
				Debug.WriteLine(AbstractData.SerializeJson(mRequest_));
            #endif
			
			ResponseMessage response = ConnectionProvider.Send(mRequest_);
				
			#if DEBUG
				Debug.WriteLine(AbstractData.SerializeJson(response));
            #endif
                
            if(HandleErrors(response.AllMessages))
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
                else if (errorMessage.ErrorLevel.Equals(ErrorLevel.Info))
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
        	if(control != null && control.InvokeRequired)
        	{
        		control.Invoke(method);
        	}
        	else
        	{
        		method.Invoke();
        	}
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
