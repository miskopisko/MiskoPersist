using System;
using System.Reflection;
using System.Xml;
using Message;
using MiskoPersist.Core;
using MiskoPersist.Data;
using MiskoPersist.Enums;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace MiskoPersist.Message.Response
{
	[JsonConverter(typeof(ResponseSerializer))]
	public class ResponseMessage : CoreMessage
	{
		private static Logger Log = Logger.GetInstance(typeof(ResponseMessage));

		#region Fields


		#endregion

		#region Properties

		public ErrorLevel Status 
		{ 
			get; 
			set; 
		}        
		
		public ErrorMessages Errors
		{
			get;
			set;
		}
		
		public ErrorMessages Infos
		{
			get;
			set;
		}
		
		public ErrorMessages Warnings
		{
			get;
			set;
		}

		public Boolean HasErrors 
		{ 
			get 
			{ 
				return Errors != null && Errors.Count > 0; 
			} 
		}

		public Boolean HasInfos 
		{ 
			get 
			{ 
				return Infos != null && Infos.Count > 0; 
			} 
		}
		
		public Boolean HasWarnings 
		{ 
			get 
			{ 
				return Warnings != null && Warnings.Count > 0; 
			} 
		}
		
		public Boolean HasConfirmations 
		{ 
			get 
			{ 
				return Confirmations != null && Confirmations.Count > 0; 
			} 
		}

		public Boolean HasUnconfirmed 
		{
			get
			{
				Boolean hasUnconfirmed = false;
				if(HasConfirmations)
				{
					foreach (ErrorMessage confirmMessage in Confirmations) 
					{
						if(confirmMessage.Confirmed.HasValue && !confirmMessage.Confirmed.Value)
						{
							hasUnconfirmed = true;
							break;
						}
					}
				}
				return hasUnconfirmed;
			}
		}
		
		public ErrorMessages ErrorMessages
		{
			get
			{
				ErrorMessages result = new ErrorMessages();
				result.AddRange(Errors);
				result.AddRange(Confirmations);
				result.AddRange(Warnings);
				result.AddRange(Infos);

				return result;
			}
			set
			{
				Errors = ((ErrorMessages)value).ListOf(ErrorLevel.Error);
				Confirmations = ((ErrorMessages)value).ListOf(ErrorLevel.Confirmation);
				Warnings = ((ErrorMessages)value).ListOf(ErrorLevel.Warning);
				Infos = ((ErrorMessages)value).ListOf(ErrorLevel.Information);
			}
		}
		
		public TimeSpan MessageExecutionTime
		{
			get;
			set;
		}
		
		public TimeSpan SqlExecutionTime
		{
			get;
			set;
		}

		#endregion

		#region Constructors

		public  ResponseMessage()
		{
			Errors = new ErrorMessages();
			Warnings = new ErrorMessages();
			Infos = new ErrorMessages();
		}

		#endregion
		
		#region XmlSerialization

        public override void ReadXml(XmlReader reader)
		{
        	if(!GetType().Equals(typeof(ResponseMessage)))
        	{
				base.ReadXml(reader);
        	}
			
			Status = (ErrorLevel)GetEnumElement("Status", typeof(ErrorLevel));
			Errors.ReadXml(XML, "Errors");
			Confirmations.ReadXml(XML, "Confirmations");
			Warnings.ReadXml(XML, "Warnings");
			Infos.ReadXml(XML, "Infos");
			Page = (Page)GetDataElement("Page", typeof(Page));			
			MessageExecutionTime = TimeSpan.Parse(GetElement("MessageExecutionTime"));
			SqlExecutionTime = TimeSpan.Parse(GetElement("SqlExecutionTime"));
		}

        public override void WriteXml(XmlWriter writer)
		{
        	if(!GetType().Equals(typeof(ResponseMessage)))
        	{
            	base.WriteXml(writer);
        	}

			writer.WriteStartElement("Status");
			Status.WriteXml(writer);
			writer.WriteEndElement();

			if(HasErrors)
			{
				writer.WriteStartElement("Errors");
				Errors.WriteXml(writer);
				writer.WriteEndElement();
			}

			if(HasConfirmations)
			{
				writer.WriteStartElement("Confirmations");
				Confirmations.WriteXml(writer);
				writer.WriteEndElement();
			}

			if(HasWarnings)
			{
				writer.WriteStartElement("Warnings");
				Warnings.WriteXml(writer);
				writer.WriteEndElement();
			}

			if(HasInfos)
			{
				writer.WriteStartElement("Infos");
				Infos.WriteXml(writer);
				writer.WriteEndElement();
			}

			if(Page != null)
			{
				writer.WriteStartElement("Page");
				Page.WriteXml(writer);
				writer.WriteEndElement();
			}
			
            writer.WriteStartElement("MessageExecutionTime");
            writer.WriteString(MessageExecutionTime.ToString("G"));
            writer.WriteEndElement();
			
            writer.WriteStartElement("SqlExecutionTime");
            writer.WriteString(SqlExecutionTime.ToString("G"));
            writer.WriteEndElement();
		}

		#endregion
	}

	internal sealed class ResponseSerializer : JsonConverter
	{
		#region implemented abstract members of JsonConverter

		public override void WriteJson(JsonWriter writer, Object value, JsonSerializer serializer)
		{
			ResponseMessage response = value as ResponseMessage;

			writer.WriteStartObject();

			writer.WritePropertyName("$type");
			writer.WriteValue(response.GetType().ToString() + ", " + response.GetType().Assembly.GetName().Name);

			if(response.GetType() != typeof(ResponseMessage))
			{
				foreach (PropertyInfo property in response.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly))
				{
					Object obj = property.GetValue(response);
					if(obj != null)
					{
						writer.WritePropertyName(property.Name);
						serializer.Serialize(writer, obj);
					}
				}    
			}

			writer.WritePropertyName("Status");
			serializer.Serialize(writer, response.Status);

			if(response.HasErrors)
			{
				writer.WritePropertyName("Errors");
				serializer.Serialize(writer, response.Errors);
			}

			if(response.HasConfirmations)
			{
				writer.WritePropertyName("Confirmations");
				serializer.Serialize(writer, response.Confirmations);
			}

			if(response.HasWarnings)
			{
				writer.WritePropertyName("Warnings");
				serializer.Serialize(writer, response.Warnings);
			}

			if(response.HasInfos)
			{
				writer.WritePropertyName("Infos");
				serializer.Serialize(writer, response.Infos);
			}

			if(response.Page != null)
			{
				writer.WritePropertyName("Page");
				serializer.Serialize(writer, response.Page);
			}
			
			writer.WritePropertyName("MessageExecutionTime");
			serializer.Serialize(writer, response.MessageExecutionTime.ToString("G"));
			
			writer.WritePropertyName("SqlExecutionTime");
			serializer.Serialize(writer, response.SqlExecutionTime.ToString("G"));

			writer.WriteEndObject();
		}

		public override Object ReadJson(JsonReader reader, Type objectType, Object existingValue, JsonSerializer serializer)
		{
			JObject jsonObject = JObject.Load(reader);
			
			ResponseMessage response = (ResponseMessage)Activator.CreateInstance(Type.GetType((String)jsonObject["$type"]));
			
			foreach(PropertyInfo property in response.GetType().GetProperties())
			{
				JToken token = jsonObject.GetValue(property.Name);
				if(token != null)
				{
					property.SetValue(response, serializer.Deserialize(token.CreateReader(), property.PropertyType));    
				}
			}
			
			return response;
		}

		public override Boolean CanConvert(Type objectType)
		{
			throw new NotImplementedException();
		}

		#endregion
	}
}
