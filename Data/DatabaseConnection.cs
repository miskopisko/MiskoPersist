using System;
using System.Data;
using System.Xml;
using MiskoPersist.Attributes;
using MiskoPersist.Core;
using MiskoPersist.Data;
using MiskoPersist.Enums;

namespace MiskoPersist.Data
{
    public class DatabaseConnection : AbstractViewedData
	{
		private static Logger Log = Logger.GetInstance(typeof(DatabaseConnection));
		
		#region Fields

		

        #endregion

        #region Properties

        [Viewed]
        public String Name
        {
        	get;
        	set;
        }

        [Viewed]
        public DatabaseType DatabaseType
        {
        	get;
        	set;
        }

        [Viewed]
        public String Server
        {
        	get;
        	set;
        }

        [Viewed]
        public Int32? Port
        {
        	get;
        	set;
        }

        [Viewed]
        public String Datasource
        {
        	get;
        	set;
        }

        [Viewed]
        public String Username
        {
        	get;
        	set;
        }

        [Viewed]
        public String Password
        {
        	get;
        	set;
        }

        [Viewed]
        public String ConnectionString
        {
        	get;
        	set;
        }
            
        public ConnectionState State
        {
        	get
        	{
        		return ServiceLocator.GetConnection(Name).State;
        	}
        }
        
        #endregion

        #region XmlSerialization

        public override void WriteXml(XmlWriter writer)
        {
            base.WriteXml(writer);

            writer.WriteStartElement("ConnectionStatus");
            writer.WriteValue(State.ToString());
            writer.WriteEndElement();
        }

        #endregion
	}
}
