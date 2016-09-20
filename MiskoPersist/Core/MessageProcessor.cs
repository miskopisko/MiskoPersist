using System;
using System.Data;
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
				session = new Session(DatabaseConnections.GetDatabaseConnection(request.Connection));
                response = (ResponseMessage)Activator.CreateInstance(request.ResponseClass);
                MessageWrapper wrapper = (MessageWrapper)Activator.CreateInstance(request.WrapperClass, request, response);
                
				session.BeginTransaction(request);
				wrapper.Execute(session);
			}
            catch(Exception ex)
            {   
                if (session != null) 
                {
					session.Status = ErrorLevel.Error;
                }
                else
                {
                	response.Status = ErrorLevel.Error;
                }

                do
                {
                    // Ignore TargetInvocationException and the generic exception thrown from session.Error
                    if(ex is TargetInvocationException || (ex is MiskoException && (((MiskoException)ex).Class.Name.Equals("Session") && ((MiskoException)ex).Method.Name.Equals("Error"))))
                    {
                        ex = ex.InnerException;
                        continue;
                    }
                    
                    if (session != null) 
                    {
                    	session.ErrorMessages.Add(new ErrorMessage(ex));	
                    }
                    else
                    {
                    	response.Errors.Add(new ErrorMessage(ex));
                    }
                    
					Log.Error(ex);
                    ex = ex.InnerException;
                }
                while (ex != null);
            }
			finally
			{
				if (session != null) 
				{
					session.FlushPersistence();
					session.EndTransaction();
						
					response.Status = session.Status;
	                response.ErrorMessages = session.ErrorMessages;
	                
					if (session.Connection != null && !session.Connection.State.Equals(ConnectionState.Closed))
	                {
	                    session.Connection.Close();
	                }	
				}
				
				stopwatch.Stop();
				Log.Debug(String.Format("{0} execution time : {1}", response.GetType().Name, stopwatch.Elapsed));
                Log.Debug(String.Format("{0} SQL execution time : {1}", response.GetType().Name, session != null ? session.SqlExecutionTime : TimeSpan.Zero));
            }
			
			return response;
		}

		#endregion
	}
}