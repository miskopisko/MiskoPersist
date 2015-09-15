using System;
using System.Reflection;
using MiskoPersist.Attributes;
using MiskoPersist.Core;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace MiskoPersist.Data
{
    [JsonConverter(typeof(ViewedDataSerializer))]
    public abstract class AbstractViewedData : AbstractData
    {
        private static Logger Log = Logger.GetInstance(typeof(AbstractViewedData));

        #region Fields



        #endregion

        #region Properties

        

        #endregion

        #region Constructors

        protected AbstractViewedData()
        {
        }

        protected AbstractViewedData(Session session, Persistence persistence)
        {
            Set(session, persistence);
        }

        #endregion

        #region Override Methods
        
        public new AbstractViewedData Set(Session session, Persistence persistence)
		{
			IsSet = persistence.Next();
			
			if(IsSet)
            {
				base.Set(session, persistence);
			}
			
			return this;
		}

        public override string ToString()
        {
            String result = GetType().Name + Environment.NewLine;

            foreach (PropertyInfo property in GetType().GetProperties())
            {
                result += property.Name + ": " + property.GetValue(this, null) + Environment.NewLine;
            }

            return result;
        }

        #endregion

        #region Private Methods

        

        #endregion

        #region Public Methods

        public virtual void Fetch(Session session)
        {
        	Fetch(session, false);
        }
        
		public virtual void Fetch(Session session, bool deep)
        {
        }
		
		public virtual void FetchDeep(Session session)
		{
		}
        
        #endregion
    }

    internal sealed class ViewedDataSerializer : JsonConverter
    {
        #region implemented abstract members of JsonConverter

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
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

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            JObject jsonObject = JObject.Load(reader);

            AbstractViewedData data = (AbstractViewedData)Activator.CreateInstance(objectType);
            data.IsSet = true;

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

        public override bool CanConvert(Type objectType)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
