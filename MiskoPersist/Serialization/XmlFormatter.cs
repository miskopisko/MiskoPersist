using System;
using System.Collections;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Xml;
using log4net;
using MiskoPersist.Attributes;
using MiskoPersist.Core;
using MiskoPersist.Data.Viewed;
using MiskoPersist.Enums;
using MiskoPersist.Interfaces;
using MiskoPersist.Message;

namespace MiskoPersist.Serialization
{
	public class XmlFormatter : IMiskoFormatter
	{
		private static ILog Log = LogManager.GetLogger(typeof(XmlFormatter));

        #region Fields



        #endregion

        #region Properties



        #endregion

        #region Public Methods

		Object IMiskoFormatter.Deserialize(Stream serializationStream)
		{
			serializationStream.Position = 0;
			XmlDocument document = new XmlDocument();
			document.Load(serializationStream);
			Type objectType = Type.GetType(document.DocumentElement.Attributes["Type"].Value);
			if (!objectType.IsSubclassOf(typeof(CoreMessage)))
			{
				throw new MiskoException("Can only deserialize messages");
			}
			return InitializeObject(document.DocumentElement, objectType);
		}

		void IMiskoFormatter.Serialize(Stream serializationStream, Object graph)
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
			using (XmlTextWriter writer = new XmlTextWriter(serializationStream, Serializer.Encoding))
			{
				writer.Formatting = Formatting.Indented;
				writer.WriteStartDocument();
				writer.WriteStartElement(graph.GetType().Name);
				writer.WriteAttributeString("Type", null, graph.GetType() + ", " + graph.GetType().Assembly.GetName().Name);
				Serialize(writer, graph);
				writer.WriteEndElement();
				writer.WriteEndDocument();
				writer.Flush();
			}
			stopwatch.Stop();
			Log.Debug(String.Format("{0} to {1} : {2}", graph.GetType().Name, SerializationType.Xml, stopwatch.Elapsed));
		}

        #endregion

        #region Serialization

        private void Serialize(XmlWriter writer, Object objectToSerialize)
		{
			foreach (SerializationElement element in Serializer.GetMemberInfo(objectToSerialize))
			{
				if (element.ElementType.IsPrimitive || element.ElementType.Equals(typeof(String)) || element.ElementType.IsEnum)
				{
					writer.WriteStartElement(element.ElementName);
					writer.WriteValue(element.ElementValue.ToString());
					writer.WriteEndElement();
				}
				else if (element.ElementType.IsGenericType && element.ElementType.GetGenericTypeDefinition().Equals(typeof(ViewedDataList<>)))
				{
					foreach (ViewedData viewedData in (IList)element.ElementValue)
					{
						writer.WriteStartElement(element.ElementName);
						Serialize(writer, viewedData);
						writer.WriteEndElement();
					}
				}
				else
				{
					writer.WriteStartElement(element.ElementName);
					Serialize(writer, element.ElementValue);
					writer.WriteEndElement();
				}
			}
		}

		#endregion

		#region Deserialization

		private Object InitializeObject(XmlNode node, Type objectType)
		{
			if (node == null)
			{
				return null;
			}
			
			Object initializedObject = Activator.CreateInstance(objectType);
				
			foreach (PropertyInfo property in Serializer.GetViewedProperties(initializedObject))
			{
				Object obj = null;
				if (MiskoConverter.CanConvert(property.PropertyType))
				{
					obj = MiskoConverter.Convert(GetElement(node, property.Name), property.PropertyType);
				}
				else if (((ViewedAttribute)property.GetCustomAttribute(typeof(ViewedAttribute))).ViewedDeserializer != null)
				{
					obj = ((ViewedAttribute)property.GetCustomAttribute(typeof(ViewedAttribute))).ViewedDeserializer.Invoke(GetElement(node, property.Name));
				}
				else if (property.PropertyType.BaseType.IsGenericType && property.PropertyType.BaseType.GetGenericTypeDefinition().Equals(typeof(ViewedDataList<>)))
				{
					obj = Activator.CreateInstance(property.PropertyType);
					foreach (XmlNode innerNode in node.SelectNodes(property.Name))
					{
						ViewedData data = (ViewedData)InitializeObject(innerNode, property.PropertyType.BaseType.GetGenericArguments()[0]);
						((IList)obj).Add(data);
					}
				}
				else
				{
					obj = InitializeObject(node[property.Name], property.PropertyType);
				}						
				property.SetValue(initializedObject, obj);
			}
			
			return initializedObject;
		}

		private String GetElement(XmlNode element, String name)
		{
			XmlElement e = element == null ? null : element[name];
			return e != null ? e.InnerText : null;
		}

		#endregion
	}
}
