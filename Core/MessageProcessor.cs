using System;
using System.Diagnostics;
using System.Reflection;
using MiskoPersist.Data;
using MiskoPersist.Enums;
using MiskoPersist.Message;
using MiskoPersist.Message.Request;
using MiskoPersist.Message.Response;

namespace MiskoPersist.Core
{
    public class MessageProcessor
    {
        private static readonly Logger Log = Logger.GetInstance(typeof(MessageProcessor));

        #region Constructors

        private MessageProcessor()
        {            
        }

        #endregion

        #region Private Methods
        
        public static ResponseMessage Process(String request)
        {
        	return Process((RequestMessage)AbstractData.DeserializeJson(request));
        }

        public static ResponseMessage Process(RequestMessage request)
        {
            if (request != null)
            {
                ResponseMessage response = null;
                MessageWrapper wrapper = null;
                DateTime startTime = DateTime.Now;
                Session session = null;

                try
                {
                	session = new Session(request.Connection ?? "Default");
					session.MessageMode = request.MessageMode ?? MessageMode.Normal;
					session.ErrorMessages.AddRange(request.Confirms);
                	
                    String msgName = request.GetType().Name.Substring(0, request.GetType().Name.Length - 2);
                    String msgPath = request.GetType().FullName.Replace("Requests." + msgName + "RQ", "");

                    response = (ResponseMessage)request.GetType().Assembly.CreateInstance(msgPath + "Responses." + msgName + "RS");
                    wrapper = (MessageWrapper)request.GetType().Assembly.CreateInstance(msgPath + msgName, false, BindingFlags.CreateInstance, null, new object[] { request, response }, null, null);

                    response.Page = request.Page;

                    session.BeginTransaction();

                    Stopwatch watch = Stopwatch.StartNew();
                    wrapper.GetType().InvokeMember(request.Command ?? "Execute", BindingFlags.Default | BindingFlags.InvokeMethod, null, wrapper, new Object[] { session });
                    watch.Stop();
        			long elapsedMs = watch.ElapsedMilliseconds;
                }
                catch (Exception e)
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
                            
                            #if DEBUG
								Trace.Write(ActualException.StackTrace);
							#endif                   
                        }
                    }
                    else
                    {
                        Log.Error("Unexpected Error: (" + ActualException.Message + ")", ActualException);
                        session.ErrorMessages.Add(new ErrorMessage(ActualException));
                        
                        #if DEBUG
							Trace.Write(ActualException.StackTrace);
						#endif
                    }
                }
                finally
                {
                    if (session != null)
                    {
                        session.EndTransaction();
                        session.FlushPersistence();

                        if (response != null)
                        {
                            response.Status = session.Status;
                            response.Errors = session.ErrorMessages.ListOf(ErrorLevel.Error);
                            response.Infos = session.ErrorMessages.ListOf(ErrorLevel.Info);
                            response.Warnings = session.ErrorMessages.ListOf(ErrorLevel.Warning);
                            response.Confirmations = session.ErrorMessages.ListOf(ErrorLevel.Confirmation);
                        }
                        
                        session.Connection.Close();
                    }
                }
                
                return response;
            }

            return new ResponseMessage();
        }

        #endregion        
    }
}