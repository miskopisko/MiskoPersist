using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using log4net;
using MiskoPersist.Attributes;
using MiskoPersist.Core;
using MiskoPersist.Data.Viewed;
using MiskoPersist.Enums;
using MiskoPersist.MoneyType;

namespace MiskoPersist.Serialization
{
    public static class Serializer
	{
		private static ILog Log = LogManager.GetLogger(typeof(Serializer));

		#region Fields
		
		private static readonly Encoding mEncoding_ = new UTF8Encoding(false);
		
		#endregion
		
		#region Properties
		
		public static Encoding Encoding
		{
			get
			{
				return mEncoding_;
			}
		}
		
		public static String DateFormat
		{
			get
			{
				return "yyyyMMddHHmmss";
			}
		}
		
		public static String MoneyFormat
		{
			get
			{
				return "F4";
			}
		}
		
		#endregion
		
		#region Public Static Methods
		
		public static String Serialize(Object o, SerializationType serializationType, Boolean indent = false)
		{
			if (o == null)
			{
				throw new ArgumentNullException("o");
			}
			
			if (serializationType == null)
			{
				throw new ArgumentNullException("serializationType");
			}

			using (MemoryStream ms = new MemoryStream())
			{
				serializationType.GetFormatter().Serialize(ms, o, indent);
				return Encoding.GetString(ms.ToArray());
			}
		}
		
		public static Object Deserialize(String str)
		{
            using (MemoryStream ms = new MemoryStream(Encoding.GetBytes(str)))
			{
				return Deserialize(ms);
			}
		}
		
		public static Object Deserialize(Stream stream)
		{            
			stream.Position = 0;
			Int32 firstByte = stream.ReadByte();
			stream.Position = 0;
			if (firstByte.Equals(Encoding.GetBytes("<")[0]))
			{
				return SerializationType.Xml.GetFormatter().Deserialize(stream);
			}
			if (firstByte.Equals(Encoding.GetBytes("{")[0]))
			{
				return SerializationType.Json.GetFormatter().Deserialize(stream);
			}
			throw new MiskoException("Unable to determine serialization method");
		}
		
		public static SerializationType GetSerializationType(this String message)
		{
			if (message.StartsWith("<", StringComparison.OrdinalIgnoreCase))
			{
				return SerializationType.Xml;
			}
			if (message.StartsWith("{", StringComparison.OrdinalIgnoreCase))
			{
				return SerializationType.Json;
			}
			throw new MiskoException("Unrecognized string format");
		}
		
		#endregion
		
		#region Internal Methods
		
		internal static IEnumerable<SerializationElement> GetMemberInfo(Object objectToSerialize)
		{
			if (objectToSerialize == null)
			{
				yield break;
			}
			
			if (objectToSerialize.GetType().BaseType.IsGenericType && objectToSerialize.GetType().BaseType.GetGenericTypeDefinition().Equals(typeof(ViewedDataList<>)))
			{
				yield return new SerializationElement(objectToSerialize.GetType().Name, objectToSerialize.GetType().BaseType, objectToSerialize);
			}
			else
			{
				foreach (PropertyInfo property in GetViewedProperties(objectToSerialize))
				{
					Object propertyValue = property.GetValue(objectToSerialize);
					
					if (propertyValue == null)
					{
						continue;
					}
	
					if (((ViewedAttribute)property.GetCustomAttribute(typeof(ViewedAttribute))).ViewedSerializer != null)
					{
						String value = ((ViewedAttribute)property.GetCustomAttribute(typeof(ViewedAttribute))).ViewedSerializer.Invoke(propertyValue);
						yield return new SerializationElement(property.Name, typeof(String), value);
					}
					else if (typeof(MiskoEnum).IsAssignableFrom(property.PropertyType))
					{
						if (((MiskoEnum)propertyValue).IsSet)
						{
							yield return new SerializationElement(property.Name, typeof(Int64), ((MiskoEnum)propertyValue).Value);
						}
					}
					else if (typeof(PrimaryKey).IsAssignableFrom(property.PropertyType) || typeof(PrimaryKey?).IsAssignableFrom(property.PropertyType))
					{
						if (((PrimaryKey)propertyValue).IsSet)
						{
							yield return new SerializationElement(property.Name, typeof(Int64), ((PrimaryKey)propertyValue).Value);
						}
					}
					else if (typeof(DateTime).IsAssignableFrom(property.PropertyType) || typeof(DateTime?).IsAssignableFrom(property.PropertyType))
					{
						if (((DateTime?)propertyValue).HasValue)
						{
							yield return new SerializationElement(property.Name, typeof(String), ((DateTime)propertyValue).ToString(DateFormat));
						}
					}
					else if (typeof(Guid).IsAssignableFrom(property.PropertyType) || typeof(Guid?).IsAssignableFrom(property.PropertyType))
					{
						if (!((Guid?)propertyValue).Equals(Guid.Empty))
						{
							yield return new SerializationElement(property.Name, typeof(String), ((Guid?)propertyValue).Value);
						}
					}
					else if (typeof(Money).IsAssignableFrom(property.PropertyType) || typeof(Money?).IsAssignableFrom(property.PropertyType))
					{
						yield return new SerializationElement(property.Name, typeof(String), ((Money)propertyValue).Value.ToString(MoneyFormat));
					}
					else if (typeof(String).IsAssignableFrom(property.PropertyType))
					{
						if (!String.IsNullOrEmpty((String)propertyValue))
						{
							yield return new SerializationElement(property.Name, property.PropertyType, propertyValue);
						}
					}
					else if (typeof(Decimal).IsAssignableFrom(property.PropertyType) || typeof(Decimal?).IsAssignableFrom(property.PropertyType))
					{
						yield return new SerializationElement(property.Name, typeof(String), ((Decimal)propertyValue).ToString());
					}
					else if (property.PropertyType.IsGenericType && property.PropertyType.GetGenericTypeDefinition().Equals(typeof(Nullable<>)))
					{
						yield return new SerializationElement(property.Name, Nullable.GetUnderlyingType(property.PropertyType), propertyValue);
					}
					else if (property.PropertyType.BaseType.IsGenericType && property.PropertyType.BaseType.GetGenericTypeDefinition().Equals(typeof(ViewedDataList<>)))
					{
						if (((IList)propertyValue).Count > 0)
						{
							yield return new SerializationElement(property.Name, property.PropertyType.BaseType, propertyValue);	
						}
					}
					else
					{
						yield return new SerializationElement(property.Name, property.PropertyType, propertyValue);
					}
				}
			}
		}
		
		internal static IEnumerable<PropertyInfo> GetViewedProperties(Object objectToSerialize)
		{
			foreach (PropertyInfo property in objectToSerialize.GetType().GetProperties())
			{
				if (property.GetCustomAttribute(typeof(ViewedAttribute)) != null && property.CanRead && property.CanWrite)
				{
					yield return property;
				}
			}
		}
		
		#endregion
	}
	
	internal struct SerializationElement
	{
		#region Fields
		
		private readonly String mElementName_;
		private readonly Type mElementType_;
		private readonly Object mElementValue_;
		
		#endregion
		
		#region Properties
		
		public String ElementName
		{
			get
			{
				return mElementName_;
			}
		}
		
		public Type ElementType
		{
			get
			{
				return mElementType_;
			}
		}
		
		public Object ElementValue
		{
			get
			{
				return mElementValue_;
			}
		}
		
		#endregion
		
		public SerializationElement(String name, Type type, Object value)
		{
			mElementName_ = name;
			mElementType_ = type;
			mElementValue_ = value;
		}
	}
}
