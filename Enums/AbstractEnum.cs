using System;
using System.ComponentModel;
using System.Reflection;
using System.Xml;
using Newtonsoft.Json;

namespace MiskoPersist.Enums
{
	[JsonConverter(typeof(EnumSerializer))]
    public abstract class AbstractEnum : IComparable
    {
        #region Properties
		
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
        public Boolean IsSet 
        { 
        	get 
        	{ 
        		return Value != -1; 
        	} 
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        public Boolean IsNotSet 
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
        public virtual Int32 CompareTo(Object e)
        {
            if (e is AbstractEnum)
            {
                if (e == null || ((AbstractEnum)e).Code == null || ((AbstractEnum)e).Code.Trim().Length == 0)
                {
                    return 1;
                }
				return String.Compare(Code, ((AbstractEnum)e).Code, StringComparison.Ordinal);
            }
            return -1;
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        public override Int32 GetHashCode()
        {
            return base.GetHashCode();
        }

        public override Boolean Equals(Object obj)
        {
            AbstractEnum o = obj as AbstractEnum;

            return o != null && o.GetType() == GetType() && o.Value == Value;
        }

		#region XmlSerialization

		public void ReadXml(XmlReader reader)
		{
			
		}

		public void WriteXml(XmlWriter writer)
		{
			if(IsSet)
			{
				writer.WriteValue(Value);
			}
		}

		#endregion
    }
    
    internal sealed class EnumSerializer : JsonConverter
    {
		#region implemented abstract members of JsonConverter
		
		public override void WriteJson(JsonWriter writer, Object value, JsonSerializer serializer)
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
		
		public override Object ReadJson(JsonReader reader, Type objectType, Object existingValue, JsonSerializer serializer)
		{
			return reader.Value != null ? (AbstractEnum)objectType.InvokeMember("GetElement", BindingFlags.Default | BindingFlags.InvokeMethod, null, null, new Object[] { (Int64)reader.Value }) : null;
		}
		
		public override Boolean CanConvert(Type objectType)
		{
			throw new NotImplementedException();
		}
		
		#endregion
    }
}
