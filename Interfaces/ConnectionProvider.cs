using System;
using MiskoPersist.Message.Request;
using MiskoPersist.Message.Response;

namespace MiskoPersist.Interfaces
{
	public interface ConnectionProvider
	{
		ResponseMessage Send(RequestMessage request);
	}
}
