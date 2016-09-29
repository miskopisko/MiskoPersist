using System;
using System.Diagnostics;
using System.Reflection;
using log4net;
using MiskoPersist.Data.Viewed;
using MiskoPersist.Enums;
using MiskoPersist.Message;
using MiskoPersist.Message.Requests;
using MiskoPersist.Message.Responses;

namespace MiskoPersist.Core
{
	public static class MessageProcessor
	{
		private static readonly ILog Log = LogManager.GetLogger(typeof(MessageProcessor));

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
				session = new Session(DatabaseConnections.GetConnection(request.Connection));
                response = (ResponseMessage)Activator.CreateInstance(request.ResponseClass);
                MessageWrapper wrapper = (MessageWrapper)Activator.CreateInstance(request.WrapperClass, request, response);
                
				session.BeginTransaction(request);
				wrapper.Execute(session);
			}
            catch(Exception ex)
            {   
                response.Status = ErrorLevel.Error;

                do
                {
                    // Ignore TargetInvocationException and the generic exception thrown from session.Error
                    if(ex is TargetInvocationException || (ex is MiskoException && (((MiskoException)ex).Class.Name.Equals("Session") && ((MiskoException)ex).Method.Name.Equals("Error"))))
                    {
                        ex = ex.InnerException;
                        continue;
                    }
                    
                    response.Errors.Add(new ErrorMessage(ex));
                    
					Log.Error(ex);
                    ex = ex.InnerException;
                }
                while (ex != null);
            }
			finally
			{
				stopwatch.Stop();
				Log.Debug(String.Format("{0} execution time : {1}", response.GetType().Name, stopwatch.Elapsed));
				Log.Debug(String.Format("{0} SQL execution time : {1}", response.GetType().Name, session != null ? session.SqlExecutionTime : TimeSpan.Zero));
                
				if (session != null)
				{
					response.Status = session.Status;
					response.Errors.Add(session.ErrorMessages.ListOf(ErrorLevel.Error));
					response.Warnings.Add(session.ErrorMessages.ListOf(ErrorLevel.Warning));
					response.Infos.Add(session.ErrorMessages.ListOf(ErrorLevel.Information));
					response.Confirmations.Add(session.ErrorMessages.ListOf(ErrorLevel.Confirmation));
					session.Dispose();
				}
            }
			
			return response;
		}

		#endregion
	}
}