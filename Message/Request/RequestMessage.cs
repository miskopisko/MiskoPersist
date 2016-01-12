﻿using System;
using System.Reflection;
using System.Xml;
using Message;
using MiskoPersist.Core;
using MiskoPersist.Data;
using MiskoPersist.Enums;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace MiskoPersist.Message.Request
{
	[JsonConverter(typeof(RequestSerializer))]
	public class RequestMessage : CoreMessage
	{
		private static Logger Log = Logger.GetInstance(typeof(RequestMessage));

		#region Fields

		

		#endregion

		#region Properties

		public MessageMode MessageMode 
		{ 
			get;
			set; 
		}

		public String Command 
		{ 
			get;
			set; 
		}

		public String Connection 
		{ 
			get;
			set; 
		}

		#endregion

		#region Constructors

		public RequestMessage()
		{
			MessageMode = MessageMode.Normal;
			Command = "Execute";
			Connection = "Default";
		}

		#endregion
		
		#region XmlSerialization

        public override void ReadXml(XmlReader reader)
		{
        	if(!GetType().Equals(typeof(RequestMessage)))
        	{
				base.ReadXml(reader);
        	}
			
			MessageMode = (MessageMode)GetEnumElement("MessageMode", typeof(MessageMode)) ?? MessageMode.Normal;
			Command = GetElement("Command") ?? "Execute";
			Connection = GetElement("Connection") ?? "Default";
			Page = (Page)GetDataElement("Page", typeof(Page));
			Confirmations.ReadXml(XML, "Confirmations");
		}
		
        public override void WriteXml(XmlWriter writer)
		{
        	if(!GetType().Equals(typeof(RequestMessage)))
        	{
				base.WriteXml(writer);
        	}
						
			if(!MessageMode.Equals(MessageMode.Normal))
			{
				writer.WriteStartElement("MessageMode");
				writer.WriteValue(MessageMode);
				writer.WriteEndElement();
			}

			if(!Command.Equals("Execute"))
			{
				writer.WriteStartElement("Command");
				writer.WriteValue(Command);
				writer.WriteEndElement();
			}

			if(!Connection.Equals("Default"))
			{
				writer.WriteStartElement("Connection");
				writer.WriteValue(Connection);
				writer.WriteEndElement();
			}

			if(Page != null)
			{
				writer.WriteStartElement("Page");
				Page.WriteXml(writer);
				writer.WriteEndElement();
			}

			if(Confirmations != null && Confirmations.Count > 0)
			{
				writer.WriteStartElement("Confirmations");
				Confirmations.WriteXml(writer);
				writer.WriteEndElement();
			}
		}

		#endregion
	}

	internal sealed class RequestSerializer : JsonConverter
	{
		#region implemented abstract members of JsonConverter

		public override void WriteJson(JsonWriter writer, Object value, JsonSerializer serializer)
		{
			RequestMessage request = value as RequestMessage;

			writer.WriteStartObject();

			writer.WritePropertyName("$type");
			writer.WriteValue(request.GetType().ToString() + ", " + request.GetType().Assembly.GetName().Name);

			foreach (PropertyInfo property in request.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly))
			{
				writer.WritePropertyName(property.Name);
				serializer.Serialize(writer, property.GetValue(request));
			}

			if(!request.MessageMode.Equals(MessageMode.Normal))
			{
				writer.WritePropertyName("MessageMode");
				serializer.Serialize(writer, request.MessageMode);
			}

			if(!request.Command.Equals("Execute"))
			{
				writer.WritePropertyName("Command");
				serializer.Serialize(writer, request.Command);
			}

			if(!request.Connection.Equals("Default"))
			{
				writer.WritePropertyName("Connection");
				serializer.Serialize(writer, request.Connection);
			}

			if(request.Page != null)
			{
				writer.WritePropertyName("Page");
				serializer.Serialize(writer, request.Page);
			}

			if(request.Confirmations != null && request.Confirmations.Count > 0)
			{
				writer.WritePropertyName("Confirmations");
				serializer.Serialize(writer, request.Confirmations);
			}

			writer.WriteEndObject();
		}

		public override Object ReadJson(JsonReader reader, Type objectType, Object existingValue, JsonSerializer serializer)
		{
			JObject jsonObject = JObject.Load(reader);

			RequestMessage request = (RequestMessage)Activator.CreateInstance(Type.GetType((String)jsonObject["$type"]));

			foreach(PropertyInfo property in request.GetType().GetProperties())
			{
				JToken token = jsonObject.GetValue(property.Name);
				if(token != null)
				{
					property.SetValue(request, serializer.Deserialize(token.CreateReader(), property.PropertyType));    
				}
			}

			return request;
		}

		public override Boolean CanConvert(Type objectType)
		{
			throw new NotImplementedException();
		}

		#endregion
	}
}
