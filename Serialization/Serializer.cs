using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text;
using log4net;
using Message;
using MiskoPersist.Attributes;
using MiskoPersist.Core;
using MiskoPersist.Data;
using MiskoPersist.Enums;
using MiskoPersist.MoneyType;

namespace MiskoPersist.Serialization
{
	public class Serializer
	{
		private static ILog Log = LogManager.GetLogger(typeof(Serializer));

		#region Fields

		private MiskoConverter mConverter_;
		
		internal static readonly String DATEFORMAT = "yyyyMMddHHmmss";
		internal static readonly String MONEYFORMAT = "F4";
        public static readonly Encoding ENCODING = new UTF8Encoding(false);
		
		#endregion
		
		#region Properties

		internal MiskoConverter Converter
		{
			get
			{
				if (mConverter_ == null)
				{
					mConverter_ = new MiskoConverter();
				}
				return mConverter_;
			}
		}
		
		public ISurrogateSelector SurrogateSelector
		{
			get
			{
				return null;
			}
			set
			{
			}
		}

		public SerializationBinder Binder
		{
			get
			{
				return null;
			}
			set
			{
			}
		}

		public StreamingContext Context
		{
			get
			{
				return new StreamingContext(StreamingContextStates.All);
			}
			set
			{
			}
		}
		
		#endregion
		
		#region Public Static Methods
		
		public static String Serialize(CoreMessage message)
		{
			if (message == null)
			{
				throw new ArgumentNullException(nameof(message));
			}

			using (MemoryStream ms = new MemoryStream())
			{
				message.SerializationType.GetFormatter().Serialize(ms, message);
				return ENCODING.GetString(ms.ToArray());
			}
		}
		
		public static CoreMessage Deserialize(String str)
		{
            using (MemoryStream ms = new MemoryStream(ENCODING.GetBytes(str)))
			{
				return Deserialize(ms);
			}
		}
		
		public static CoreMessage Deserialize(Stream stream)
		{
            stream.Position = 0;
            int firstByte = stream.ReadByte();

            CoreMessage result = null;
            if (firstByte.Equals(ENCODING.GetBytes("<")[0]))
            {
                XmlFormatter formatter = new XmlFormatter();
                result = (CoreMessage)((IFormatter)formatter).Deserialize(stream);
                result.SerializationType = SerializationType.Xml;
            }
            else if (firstByte.Equals(ENCODING.GetBytes("{")[0]))
            {
                JsonFormatter formatter = new JsonFormatter();
                result = (CoreMessage)((IFormatter)formatter).Deserialize(stream);
                result.SerializationType = SerializationType.Json;
            }
            else
            {
			    throw new MiskoException("Unable to determine serialization method");
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
						yield return new SerializationElement(property.Name, typeof(String), ((DateTime)propertyValue).ToString(DATEFORMAT));
					}
				}
				else if (typeof(Money).IsAssignableFrom(property.PropertyType))
				{
					if (propertyValue != null)
					{
						yield return new SerializationElement(property.Name, typeof(String), ((Money)propertyValue).Value.ToString(MONEYFORMAT));
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
