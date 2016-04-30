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
		internal static readonly Encoding ENCODING = Encoding.GetEncoding("ISO-8859-1");
		
		#endregion
		
		#region Properties

		internal MiskoConverter Converter
		{
			get
			{
				if(mConverter_ == null)
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
		
		#region Public Methods
				
		public static String Serialize(Object o, SerializationType serializationType)
		{
			if(o == null)
			{
				throw new ArgumentNullException();
			}
			
			if(!(o is CoreMessage))
			{
				throw new MiskoException("Can only serialize messages");
			}
			
			using(MemoryStream ms = new MemoryStream())
			{
				IFormatter formatter = serializationType.Equals(SerializationType.Xml) ? (IFormatter)new XmlFormatter() : (IFormatter)new JsonFormatter();
				formatter.Serialize(ms, o);
				return ENCODING.GetString(ms.ToArray());
			}
		}
		
		public static Object Deserialize(String str)
		{
			using(MemoryStream ms = new MemoryStream(ENCODING.GetBytes(str)))
			{
				if(str.StartsWith("<", StringComparison.Ordinal))
				{
					return Deserialize(ms, SerializationType.Xml);
				}
				if(str.StartsWith("{", StringComparison.Ordinal))
				{
					return Deserialize(ms, SerializationType.Json);
				}
				throw new MiskoException("Invalid input. Not a valid XML or Json string.");
			}
		}
		
		public static Object Deserialize(Stream stream, SerializationType serializationType)
		{
			IFormatter formatter = null;
			if(serializationType.Equals(SerializationType.Xml))
			{
				formatter = new XmlFormatter();
			}
			else if(serializationType.Equals(SerializationType.Json))
			{
				formatter = new JsonFormatter();
			}
			return formatter != null ? formatter.Deserialize(stream) : null;
		}
	
		#endregion
		
		#region Internal Methods
		
		internal static IEnumerable<SerializationElement> GetMemberInfo(Object objectToSerialize)
		{
			if(objectToSerialize == null)
			{
				yield break;
			}
			
			foreach(PropertyInfo property in GetViewedProperties(objectToSerialize))
			{
				Object propertyValue = property.GetValue(objectToSerialize);

				if(typeof(MiskoEnum).IsAssignableFrom(property.PropertyType))
				{
					if(propertyValue != null && ((MiskoEnum)propertyValue).IsSet)
					{
						yield return new SerializationElement(property.Name, typeof(Int64), ((MiskoEnum)propertyValue).Value);
					}
				}
				else if(typeof(PrimaryKey).IsAssignableFrom(property.PropertyType))
				{
					if(propertyValue != null && ((PrimaryKey)propertyValue).IsSet)
					{
						yield return new SerializationElement(property.Name, typeof(Int64), ((PrimaryKey)propertyValue).Value);
					}
				}
				else if(typeof(DateTime).IsAssignableFrom(property.PropertyType) || typeof(DateTime?).IsAssignableFrom(property.PropertyType))
				{
					if(((DateTime?)propertyValue).HasValue)
					{
						yield return new SerializationElement(property.Name, typeof(String), ((DateTime)propertyValue).ToString(DATEFORMAT));	
					}
				}
				else if(typeof(Money).IsAssignableFrom(property.PropertyType))
				{
					if(propertyValue != null)
					{
						yield return new SerializationElement(property.Name, typeof(String), ((Money)propertyValue).Value.ToString(MONEYFORMAT));
					}
				}
				else if(typeof(String).IsAssignableFrom(property.PropertyType))
				{
					if(!String.IsNullOrEmpty(((String)propertyValue)))
					{
						yield return new SerializationElement(property.Name, property.PropertyType, propertyValue);
					}
				}
				else if(property.PropertyType.IsGenericType && property.PropertyType.GetGenericTypeDefinition().Equals(typeof(Nullable<>)))
				{
					if(propertyValue != null)
					{
						yield return new SerializationElement(property.Name, Nullable.GetUnderlyingType(property.PropertyType), propertyValue);
					}
				}
				else if(typeof(ViewedDataList).IsAssignableFrom(property.PropertyType))
				{
					if(propertyValue != null && ((ViewedDataList)propertyValue).Count > 0)
					{
						yield return new SerializationElement(property.Name, property.PropertyType, propertyValue);
					}
				}
				else if(propertyValue != null)
				{
					yield return new SerializationElement(property.Name, property.PropertyType, propertyValue);
				}
			}
		}
		
		internal static IEnumerable<PropertyInfo> GetViewedProperties(Object objectToSerialize)
		{
			foreach(PropertyInfo property in objectToSerialize.GetType().GetProperties())
			{
				if(property.GetCustomAttribute(typeof(ViewedAttribute)) != null && property.CanRead && property.CanWrite)
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
