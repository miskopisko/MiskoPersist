using System;
using MiskoPersist.Core;
using MiskoPersist.Data;
using MiskoPersist.Enums;
using Newtonsoft.Json;
using System.Reflection;

namespace MiskoPersist.Message.Request
{
    [JsonConverter(typeof(RequestSerializer))]
    public class RequestMessage
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
        
        public Page Page 
        { 
        	get;
        	set; 
        }
        
        public ErrorMessages Confirms
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

        #region Private Properties

        
        
        #endregion
    }

    internal class RequestSerializer : JsonConverter
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

            if(request.Confirms != null && request.Confirms.Count > 0)
            {
                writer.WritePropertyName("Confirms");
                serializer.Serialize(writer, request.Confirms);
            }

            writer.WriteEndObject();
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }

        public override bool CanConvert(Type objectType)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
