using System;
using System.ComponentModel;
using System.Reflection;
using Newtonsoft.Json;

namespace MiskoPersist.Enums
{
    [JsonConverter(typeof(EnumSerializer))]
    public abstract class AbstractEnum : IComparable
    {
        #region Properties
		
        [EditorBrowsable(EditorBrowsableState.Never)]
        public Int64 Value 
        { 
        	get; 
        	set; 
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        public String Code 
        { 
        	get; 
        	set; 
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        public String Description 
        { 
        	get; 
        	set; 
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        public bool IsSet 
        { 
        	get 
        	{ 
        		return Value != -1; 
        	} 
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
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

        [EditorBrowsable(EditorBrowsableState.Never)]
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

        [EditorBrowsable(EditorBrowsableState.Never)]
        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            AbstractEnum o = obj as AbstractEnum;

            return o != null && o.GetType() == GetType() && o.Value == Value;
        }
    }
    
    internal sealed class EnumSerializer : JsonConverter
    {
		#region implemented abstract members of JsonConverter
		
		public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
		{
			AbstractEnum e = value as AbstractEnum;
			
			if(e.IsSet)
			{
		    	writer.WriteValue(((AbstractEnum)value).Value);
			}
            else
            {
                writer.WriteNull();
            }
		}
		
		public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
		{
            if(reader.Value != null)
            {
                return (AbstractEnum)objectType.InvokeMember("GetElement", BindingFlags.Default | BindingFlags.InvokeMethod, null, null, new object[] { (Int64)reader.Value });
            }
            return null;
		}
		
		public override bool CanConvert(Type objectType)
		{
			throw new NotImplementedException();
		}
		
		#endregion
    }
}
