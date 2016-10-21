using System;
using log4net;
using MiskoPersist.Attributes;
using MiskoPersist.Enums;

namespace MiskoPersist.Message.Requests
{
	public class RequestMessage : CoreMessage
	{
		private static ILog Log = LogManager.GetLogger(typeof(RequestMessage));

		#region Fields

		

		#endregion

		#region Viewed Properties

		[Viewed]
		public MessageMode MessageMode 
		{
			get;
			set; 
		}
		
		[Viewed]
		public String Command
		{
			get;
			set; 
		}
		
		[Viewed]
		public String Connection
		{
			get;
			set; 
		}

		#endregion

		#region Other Properties

		public virtual Boolean SecurityExempt
		{
			get
			{
				return false;
			}
		}

		#endregion
	}
}


