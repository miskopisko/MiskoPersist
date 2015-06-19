using System;
using System.ComponentModel;
using System.Reflection;
using MiskoPersist.Core;
using MiskoPersist.Data;
using MiskoPersist.Enums;
using Newtonsoft.Json;

namespace MiskoPersist.Message.Response
{
    [JsonConverter(typeof(ResponseSerializer))]
    public class ResponseMessage
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
        
        public ErrorMessages Confirmations
        {
            get;
            set;
        }
        
        public Page Page 
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
                bool hasUnconfirmed = false;
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

        #endregion

        #region Constructors

        public  ResponseMessage()
        {
            Errors = new ErrorMessages();
            Confirmations = new ErrorMessages();
            Warnings = new ErrorMessages();
            Infos = new ErrorMessages();
        }

        #endregion
    }

    internal class ResponseSerializer : JsonConverter
    {
        #region implemented abstract members of JsonConverter

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
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
