using System;
using MiskoPersist.Data;
using MiskoPersist.Interfaces;
using MiskoPersist.Message.Request;
using MiskoPersist.Message.Response;

namespace MiskoPersist.Core
{
	public class OnlineConnectionProvider : ConnectionProvider
	{
		public OnlineConnectionProvider()
		{
		}

		#region ConnectionProvider implementation

		public ResponseMessage Send(RequestMessage request)
		{
			ResponseMessage response = MessageProcessor.Process(AbstractData.SerializeJson(request));
			
			return (ResponseMessage)AbstractData.DeserializeJson(AbstractData.SerializeJson(response));
		}

		#endregion
	}
}
