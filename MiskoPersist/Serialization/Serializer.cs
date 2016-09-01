using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Text;
using log4net;
using Message;
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
		
		
		
		#endregion
		
		#region Properties
		
		public static Encoding Encoding
		{
			get
			{
				return new UTF8Encoding(false);
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
		
		public static String Serialize(CoreMessage message, SerializationType serializationType)
		{
			if (message == null)
			{
				throw new ArgumentNullException("message");
			}
			
			if (serializationType == null)
			{
				throw new ArgumentNullException("serializationType");
			}

			using (MemoryStream ms = new MemoryStream())
			{
				serializationType.GetFormatter().Serialize(ms, message);
				return Encoding.GetString(ms.ToArray());
			}
		}
		
		public static CoreMessage Deserialize(String str)
		{
            using (MemoryStream ms = new MemoryStream(Encoding.GetBytes(str)))
			{
				return Deserialize(ms);
			}
		}
		
		public static CoreMessage Deserialize(Stream stream)
		{
			CoreMessage result = null;            
			SerializationType serializationtype = SerializationType.NULL;            
            
			stream.Position = 0;
            Int32 firstByte = stream.ReadByte();
            if (firstByte.Equals(Encoding.GetBytes("<")[0]))
            {
                serializationtype = SerializationType.Xml;
            }
            else if (firstByte.Equals(Encoding.GetBytes("{")[0]))
            {
                serializationtype = SerializationType.Json;
            }
            
            if(serializationtype != null && serializationtype.IsSet)
            {
            	Stopwatch stopwatch = Stopwatch.StartNew();
            	result = (CoreMessage) serializationtype.GetFormatter().Deserialize(stream);
            	stopwatch.Stop();
            	Log.Debug(String.Format("{0} to {1} : {2}", serializationtype, result.GetType().Name, stopwatch.Elapsed));
            }
            
            return result;
		}
		
		#endregion
		
		#region Internal Methods
		
		internal static IEnumerable<SerializationElement> GetMemberInfo(Object objectToSerialize)
		{
			if (objectToSerialize == null)
			{
				yield break;
			}
			
			foreach (PropertyInfo property in GetViewedProperties(objectToSerialize))
			{
				Object propertyValue = property.GetValue(objectToSerialize);

				if (((ViewedAttribute)property.GetCustomAttribute(typeof(ViewedAttribute))).ViewedSerializer != null)
				{
					if (propertyValue != null)
					{
						String value = ((ViewedAttribute)property.GetCustomAttribute(typeof(ViewedAttribute))).ViewedSerializer.Invoke(propertyValue);
						yield return new SerializationElement(property.Name, typeof(String), value);
					}
				}
				else if (typeof(MiskoEnum).IsAssignableFrom(property.PropertyType))
				{
					if (propertyValue != null && ((MiskoEnum)propertyValue).IsSet)
					{
						yield return new SerializationElement(property.Name, typeof(Int64), ((MiskoEnum)propertyValue).Value);
					}
				}
				else if (typeof(PrimaryKey).IsAssignableFrom(property.PropertyType))
				{
					if (propertyValue != null && ((PrimaryKey)propertyValue).IsSet)
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
				else if (typeof(Money).IsAssignableFrom(property.PropertyType))
                {
                    if (propertyValue != null)
                    {
                        yield return new SerializationElement(property.Name, typeof(String), ((Money)propertyValue).Value.ToString(MoneyFormat));
                    }
                }
				else if (typeof(String).IsAssignableFrom(property.PropertyType))
				{
					if (!String.IsNullOrEmpty(((String)propertyValue)))
					{
						yield return new SerializationElement(property.Name, property.PropertyType, propertyValue);
					}
				}
				else if (property.PropertyType.IsGenericType && property.PropertyType.GetGenericTypeDefinition().Equals(typeof(Nullable<>)))
				{
					if (propertyValue != null)
					{
						yield return new SerializationElement(property.Name, Nullable.GetUnderlyingType(property.PropertyType), propertyValue);
					}
				}
				else if (typeof(ViewedDataList).IsAssignableFrom(property.PropertyType))
				{
					if (propertyValue != null && ((ViewedDataList)propertyValue).Count > 0)
					{
						yield return new SerializationElement(property.Name, property.PropertyType, propertyValue);
					}
				}
				else if (propertyValue != null)
				{
					yield return new SerializationElement(property.Name, property.PropertyType, propertyValue);
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
