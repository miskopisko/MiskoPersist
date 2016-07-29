using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Runtime.Serialization;
using log4net;
using Message;
using MiskoPersist.Attributes;
using MiskoPersist.Core;
using MiskoPersist.Data;
using MiskoPersist.Enums;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace MiskoPersist.Serialization
{
	public class JsonFormatter : Serializer, IFormatter
	{
		private static ILog Log = LogManager.GetLogger(typeof(JsonFormatter));

        #region Fields



        #endregion

        #region Properties



        #endregion

        #region IFormatter implementation

        Object IFormatter.Deserialize(Stream serializationStream)
        {
            serializationStream.Position = 0;
            using (StreamReader reader = new StreamReader(serializationStream, ENCODING))
            {
                using (JsonReader jr = new JsonTextReader(reader))
                {
                    JObject jObject = JObject.Load(jr);
                    Type objectType = Type.GetType((String)jObject["Type"]);
                    if (!objectType.IsSubclassOf(typeof(CoreMessage)))
                    {
                        throw new MiskoException("Can only deserialize messages");
                    }
                    return InitializeObject(jObject, objectType);
                }
            }
        }

        void IFormatter.Serialize(Stream serializationStream, Object graph)
        {
            if (graph == null)
            {
                return;
            }

            if (serializationStream == null)
            {
                throw new ArgumentNullException("serializationStream");
            }

            Stopwatch stopwatch = Stopwatch.StartNew();
            using (StreamWriter streamWriter = new StreamWriter(serializationStream, ENCODING))
            {
                using (JsonTextWriter writer = new JsonTextWriter(streamWriter))
                {
                    writer.Formatting = Formatting.Indented;
                    writer.WriteStartObject();
                    writer.WritePropertyName("Type");
                    writer.WriteValue(graph.GetType() + ", " + graph.GetType().Assembly.GetName().Name);
                    Serialize(writer, graph);
                    writer.WriteEndObject();
                    writer.Flush();
                }
            }
            stopwatch.Stop();

            Log.Debug(String.Format("{0} to {1} : {2}", graph.GetType().Name, SerializationType.Json, stopwatch.Elapsed));
        }

        #endregion

        #region Serialization

        private void Serialize(JsonWriter writer, Object objectToSerialize)
		{
			foreach (SerializationElement element in GetMemberInfo(objectToSerialize))
			{
				if (element.ElementType.IsPrimitive || element.ElementType.Equals(typeof(String)) || element.ElementType.IsEnum)
				{
					writer.WritePropertyName(element.ElementName);
					writer.WriteValue(element.ElementValue.ToString());
				}
				else if (typeof(ViewedDataList).IsAssignableFrom(element.ElementType))
				{
					writer.WritePropertyName(element.ElementName);
					writer.WriteStartArray();
					foreach (ViewedData viewedData in (ViewedDataList)element.ElementValue)
					{
						writer.WriteStartObject();
						Serialize(writer, viewedData);
						writer.WriteEndObject();
					}
					writer.WriteEndArray();
				}
				else
				{
					writer.WritePropertyName(element.ElementName);
					writer.WriteStartObject();
					Serialize(writer, element.ElementValue);
					writer.WriteEndObject();
				}
			}
		}

		#endregion
		
		#region Deserialization
		
		private Object InitializeObject(JToken token, Type objectType)
		{
			if (token == null)
			{
				return null;
			}
			
			Object initializedObject = Activator.CreateInstance(objectType);
			
			foreach (PropertyInfo property in GetViewedProperties(initializedObject))
			{
				Object obj = null;
				if (Converter.CanConvert(property.PropertyType))
				{
					obj = Converter.Convert((String)token[property.Name], property.PropertyType);
				}
				else if (((ViewedAttribute)property.GetCustomAttribute(typeof(ViewedAttribute))).ViewedDeserializer != null)
				{
					obj = ((ViewedAttribute)property.GetCustomAttribute(typeof(ViewedAttribute))).ViewedDeserializer.Invoke((String)token[property.Name]);
				}
				else if (typeof(ViewedDataList).IsAssignableFrom(property.PropertyType))
				{
					obj = Activator.CreateInstance(property.PropertyType);
					if (token[property.Name] != null)
					{
						foreach (var innerToken in token[property.Name])
						{
							ViewedData data = (ViewedData)InitializeObject(innerToken, ((ViewedDataList)obj).ViewedDataType);
							((ViewedDataList)obj).Add(data);
						}
					}
				}
				else
				{
					obj = InitializeObject(token[property.Name], property.PropertyType);
				}
				property.SetValue(initializedObject, obj);
			}
			
			return initializedObject;
		}
		
		#endregion
	}
}
