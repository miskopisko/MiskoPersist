using log4net;
using MiskoPersist.Core;
using MiskoPersist.Message.Request;
using MiskoPersist.Message.Response;

namespace MiskoPersist.Message
{
	public abstract class MessageWrapper
	{
		private static ILog Log = LogManager.GetLogger(typeof(MessageWrapper));

		#region Fields

		

		#endregion

		#region Properties

		public RequestMessage Request
		{ 
			get;
			private set;
		}
		
		public ResponseMessage Response
		{
			get;
			private set;
		}

		#endregion

		#region Constructor

		protected MessageWrapper(RequestMessage request, ResponseMessage response)
		{
			Request = request;
			Response = response;
		}

		#endregion

		public abstract void Execute(Session session);
	}
}