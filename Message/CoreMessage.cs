using System;
using System.IO;
using System.Runtime.Serialization.Formatters;
using System.Xml;
using MiskoPersist.Core;
using MiskoPersist.Data;
using MiskoPersist.Enums;
using MiskoPersist.Message.Response;
using Newtonsoft.Json;

namespace Message
{
	public class CoreMessage : AbstractViewedData
	{
		private static Logger Log = Logger.GetInstance(typeof(CoreMessage));

		#region Fields

		private static JsonSerializerSettings mJsonSettings_;

		#endregion

		#region Properties

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
		
		private static JsonSerializerSettings JsonSettings
		{
			get
			{
				if(mJsonSettings_ == null)
				{
					mJsonSettings_ = new JsonSerializerSettings();
					mJsonSettings_.TypeNameAssemblyFormat = FormatterAssemblyStyle.Simple;
					mJsonSettings_.TypeNameHandling = TypeNameHandling.Objects;
					mJsonSettings_.NullValueHandling = NullValueHandling.Ignore;
					mJsonSettings_.DateFormatHandling = DateFormatHandling.IsoDateFormat;
					mJsonSettings_.DateParseHandling = DateParseHandling.DateTime;
					mJsonSettings_.Formatting = Newtonsoft.Json.Formatting.Indented;	
				}
				
				return mJsonSettings_;
			}
		}

		#endregion

		public CoreMessage()
		{
			Confirmations = new ErrorMessages();
		}        
		
		#region Serialization
		
		public static CoreMessage Read(String message, SerializationType serializationType)
        {
			CoreMessage result = null;
			
        	if(serializationType.Equals(SerializationType.Xml))
        	{
        		XmlDocument document = new XmlDocument();
                document.LoadXml(message);

                result = (CoreMessage)Activator.CreateInstance(Type.GetType(document.DocumentElement.Attributes["Type"].Value));
                result.ReadXml(message);
        	}
        	else if(serializationType.Equals(SerializationType.Json))
        	{
				return (CoreMessage)JsonSerializer.Create(JsonSettings).Deserialize(new JsonTextReader(new StringReader(message)));
        	}
        	else
        	{
				result = new ResponseMessage();
				((ResponseMessage)result).Status = ErrorLevel.Error;
            	((ResponseMessage)result).Errors.Add(new ErrorMessage(new MiskoException("Error parsing message. It is not a proper Xml or Json format.")));
        	}
        	
        	return result;
        }

        public String Write(SerializationType serializationType)
        {
        	if(serializationType.Equals(SerializationType.Xml))
        	{
        		return WriteXml();
        	}
        	else if(serializationType.Equals(SerializationType.Json))
        	{
        		return WriteJson();
        	}

            throw new MiskoException("Invalid serialization type. Should be one of Xml or Json");
        }

		private String WriteJson()
		{
			using(StringWriter stringWriter = new StringWriter())
			{
				using(JsonTextWriter writer = new JsonTextWriter(stringWriter))
				{
					JsonSerializer.Create(JsonSettings).Serialize(writer, this);
					stringWriter.Close();
					
					return stringWriter.ToString();
				}
			}
		}
		
		private String WriteXml()
		{
			using(StringWriter stringWriter = new StringWriter())
			{
				using(XmlWriter writer = XmlWriter.Create(stringWriter, new XmlWriterSettings(){ Indent = true, NewLineHandling = NewLineHandling.Replace }))
				{
					writer.WriteStartDocument();
					writer.WriteStartElement(GetType().Name);
					writer.WriteAttributeString("Type", null, GetType().ToString() + ", " + GetType().Assembly.GetName().Name);
					WriteXml(writer);
					writer.WriteEndElement();
					writer.WriteEndDocument();
					writer.Flush();
					
					return stringWriter.ToString();
				}
			}
		}
		
		private void ReadXml(String xml)
		{
			using(XmlReader reader = XmlReader.Create(new StringReader(xml.Trim())))
			{
				reader.MoveToContent();

				XmlDocument document = new XmlDocument();
				XML = (XmlElement)document.ReadNode(reader);
				ReadXml(reader);	
			}
		}

		#endregion
	}
}

