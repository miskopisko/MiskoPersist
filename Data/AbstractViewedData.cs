using System;
using System.Reflection;
using System.Runtime.Serialization;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;
using MiskoPersist.Attributes;
using MiskoPersist.Core;
using MiskoPersist.Enums;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Message;
using MiskoPersist.MoneyType;

namespace MiskoPersist.Data
{
	[JsonConverter(typeof(ViewedDataSerializer))]
	public class AbstractViewedData : AbstractData, ICloneable, IXmlSerializable
	{
		private static Logger Log = Logger.GetInstance(typeof(AbstractViewedData));

		#region Fields

		private const String DATEFORMAT = "yyyyMMddHHmmss";

		#endregion

		#region Properties

		internal XmlElement XML
		{
			get;
			set;
		}

		#endregion

		#region Constructors

		public AbstractViewedData()
		{
		}

		public AbstractViewedData(Session session, Persistence persistence)
		{
			Set(session, persistence);
		}

		#endregion

		#region Override Methods
		
		public new AbstractViewedData Set(Session session, Persistence persistence)
		{
			if(!persistence.IsEof)
			{
				base.Set(session, persistence);
			}
			
			return this;
		}

		public override String ToString()
		{
			String result = GetType().Name + Environment.NewLine;

			foreach (PropertyInfo property in GetType().GetProperties())
			{
				result += property.Name + ": " + property.GetValue(this, null) + Environment.NewLine;
			}

			return result;
		}
		
		#endregion
		
		#region ICloneable implementation
		
		public object Clone()
		{
			return MemberwiseClone();
		}
		
		#endregion

		#region Private Methods

		

		#endregion

		#region Public Methods

		public virtual void Fetch(Session session)
		{
			Fetch(session, false);
		}
		
		public virtual void Fetch(Session session, Boolean deep)
		{
		}
		
		public virtual void FetchDeep(Session session)
		{
		}
		
		#endregion

		#region Xml Serialization

		public XmlSchema GetSchema()
		{
			throw new NotImplementedException();
		}

		public virtual void WriteXml(XmlWriter writer)
		{
			foreach (PropertyInfo property in GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly))
			{
				if(!typeof(CoreMessage).IsAssignableFrom(GetType()) && property.GetCustomAttribute(typeof(ViewedAttribute)) == null)
				{
					continue;
				}

				Object obj = property.GetValue(this);
				if(obj != null)
				{
					if(obj is AbstractViewedData)
					{
						writer.WriteStartElement(property.Name);
						((AbstractViewedData)obj).WriteXml(writer);
						writer.WriteEndElement();
					}
					else if(obj is DateTime?)
					{
						if(((DateTime?)obj).HasValue)
						{
							writer.WriteStartElement(property.Name);
							writer.WriteValue(((DateTime)obj).ToString(DATEFORMAT));
							writer.WriteEndElement();	
						}
					}
					else if(obj is PrimaryKey)
					{
						if(((PrimaryKey)obj).IsSet)
						{
							writer.WriteStartElement(property.Name);
							writer.WriteValue(((PrimaryKey)obj).ToString());
							writer.WriteEndElement();
						}
					}
					else if(obj is Money)
					{
						writer.WriteStartElement(property.Name);
						writer.WriteRaw(String.Format("{0:F2}", ((Money)obj).Value));
						writer.WriteEndElement();
					}
					else if(obj is AbstractEnum)
					{
						if(((AbstractEnum)obj).IsSet)
						{
							writer.WriteStartElement(property.Name);
							writer.WriteValue(((AbstractEnum)obj).IsSet ? ((AbstractEnum)obj).Value : (Int64?)null);
							writer.WriteEndElement();	
						}
					}
					else if(obj.GetType().BaseType.IsGenericType && obj.GetType().BaseType.GetGenericTypeDefinition().Equals(typeof(AbstractViewedDataList<>)))
					{
						obj.GetType().BaseType.InvokeMember("WriteXml", BindingFlags.InvokeMethod, null, obj, new Object[] { writer, property.Name });
					}
					else if(obj.GetType().IsEnum)
					{
						if(Enum.IsDefined(obj.GetType(), obj))
						{
							writer.WriteStartElement(property.Name);
							writer.WriteValue(((Enum)obj).ToString());
							writer.WriteEndElement();	
						}
					}
					else
					{
						writer.WriteStartElement(property.Name);
						writer.WriteValue(obj.ToString());
						writer.WriteEndElement();
					}
				}
			}
		}

