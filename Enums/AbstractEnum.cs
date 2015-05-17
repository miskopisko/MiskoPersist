using System;
using System.ComponentModel;
using System.Reflection;
using System.Runtime.Serialization;
using System.Windows.Forms;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace MiskoPersist.Enums
{
	[JsonConverter(typeof(EnumSerializer))]
	[Serializable]
    public abstract class AbstractEnum : IComparable
    {
        #region Properties
		
        public Int64 Value 
        { 
        	get; 
        	set; 
        }
        
        public String Code 
        { 
        	get; 
        	set; 
        }
        
        public String Description 
        { 
        	get; 
        	set; 
        }
        
        public bool IsSet 
        { 
        	get 
        	{ 
        		return Value != -1; 
        	} 
        }
        
        public bool IsNotSet 
        { 
        	get 
        	{ 
        		return !IsSet; 
        	} 
        }

        #endregion

        #region Constructors

        protected AbstractEnum()
        {
        }

        protected AbstractEnum(Int64 v, String code, String description)
        {
            Value = v;
            Code = code;
            Description = description;
        }

        #endregion

        public override String ToString()
        {
            return Description;
        }

        public bool Equals(String codeOrDesc)
        {
			return codeOrDesc != null && Description.Equals(codeOrDesc) || Code.Equals(codeOrDesc);
        }

        public virtual int CompareTo(object e)
        {
            if (e is AbstractEnum)
            {
                if (e == null || ((AbstractEnum)e).Code == null || ((AbstractEnum)e).Code.Trim().Length == 0)
                {
                    return 1;
                }
				return string.Compare(Code, ((AbstractEnum)e).Code, StringComparison.Ordinal);
            }
            return -1;
        }
    }
    
    internal class EnumSerializer : JsonConverter
    {
		#region implemented abstract members of JsonConverter
		
		public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
		{
			AbstractEnum e = value as AbstractEnum;
			
			if(e.IsSet)
			{
		    	writer.WriteValue(((AbstractEnum)value).Value);
			}
		}
		
		public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
		{
			return (AbstractEnum)objectType.InvokeMember("GetElement", BindingFlags.Default | BindingFlags.InvokeMethod, null, null, new object[] { (Int64)reader.Value });
		}
		
		public override bool CanConvert(Type objectType)
		{
			throw new NotImplementedException();
		}
		
		#endregion
    }
}
