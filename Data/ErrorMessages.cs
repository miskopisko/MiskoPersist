using System;
using System.Xml;
using MiskoPersist.Core;
using MiskoPersist.Enums;

namespace MiskoPersist.Data
{
	public class ErrorMessages : AbstractViewedDataList<ErrorMessage>
    {
        private static Logger Log = Logger.GetInstance(typeof(ErrorMessages));

        #region Fields



        #endregion

        #region Properties

        

        #endregion

        #region Constructors



        #endregion

        #region Private Methods



        #endregion

        #region Public Methods
        
        public Boolean IsConfirmed(ErrorMessage errorMessage)
        {
        	foreach(ErrorMessage message in this)
        	{
        		if(message.Equals(errorMessage))
        		{
        			return message.Confirmed.HasValue && message.Confirmed.Value;
        		}
        	}
        	
        	return false;
        }

        public Boolean Contains(ErrorLevel level)
        {
            if (level != null && Count > 0)
            {
                foreach (ErrorMessage message in this)
                {
                    if (message.ErrorLevel.Equals(level))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        public ErrorMessages ListOf(ErrorLevel level)
        {
            ErrorMessages list = new ErrorMessages();

            if (level != null && Count > 0)
            {
                foreach (ErrorMessage message in this)
                {
                    if (message.ErrorLevel.Equals(level))
                    {
                        list.Add(message);
                    }
                }
            }

            return list;
        }

        #endregion

		#region XmlSerialization

		public void ReadXml(XmlElement XML, String name)
		{
			if (XML != null)
			{				
				foreach (XmlNode n in XML.ChildNodes)
				{
					if (n.Name == name)
					{
						foreach (XmlNode messageNode in ((XmlElement)n).ChildNodes)
						{
							if (messageNode.Name == "ErrorMessage")
							{
								ErrorMessage value = new ErrorMessage();
								value.XML = (XmlElement)messageNode;
								value.IsSet = true;
								value.ReadXml(null);
								Add(value);
							}
						}
					}
				}
			}
		}

		public void WriteXml(XmlWriter writer)
		{
			foreach(ErrorMessage message in this)
			{
				message.WriteXml(writer);
			}
		}

		#endregion
    }
}
