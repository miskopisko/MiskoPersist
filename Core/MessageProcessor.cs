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
			if (request != null)
			{
				ResponseMessage response = new ResponseMessage();
				Session session = null;
				
				Stopwatch stopwatch = new Stopwatch();
				
				try
				{
					session = new Session(request);
					session.BeginTransaction();
					
					response = (ResponseMessage)Activator.CreateInstance(request.ResponseClass);
                    response.SerializationType = request.SerializationType;
					MessageWrapper wrapper = (MessageWrapper)Activator.CreateInstance(request.WrapperClass, BindingFlags.CreateInstance, null, new Object[] { request, response }, null, null);
					
					stopwatch.Start();
					wrapper.GetType().InvokeMember(request.Command ?? "Execute", BindingFlags.Default | BindingFlags.InvokeMethod, null, wrapper, new Object[] { session });
					stopwatch.Stop();
				}
				catch (TargetInvocationException e)
				{
					session.Status = ErrorLevel.Error;

					Exception ActualException = e;

					while (ActualException.InnerException != null)
					{
						ActualException = ActualException.InnerException;
					}

					if (ActualException is MiskoException)
					{
						MiskoException ex = (MiskoException)ActualException;

						// Ignore the generic exception thrown from session.Error
						if (!(ex.Class.Name.Equals("Session") && ex.Method.Name.Equals("Error")))
						{
							Log.Error("Unexpected Error: (" + ex.Message + ")", ex);
							session.ErrorMessages.Add(ex.ErrorMessage);
						}
					}
					else
					{
						session.Status = ErrorLevel.Error;
						
						Log.Error("Unexpected Error: (" + ActualException.Message + ")", ActualException);
						session.ErrorMessages.Add(new ErrorMessage(ActualException));
					}
				}
				catch (Exception e)
				{
					Log.Error("Unexpected Error: (" + e.Message + ")", e);
					response.Status = ErrorLevel.Error;
					
					while (e != null)
					{
						response.Errors.Add(new ErrorMessage(e));
						e = e.InnerException;
					}
				}
				finally
				{
					if (session != null)
					{
						session.EndTransaction();
						session.FlushPersistence();

						response.Status = session.Status;
						response.ErrorMessages = session.ErrorMessages;
						
						#if DEBUG
						response.MessageExecutionTime = stopwatch.Elapsed;
						response.SqlExecutionTime = session.SqlExecutionTime;
						#endif
						
						if (session.Connection != null && !session.Connection.State.Equals(ConnectionState.Closed))
						{
							session.Connection.Close();
						}
					}
				}
				
				return response;
			}

			throw new ArgumentNullException();
		}

		#endregion
	}
}