		public virtual void ReadXml(XmlReader reader)
		{		   
			foreach (PropertyInfo property in GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly))
			{
				if(!typeof(CoreMessage).IsAssignableFrom(GetType()) && property.GetCustomAttribute(typeof(ViewedAttribute)) == null)
				{
					continue;
				}
				
				if (property.PropertyType == typeof(String))
				{
					property.SetValue(this, GetElement(property.Name));
				}
				else if (property.PropertyType == typeof(Boolean))
				{
					Boolean? value = GetBooleanElement(property.Name);
					property.SetValue(this, value.HasValue && value.Value);
				}
				else if (property.PropertyType == typeof(Boolean?))
				{
					property.SetValue(this, GetBooleanElement(property.Name));
				}
				else if (property.PropertyType == typeof(Int32))
				{
					Int32? value = GetIntElement(property.Name);
					property.SetValue(this, value.HasValue ? value.Value : 0);
				}
				else if (property.PropertyType == typeof(Int32?))
				{
					property.SetValue(this, GetIntElement(property.Name));
				}
				else if (property.PropertyType == typeof(Int64))
				{
					Int64? value = GetLongElement(property.Name);
					property.SetValue(this, value.HasValue ? value.Value : 0);
				}
				else if (property.PropertyType == typeof(Int64?))
				{
					property.SetValue(this, GetLongElement(property.Name));
				}
				else if (property.PropertyType == typeof(Double))
				{
					Double? value = GetDoubleElement(property.Name);
					property.SetValue(this, value.HasValue ? value.Value : 0);
				}
				else if (property.PropertyType == typeof(Double?))
				{
					property.SetValue(this, GetDoubleElement(property.Name));
				}
				else if (property.PropertyType == typeof(DateTime))
				{
					DateTime? value = GetDateTimeElement(property.Name);
					property.SetValue(this, value.HasValue ? value.Value : DateTime.MinValue);
				}
				else if (property.PropertyType == typeof(DateTime?))
				{
					property.SetValue(this, GetDateTimeElement(property.Name));
				}
				else if (property.PropertyType.IsSubclassOf(typeof(AbstractEnum)))
				{
					property.SetValue(this, GetEnumElement(property.Name, property.PropertyType));
				}
				else if (property.PropertyType.IsSubclassOf(typeof(AbstractViewedData)))
				{
					property.SetValue(this, GetDataElement(property.Name, property.PropertyType));
				}
				else if (property.PropertyType == typeof(Guid))
				{
					property.SetValue(this, new Guid(GetElement(property.Name)));
				}
				else if (property.PropertyType == typeof(PrimaryKey))
				{
					Int64? value = GetLongElement(property.Name);
					property.SetValue(this, value.HasValue ? new PrimaryKey(value.Value) : new PrimaryKey(0));
				}
				else if (property.PropertyType == typeof(Money))
				{
					Decimal? value = GetDecimalElement(property.Name);
					property.SetValue(this, value.HasValue ? new Money(value.Value) : Money.ZERO);
				}
				else if(property.GetType().IsEnum)
				{
					property.SetValue(this, Enum.Parse(property.GetType(), GetElement(property.Name)));
				}
				else if(property.PropertyType.BaseType.IsGenericType && property.PropertyType.BaseType.GetGenericTypeDefinition() == typeof(AbstractViewedDataList<>))
				{
					property.SetValue(this, property.PropertyType.BaseType.InvokeMember("ReadXml", BindingFlags.InvokeMethod, null, null, new Object[] { XML, property.Name, property.PropertyType }));
				}
			}
		}

		public AbstractEnum GetEnumElement(String name, Type type)
		{
			AbstractEnum value = null;
			String v = GetElement(name);
			Int32 i;

			if (!String.IsNullOrEmpty(v) && Int32.TryParse(v, out i))
			{
				try
				{
					value = (AbstractEnum)type.InvokeMember("GetElement", BindingFlags.Default | BindingFlags.InvokeMethod, null, null, new Object[] { i });
				}
				catch
				{
				}
			}

			return value;
		}

		public AbstractViewedData GetDataElement(String name, Type type)
		{
			XmlElement e = XML == null ? null : XML[name];

			if (e != null)
			{
				AbstractViewedData value = (AbstractViewedData)Activator.CreateInstance(type);

				value.XML = e;
				value.ReadXml(null);

				return value;
			}

			return null;
		}

		public DateTime? GetDateTimeElement(String name)
		{
			DateTime? value = null;
			String v = GetElement(name);

			if (!String.IsNullOrEmpty(v))
			{
				try
				{
					value = DateTime.ParseExact(v, DATEFORMAT, null);
				}
				catch
				{
				}
			}

			return value;
		}

		public Double? GetDoubleElement(String name)
		{
			Double? value = null;
			String v = GetElement(name);

			if (!String.IsNullOrEmpty(v))
			{
				try
				{
					value = Double.Parse(v);
				}
				catch
				{
				}
			}

			return value;
		}
		
		public Decimal? GetDecimalElement(String name)
		{
			Decimal? value = null;
			String v = GetElement(name);

			if (!String.IsNullOrEmpty(v))
			{
				try
				{
					value = Decimal.Parse(v);
				}
				catch
				{
				}
			}

			return value;
		}

		public Int64? GetLongElement(String name)
		{
			Int64? value = null;
			String v = GetElement(name);

			if (!String.IsNullOrEmpty(v))
			{
				try
				{
					value = Int64.Parse(v);
				}
				catch
				{
				}
			}

			return value;
		}

		public Int32? GetIntElement(String name)
		{
			Int32? value = null;
			String v = GetElement(name);

			if (!String.IsNullOrEmpty(v))
			{
				try
				{
					value = Int32.Parse(v);
				}
				catch
				{
				}
			}

			return value;
		}

		public Boolean? GetBooleanElement(String name)
		{
			Boolean? value = false;
			String v = GetElement(name);

			if (!String.IsNullOrEmpty(v))
			{
				try
				{
					value = Boolean.Parse(v);
				}
				catch
				{
				}
			}

			return value;
		}

		public String GetElement(String name)
		{
			XmlElement e = XML == null ? null : XML[name];
			return e != null ? e.InnerText : null;
		}

		#endregion
	}

	internal sealed class ViewedDataSerializer : JsonConverter
	{
		#region implemented abstract members of JsonConverter

		public override void WriteJson(JsonWriter writer, Object value, JsonSerializer serializer)
		{
			AbstractViewedData data = value as AbstractViewedData;

			writer.WriteStartObject();

			writer.WritePropertyName("$type");
			writer.WriteValue(data.GetType().ToString() + ", " + data.GetType().Assembly.GetName().Name);

			foreach (PropertyInfo property in data.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly))
			{
				if (property.GetCustomAttribute(typeof(ViewedAttribute)) != null)
				{
					Object obj = property.GetValue(data);
					if(obj != null)
					{
						writer.WritePropertyName(property.Name);
						serializer.Serialize(writer, obj);
					}
				}
			}

			writer.WriteEndObject();
		}

		public override Object ReadJson(JsonReader reader, Type objectType, Object existingValue, JsonSerializer serializer)
		{
			JObject jsonObject = JObject.Load(reader);

			AbstractViewedData data = (AbstractViewedData)Activator.CreateInstance(objectType);

			foreach(PropertyInfo property in data.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly))
			{
				if(property.GetCustomAttribute(typeof(ViewedAttribute)) != null)
				{
					JToken token = jsonObject.GetValue(property.Name);
					if(token != null)
					{
						property.SetValue(data, serializer.Deserialize(token.CreateReader(), property.PropertyType));	
					}
				}
			}

			return data;
		}

		public override Boolean CanConvert(Type objectType)
		{
			throw new NotImplementedException();
		}

		#endregion
	}
}
