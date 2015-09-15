using System;
using System.IO;
using System.Reflection;
using System.Runtime.Serialization.Formatters;
using System.Text;
using Message;
using MiskoPersist.Core;
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

		#region Serialization

		public static RequestMessage Deserialize(String json)
		{
			RequestMessage requestMessage;

			using (JsonReader reader = new JsonTextReader(new StreamReader(new MemoryStream(Encoding.UTF8.GetBytes(json ?? "")))))
			{
				JsonSerializer serializer = new JsonSerializer();
				serializer.TypeNameAssemblyFormat = FormatterAssemblyStyle.Simple;
				serializer.TypeNameHandling = TypeNameHandling.Objects;
				serializer.NullValueHandling = NullValueHandling.Ignore;
				serializer.DateFormatString = "yyyy-MM-dd";
				serializer.Formatting = Formatting.Indented;
				requestMessage = serializer.Deserialize<RequestMessage>(reader);
			}

			return requestMessage;
		}

		#endregion
	}

	internal sealed class RequestSerializer : JsonConverter
	{
		#region implemented abstract members of JsonConverter

		public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
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

		public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
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

		public override bool CanConvert(Type objectType)
		{
			throw new NotImplementedException();
		}

		#endregion
	}
}
