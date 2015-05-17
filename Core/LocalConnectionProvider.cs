using System;
using MiskoPersist.Interfaces;
using MiskoPersist.Message.Request;
using MiskoPersist.Message.Response;

namespace MiskoPersist.Core
{
	public class LocalConnectionProvider : ConnectionProvider
	{
		public LocalConnectionProvider()
		{
		}

		#region ConnectionProvider implementation

		public ResponseMessage Send(RequestMessage request)
		{
			return MessageProcessor.Process(request);
		}

		#endregion
	}
}
