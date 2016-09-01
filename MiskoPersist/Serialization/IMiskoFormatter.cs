using System;
using System.IO;

namespace MiskoPersist.Serialization
{
	public interface IMiskoFormatter
	{
		void Serialize(Stream serializationStream, Object graph);
		Object Deserialize(Stream serializationStream);
	}
}