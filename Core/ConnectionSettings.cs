using System;
using System.Collections.Generic;
using MiskoPersist.Enums;

namespace MiskoPersist.Core
{
    public class ConnectionSettings
    {
        private static Logger Log = Logger.GetInstance(typeof(ConnectionSettings));

        #region Fields

        private static readonly List<ConnectionSettings> mConnections_ = new List<ConnectionSettings>();

        #endregion

        #region Properties

        public static List<ConnectionSettings> Connections 
        { 
        	get 
        	{ 
        		return mConnections_; 
        	} 
        }

        public String Name
        {
        	get;
        	set;
        }
        
        public ConnectionType ConnectionType
        {
        	get;
        	set;
        }
        
        public String Server
        {
        	get;
        	set;
        }
        
        public Int32? Port
        {
        	get;
        	set;
        }
        
        public String Datasource
        {
        	get;
        	set;
        }
        
        public String Username
        {
        	get;
        	set;
        }
        
        public String Password
        {
        	get;
        	set;
        }
        
        public String ConnectionString
        {
        	get;
        	set;
        }

        #endregion

        #region Public Static Methods

        public static void AddMySqlConnection(String server, String datasource, String username, String password)
        {
            AddMySqlConnection("Default", server, datasource, username, password);
        }

        public static void AddMySqlConnection(String name, String server, String datasource, String username, String password)
        {
            if (AlreadyExists(name))
            {
            	return;
            }

            ConnectionSettings item = new ConnectionSettings();
            item.Name = name;
            item.ConnectionType = ConnectionType.MySql;
            item.Server = server;
            item.Port = null;
            item.Datasource = datasource;
            item.Username = username;
            item.Password = password;
            item.ConnectionString = "SERVER=" + server + ";DATABASE=" + datasource + ";UID=" + username + ";PASSWORD=" + password + ";Pooling=True;ConvertZeroDateTime=True;";

            mConnections_.Add(item);
        }

        public static void AddOracleConnection(String host, Int32 port, String datasource, String username, String password)
        {
            AddOracleConnection("Default", host, port, datasource, username, password);
        }

        public static void AddOracleConnection(String name, String host, Int32 port, String datasource, String username, String password)
        {
            if (AlreadyExists(name))
            {
            	return;
            }

            ConnectionSettings item = new ConnectionSettings();
            item.Name = name;
            item.ConnectionType = ConnectionType.Oracle;
            item.Server = host;
            item.Port = port;
            item.Datasource = datasource;
            item.Username = username;
            item.Password = password;
            item.ConnectionString = "Data Source=(DESCRIPTION=(ADDRESS_LIST=(ADDRESS=(PROTOCOL=TCP)(HOST=" + host + ")(PORT=" + port + ")))(CONNECT_DATA=(SERVER=DEDICATED)(SERVICE_NAME=" + datasource + ")));User Id=" + username + ";Password=" + password + ";Pooling=True;";

            mConnections_.Add(item);
        }

        public static void AddSqliteConnection(String datasource)
        {
            AddSqliteConnection("Default", datasource);
        }

        public static void AddSqliteConnection(String name, String datasource)
        {
            if (AlreadyExists(name))
            {
            	return;
            }

            ConnectionSettings item = new ConnectionSettings();
            item.Name = name;
            item.ConnectionType = ConnectionType.SQLite;
            item.Server = null;
            item.Port = null;
            item.Datasource = datasource;
            item.Username = null;
            item.Password = null;
            item.ConnectionString = "Data Source=" + datasource + ";Version=3;Pooling=True;";

            mConnections_.Add(item);
        }

        public static void AddFoxProConnection(String datasource)
        {
            AddFoxProConnection("Default", datasource);
        }

        public static void AddFoxProConnection(String name, String datasource)
        {
            if (AlreadyExists(name))
            {
            	return;
            }

            ConnectionSettings item = new ConnectionSettings();
            item.Name = name;
            item.ConnectionType = ConnectionType.FoxPro;
            item.Server = null;
            item.Port = null;
            item.Datasource = datasource;
            item.Username = null;
            item.Password = null;
            item.ConnectionString = "Provider=vfpoledb.1;Data Source=" + datasource + ";";

            mConnections_.Add(item);            
        }

        public static ConnectionSettings GetConnectionSettings(String name)
        {
            foreach (ConnectionSettings item in mConnections_)
            {
                if (item.Name.Equals(name, StringComparison.CurrentCultureIgnoreCase))
                {
                    return item;
                }
            }
            return null;            
        }

        public static void RemoveConnection(String connectionName)
        {
            List<ConnectionSettings> connections = mConnections_;
            foreach (ConnectionSettings item in connections)
            {
                if (item.Name.Equals(connectionName, StringComparison.CurrentCultureIgnoreCase))
                {
                    mConnections_.Remove(item);
                }
            }
        }

        #endregion

        #region Private Static Methods

        private static Boolean AlreadyExists(String name)
        {
            foreach (ConnectionSettings item in mConnections_)
            {
                if(item.Name.Equals(name, StringComparison.CurrentCultureIgnoreCase))
                {
                    return true;
                }
            }
            return false;
        }

        #endregion
    }
}
