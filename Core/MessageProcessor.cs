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
			if (request == null)
			{
				throw new ArgumentNullException("request");	
			}
			
			Session session = null;
            ResponseMessage response = new ResponseMessage();
			Stopwatch stopwatch = Stopwatch.StartNew();
			
			try
			{
                response = (ResponseMessage)Activator.CreateInstance(request.ResponseClass);
				MessageWrapper wrapper = (MessageWrapper)Activator.CreateInstance(request.WrapperClass, BindingFlags.CreateInstance, null, new Object[] { request, response }, null, null);

                session = new Session(request);
				session.BeginTransaction();
				wrapper.GetType().InvokeMember(request.Command ?? "Execute", BindingFlags.Default | BindingFlags.InvokeMethod, null, wrapper, new Object[] { session });				
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
					// Ignore the generic exception thrown from session.Error
                    if (!(((MiskoException)ActualException).Class.Name.Equals("Session") && ((MiskoException)ActualException).Method.Name.Equals("Error")))
					{
						Log.Error("Unexpected Error: (" + ActualException.Message + ")", ActualException);
						session.ErrorMessages.Add(((MiskoException)ActualException).ErrorMessage);
					}
				}
				else
				{
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
					
					if (session.Connection != null && !session.Connection.State.Equals(ConnectionState.Closed))
					{
						session.Connection.Close();
					}

                    stopwatch.Stop();
                    Log.Debug(String.Format("{0} execution time : {1}", response.WrapperClass.Name, stopwatch.Elapsed));
                    Log.Debug(String.Format("{0} SQL execution time : {1}", response.WrapperClass.Name, session.SqlExecutionTime));
				}
            }
			
			return response;
		}

		#endregion
	}
}