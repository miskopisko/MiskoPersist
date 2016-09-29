using System;
using log4net;
using MiskoPersist.Core;
using MiskoPersist.Data.Stored;
using MiskoPersist.Enums;
using MiskoPersist.Message.Requests;
using MiskoPersist.Message.Responses;

namespace MiskoPersist.Message
{
	public class Logon : MessageWrapper
	{
		private static ILog Log = LogManager.GetLogger(typeof(Logon));
		
		#region Fields
		
		public Operator mOperator_ = new Operator();
		
		#endregion
		
		#region Properties

        public new LogonRQ Request { get { return (LogonRQ)base.Request; } }
        public new LogonRS Response  { get { return (LogonRS)base.Response; } }
		
        #endregion

        public Logon(LogonRQ request, LogonRS response) 
        	: base(request, response)
        {
        }

        public override void Execute(Session session)
        {
        	mOperator_.FetchByUsername(session, Request.Username);
        	
        	if(mOperator_.IsSet)
        	{
        		Response.SessionToken = mOperator_.Logon(session, Request.Password);
        	}
 			else
            {
                session.Error(ErrorLevel.Error, "Invalid username or password. Please try again.");
            }
        }
	}
}
