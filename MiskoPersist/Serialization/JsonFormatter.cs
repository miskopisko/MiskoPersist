using System;
using System.Collections;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using log4net;
using MiskoPersist.Attributes;
using MiskoPersist.Core;
using MiskoPersist.Data.Viewed;
using MiskoPersist.Enums;
using MiskoPersist.Interfaces;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace MiskoPersist.Serialization
{
	public class JsonFormatter : IMiskoFormatter
	{
		private static ILog Log = LogManager.GetLogger(typeof(JsonFormatter));

        #region Fields



        #endregion

        #region Properties



        #endregion

        #region Public Methods

		Object IMiskoFormatter.Deserialize(Stream serializationStream)
		{			
			Object o = null;
			
			Stopwatch stopwatch = Stopwatch.StartNew();
			using (StreamReader reader = new StreamReader(serializationStream, Serializer.Encoding))
			{
				using (JsonReader jr = new JsonTextReader(reader))
				{
					JObject jObject = JObject.Load(jr);
					Type objectType = Type.GetType((String)jObject["Type"]);
					if (!objectType.IsSubclassOf(typeof(ViewedData)) && !(objectType.BaseType.IsGenericType && objectType.BaseType.GetGenericTypeDefinition().Equals(typeof(ViewedDataList<>))))
					{
						throw new MiskoException("Can only deserialize viewed data");
					}
					o = InitializeObject(jObject, objectType);
				}
			}			
			stopwatch.Stop();
            Log.Debug(String.Format("{0} to {1} : {2}", SerializationType.Json, o.GetType().Name, stopwatch.Elapsed));
            
			return o;
		}

		void IMiskoFormatter.Serialize(Stream serializationStream, Object graph, Boolean indent)
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
			using (StreamWriter streamWriter = new StreamWriter(serializationStream, Serializer.Encoding))
			{
				using (JsonTextWriter writer = new JsonTextWriter(streamWriter))
				{
					writer.Formatting = indent ? Formatting.Indented : Formatting.None;
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
			foreach (SerializationElement element in Serializer.GetMemberInfo(objectToSerialize))
			{
				if (element.ElementType.IsPrimitive || element.ElementType.Equals(typeof(String)) || element.ElementType.IsEnum)
				{
					writer.WritePropertyName(element.ElementName);
					writer.WriteValue(element.ElementValue.ToString());
				}
				else if (element.ElementType.IsGenericType && element.ElementType.GetGenericTypeDefinition().Equals(typeof(ViewedDataList<>)))
				{
					writer.WritePropertyName(element.ElementName);
					writer.WriteStartArray();
					foreach (ViewedData viewedData in (IList)element.ElementValue)
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
			
			if (objectType.BaseType.IsGenericType && objectType.BaseType.GetGenericTypeDefinition().Equals(typeof(ViewedDataList<>)))
			{
				if (token[objectType.Name] != null)
				{
					foreach (var innerToken in token[objectType.Name])
					{
						ViewedData data = (ViewedData)InitializeObject(innerToken, objectType.BaseType.GetGenericArguments()[0]);
						((IList)initializedObject).Add(data);
					}
				}
			}
			else
			{
				foreach (PropertyInfo property in Serializer.GetViewedProperties(initializedObject))
				{
					Object obj = null;
					if (MiskoConverter.CanConvert(property.PropertyType))
					{
						obj = MiskoConverter.Convert((String)token[property.Name], property.PropertyType);
					}
					else if (((ViewedAttribute)property.GetCustomAttribute(typeof(ViewedAttribute))).ViewedDeserializer != null)
					{
						if (!String.IsNullOrEmpty((String)token[property.Name]))
						{
							obj = ((ViewedAttribute)property.GetCustomAttribute(typeof(ViewedAttribute))).ViewedDeserializer.Invoke((String)token[property.Name]);	
						}
					}
					else if (property.PropertyType.BaseType.IsGenericType && property.PropertyType.BaseType.GetGenericTypeDefinition().Equals(typeof(ViewedDataList<>)))
					{
						obj = Activator.CreateInstance(property.PropertyType);
						if (token[property.Name] != null)
						{
							foreach (var innerToken in token[property.Name])
							{
								ViewedData data = (ViewedData)InitializeObject(innerToken, property.PropertyType.BaseType.GetGenericArguments()[0]);
								((IList)obj).Add(data);
							}
						}
					}
					else
					{
						obj = InitializeObject(token[property.Name], property.PropertyType);
					}				
					property.SetValue(initializedObject, obj);
				}
			}
			
			return initializedObject;
		}
		
		#endregion
	}
}
