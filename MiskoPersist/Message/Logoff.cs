using System;
using log4net;
using MiskoPersist.Core;
using MiskoPersist.Message.Requests;
using MiskoPersist.Message.Responses;

namespace MiskoPersist.Message
{
	public class Logoff : MessageWrapper
	{
		private static ILog Log = LogManager.GetLogger(typeof(Logoff));
		
		#region Properties

        public new LogoffRQ Request { get { return (LogoffRQ)base.Request; } }
        public new LogoffRS Response  { get { return (LogoffRS)base.Response; } }
		
        #endregion

        public Logoff(LogoffRQ request, LogoffRS response) 
        	: base(request, response)
        {
        }

        public override void Execute(Session session)
        {
        }
	}
}
