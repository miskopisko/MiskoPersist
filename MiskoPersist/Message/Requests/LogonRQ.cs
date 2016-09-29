using System;
using log4net;
using MiskoPersist.Attributes;

namespace MiskoPersist.Message.Requests
{
	public class LogonRQ : RequestMessage
	{
		private static ILog Log = LogManager.GetLogger(typeof(LogonRQ));
		
		#region Parameters

		[Viewed]
		public String Username { get; set; }
		[Viewed]
		public String Password { get; set; }

		#endregion
	}
}
