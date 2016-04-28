using System;
using System.Data;
using System.Diagnostics;
using System.Reflection;
using log4net;
using MiskoPersist.Data;
using MiskoPersist.Enums;
using MiskoPersist.Message;
using MiskoPersist.Message.Request;
using MiskoPersist.Message.Response;

namespace MiskoPersist.Core
{
	public static class MessageProcessor
	{
		private static ILog Log = LogManager.GetLogger(typeof(MessageProcessor));

		#region Public Methods

		public static ResponseMessage Process(RequestMessage request)
		{
			if(request != null)
			{
				ResponseMessage response = new ResponseMessage();
				Session session = new Session();
				TimeSpan messageExecutionTime = TimeSpan.Zero;
					
				try
				{
					response = (ResponseMessage)Activator.CreateInstance(request.ResponseClass);				
					MessageWrapper wrapper = (MessageWrapper)Activator.CreateInstance(request.WrapperClass, BindingFlags.CreateInstance, null, new Object[] { request, response }, null, null);

					session.Connection = ServiceLocator.GetConnection(request.Connection ?? "Default");
					session.MessageMode = request.MessageMode ?? MessageMode.Normal;
					session.ErrorMessages.Concatenate(request.Confirmations);
					session.BeginTransaction();

					Stopwatch watch = Stopwatch.StartNew();
					wrapper.GetType().InvokeMember(request.Command ?? "Execute", BindingFlags.Default | BindingFlags.InvokeMethod, null, wrapper, new Object[] { session });
					watch.Stop();
					messageExecutionTime = watch.Elapsed;
				}
				catch (TargetInvocationException e)
				{
					session.Status = ErrorLevel.Error;

					Exception ActualException = e;

					while (ActualException.InnerException != null)
					{
						ActualException = ActualException.InnerException;
					}

					if(ActualException is MiskoException)
					{
						MiskoException ex = (MiskoException)ActualException;

						// Ignore the generic exception thrown from session.Error
						if (!(ex.Class.Name.Equals("Session") && ex.Method.Name.Equals("Error")))
						{
							Log.Error("Unexpected Error: (" + ex.Message + ")", ex);
							session.ErrorMessages.Add(ex.ErrorMessage);
							
							#if DEBUG
								Debug.WriteLine(ActualException.StackTrace);
							#endif                   
						}
					}
					else
					{
						session.Status = ErrorLevel.Error;
						
						Log.Error("Unexpected Error: (" + ActualException.Message + ")", ActualException);
						session.ErrorMessages.Add(new ErrorMessage(ActualException));
						
						#if DEBUG
							Debug.WriteLine(ActualException.StackTrace);
						#endif
					}
				}
				catch(Exception e)
				{
					Log.Error("Unexpected Error: (" + e.Message + ")", e);
					session.ErrorMessages.Add(new ErrorMessage(e));
						
					#if DEBUG
						Debug.WriteLine(e.StackTrace);
					#endif
				}
				finally
				{
					if(session != null)
					{
						session.EndTransaction();
						session.FlushPersistence();

						response.Status = session.Status;
						response.ErrorMessages = session.ErrorMessages;
						response.MessageExecutionTime = messageExecutionTime;
						response.SqlExecutionTime = session.SqlExecutionTime;
						
						if(session.Connection != null && !session.Connection.State.Equals(ConnectionState.Closed))
						{
							session.Connection.Close();
						}                        
					}
				}
				
				return response;
			}

			return new ResponseMessage();
		}

		#endregion        
	}
}