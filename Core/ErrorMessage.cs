using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using MiskoPersist.Data;
using MiskoPersist.Enums;
using MiskoPersist.Tools;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace MiskoPersist.Core
{
    [JsonConverter(typeof(ErrorSerializer))]
    public class ErrorMessage : AbstractViewedData
    {
        private static Logger Log = Logger.GetInstance(typeof(ErrorMessage));

        #region Fields

        private String mErrorMessage_;
        private Boolean? mConfirmed_ = false;

        #endregion

        #region Properties

		public String Class 
		{
			get;
			set;
		}
		
		public String Method 
		{
			get;
			set;
		}
		
		public List<Object> Parameters 
		{
			get;
			set;
		}
		
		public ErrorLevel ErrorLevel
		{
			get;
			set;
		}
		
        public String Message 
        { 
        	get 
        	{ 
        		return ToString(); 
        	} 
        	set 
        	{ 
        		mErrorMessage_ = value;
        	} 
        }

        public Boolean? Confirmed 
        { 
        	get  
        	{ 
        		return ErrorLevel.Equals(ErrorLevel.Confirmation) ? mConfirmed_ : null; 
        	} 
        	set
        	{ 
        		mConfirmed_ = value; 
        	} 
        }

        #endregion

        #region Constructors

        public ErrorMessage()
        {
        }

        public ErrorMessage(Exception e) 
        {
        	IsSet = true;
        	
        	if(e.TargetSite == null)
        	{
        		StackFrame stackFrame = new StackFrame(1);
        		Class = stackFrame.GetMethod().DeclaringType.Name;
            	Method = stackFrame.GetMethod().Name;
        	}
			else
			{
				Class = e.TargetSite.DeclaringType.Name;
				Method = e.TargetSite.Name;
			}
        	
            ErrorLevel = ErrorLevel.Error;
            mErrorMessage_ = e.Message;
            Parameters = null;
        }

        public ErrorMessage(Type clazz, MethodBase method, ErrorLevel level, String message) 
        	: this(clazz, method, level, message, null)
        {
        }

        public ErrorMessage(Type clazz, MethodBase method, ErrorLevel level, String message, Object[] parameters)
        {
        	IsSet = true;
            Class = clazz.Name;
            Method = method.Name;
            ErrorLevel = level;
            mErrorMessage_ = message;
            Parameters = parameters != null ? new List<Object>(parameters) : null;
        }

        #endregion

        #region Override Methods

        public override string ToString()
        {
            return Utils.ResolveTextParameters(mErrorMessage_, Parameters != null ? Parameters.ToArray() : null);
        }
        
        #endregion

		#region Equals and GetHashCode implementation
		
		public override int GetHashCode()
		{
			int hashCode = 0;
			
			unchecked
			{
				if (Message != null) 
				{
					hashCode += 1000000007 * Message.GetHashCode();
				}
				
				if (Class != null) 
				{
					hashCode += 1000000021 * Class.GetHashCode();
				}
				
				if (Method != null) 
				{
					hashCode += 1000000033 * Method.GetHashCode();
				}
				
				if (Parameters != null) 
				{
					hashCode += 1000000087 * Parameters.GetHashCode();
				}
				
				if (ErrorLevel != null) 
				{
					hashCode += 1000000093 * ErrorLevel.GetHashCode();
				}
			}
			
			return hashCode;
		}

		public override bool Equals(object obj)
		{
			ErrorMessage other = obj as ErrorMessage;
			
			if(other == null)
			{
				return false;
			}
			
			bool parametersEqual = true;
			if((Parameters != null && other.Parameters != null) && (Parameters.Count.Equals(other.Parameters.Count)))
			{
				parametersEqual = new HashSet<Object>(Parameters).SetEquals(new HashSet<Object>(other.Parameters));
			}
						
			return mErrorMessage_.Equals(other.mErrorMessage_) && Class.Equals(other.Class) && Method.Equals(other.Method) && ErrorLevel.Equals(other.ErrorLevel) && parametersEqual;
		}

		public static bool operator ==(ErrorMessage lhs, ErrorMessage rhs) 
		{
			if (ReferenceEquals(lhs, rhs)) 
			{
				return true;
			}
			
			if (ReferenceEquals(lhs, null) || ReferenceEquals(rhs, null)) 
			{
				return false;
			}
			
			return lhs.Equals(rhs);
		}

		public static bool operator !=(ErrorMessage lhs, ErrorMessage rhs) 
		{
			return !(lhs == rhs);
		}

        #endregion
    }

    internal class ErrorSerializer : JsonConverter
    {
        #region implemented abstract members of JsonConverter

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            ErrorMessage errorMessage = value as ErrorMessage;

            writer.WriteStartObject();

            writer.WritePropertyName("$type");
            writer.WriteValue(errorMessage.GetType().ToString() + ", " + errorMessage.GetType().Assembly.GetName().Name);

            writer.WritePropertyName("Class");
            serializer.Serialize(writer, errorMessage.Class);

            writer.WritePropertyName("Method");
            serializer.Serialize(writer, errorMessage.Method);

            if(errorMessage.Parameters != null && errorMessage.Parameters.Count > 0)
            {
                writer.WritePropertyName("Parameters");
                serializer.Serialize(writer, errorMessage.Parameters);    
            }

            writer.WritePropertyName("ErrorLevel");
            serializer.Serialize(writer, errorMessage.ErrorLevel);

            writer.WritePropertyName("Message");
            serializer.Serialize(writer, errorMessage.Message);

            writer.WriteEndObject();
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            JObject jsonObject = JObject.Load(reader);

            ErrorMessage result = new ErrorMessage();
            result.Class = (String)jsonObject["Class"];
            result.Method = (String)jsonObject["Method"];
            result.Parameters = jsonObject["Parameters"] != null ? jsonObject["Parameters"].ToObject<List<Object>>() : null;
            result.ErrorLevel = ErrorLevel.GetElement((int)jsonObject["ErrorLevel"]);
            result.Message = (String)jsonObject["Message"];

            return result;
        }

        public override bool CanConvert(Type objectType)
        {
            throw new NotImplementedException();
        }

        #endregion


    }
}
