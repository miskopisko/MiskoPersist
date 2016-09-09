using System;
using MiskoPersist.Core;
using MiskoPersist.Interfaces;
using MiskoPersist.Serialization;

namespace MiskoPersist.Enums
{
    public class SerializationType : MiskoEnum
	{
		#region Fields

		private static readonly SerializationType mNULL_ = new SerializationType(-1, "", "");
		private static readonly SerializationType mXml_ = new SerializationType(0, "", "Xml");
		private static readonly SerializationType mJson_ = new SerializationType(1, "", "Json");

		private static readonly SerializationType[] mElements_ = new[]
		{
			mNULL_, mXml_, mJson_
		};

		#endregion

		#region Parameters

		public static SerializationType[] Elements { get { return mElements_; } }
		public static SerializationType NULL { get { return mNULL_; } }
		public static SerializationType Xml { get { return mXml_; } }
		public static SerializationType Json { get { return mJson_; } }

		#endregion

		#region Constructors

		public SerializationType()
		{
		}

		public SerializationType(Int64 value, String code, String description)
			: base(value, code, description)
		{
		}

		#endregion

		#region Helpers

        public static SerializationType FromHttpContentType(String contentType)
		{
			if (contentType.Equals("application/xml"))
			{
				return SerializationType.Xml;
			}
			if (contentType.Equals("application/json"))
			{
				return SerializationType.Json;
			}
			throw new MiskoException("Invalid or unsupported HTTP content type");
		}
		
		public String ToHttpContentType()
		{
			if (Equals(SerializationType.Xml))
			{
				return "application/xml";
			}
			if (Equals(SerializationType.Json))
			{
				return "application/json";
			}
			throw new MiskoException("Unknown HTTP content type");
		}

        public IMiskoFormatter GetFormatter()
		{
			if (Equals(Xml))
			{
				return new XmlFormatter();
			}
			if (Equals(Json))
			{
				return new JsonFormatter();
			}
			throw new MiskoException("Unknown serialization type");
		}

		#endregion
	}
}
