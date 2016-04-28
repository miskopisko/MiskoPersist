using System;
using log4net;
using Message;
using MiskoPersist.Attributes;
using MiskoPersist.Enums;

namespace MiskoPersist.Message.Request
{
	public class RequestMessage : CoreMessage
	{
		private static ILog Log = LogManager.GetLogger(typeof(RequestMessage));

		#region Fields

		

		#endregion

		#region Viewed Properties

		[Viewed]
		public MessageMode MessageMode { get; set; }
		[Viewed]
		public String Command  { get; set; }
		[Viewed]
		public String Connection  { get; set; }

		#endregion
	}
}
