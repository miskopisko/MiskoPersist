using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Runtime.Serialization;
using System.Xml;
using log4net;
using Message;
using MiskoPersist.Attributes;
using MiskoPersist.Core;
using MiskoPersist.Data;
using MiskoPersist.Enums;

namespace MiskoPersist.Serialization
{
	public class XmlFormatter : Serializer, IFormatter
	{
		private static ILog Log = LogManager.GetLogger(typeof(XmlFormatter));
		
		#region Fields
		
		
		
		#endregion
		
		#region Properties
		
		
		
		#endregion

		#region IFormatter implementation

		public Object Deserialize(Stream serializationStream)
		{
			XmlDocument document = new XmlDocument();
			document.Load(serializationStream);
			Type objectType = Type.GetType(document.DocumentElement.Attributes["Type"].Value);
			if (!objectType.IsSubclassOf(typeof(CoreMessage)))
			{
				throw new MiskoException("Can only deserialize messages");
			}
			return InitializeObject(document.DocumentElement, objectType);
		}

		public void Serialize(Stream serializationStream, Object graph)
		{
			if (graph == null)
			{
				return;
			}

			if (serializationStream == null)
			{
				throw new ArgumentException("Empty serializationStream!");
			}

			Stopwatch stopwatch = Stopwatch.StartNew();
			using (XmlTextWriter writer = new XmlTextWriter(serializationStream, ENCODING))
			{
				#if DEBUG
				writer.Formatting = Formatting.Indented;
				#endif
				
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
			foreach (SerializationElement element in GetMemberInfo(objectToSerialize))
			{
				if (element.ElementType.IsPrimitive || element.ElementType.Equals(typeof(String)) || element.ElementType.IsEnum)
				{
					writer.WriteStartElement(element.ElementName);
					writer.WriteValue(element.ElementValue.ToString());
					writer.WriteEndElement();
				}
				else if (typeof(ViewedDataList).IsAssignableFrom(element.ElementType))
				{
					foreach (ViewedData viewedData in (ViewedDataList)element.ElementValue)
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
			
			Object initializedObject = null;
			
			try
			{
				initializedObject = Activator.CreateInstance(objectType);
				
				foreach (PropertyInfo property in GetViewedProperties(initializedObject))
				{
					if (property.CanWrite)
					{
						Object obj = null;
						if (Converter.CanConvert(property.PropertyType))
						{
							obj = Converter.Convert(GetElement(node, property.Name), property.PropertyType);
						}
						else if (((ViewedAttribute)property.GetCustomAttribute(typeof(ViewedAttribute))).ViewedDeserializer != null)
						{
							obj = ((ViewedAttribute)property.GetCustomAttribute(typeof(ViewedAttribute))).ViewedDeserializer.Invoke(GetElement(node, property.Name));
						}
						else if (typeof(ViewedDataList).IsAssignableFrom(property.PropertyType))
						{
							obj = Activator.CreateInstance(property.PropertyType);
							foreach (XmlNode innerNode in node.SelectNodes(property.Name))
							{
								ViewedData data = (ViewedData)InitializeObject(innerNode, ((ViewedDataList)obj).ViewedDataType);
								((ViewedDataList)obj).Add(data);
							}
						}
						else
						{
							obj = InitializeObject(node[property.Name], property.PropertyType);
						}
						property.SetValue(initializedObject, obj);
					}
				}
			}
			catch (Exception e)
			{
				Log.Error(String.Format("Unable to create object {0}", objectType.Name), e);
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
