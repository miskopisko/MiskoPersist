using System;
using System.IO;

namespace MiskoPersist.Interfaces
{
	public interface IMiskoFormatter
	{
		void Serialize(Stream serializationStream, Object graph, Boolean indent);
		Object Deserialize(Stream serializationStream);
	}
}