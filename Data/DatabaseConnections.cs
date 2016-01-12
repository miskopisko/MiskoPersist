using System;
using MiskoPersist.Core;
using MiskoPersist.Enums;

namespace MiskoPersist.Data
{
	public class DatabaseConnections : AbstractViewedDataList<DatabaseConnection>
	{
		private static Logger Log = Logger.GetInstance(typeof(DatabaseConnections));
		
		#region Fields
		
        private static readonly DatabaseConnections mConnections_ = new DatabaseConnections();
		
		#endregion
		
		#region Properties
		
        public static DatabaseConnections Connections
		{
			get
			{
				return mConnections_;
			}
		}
		
		#endregion
		
		public static void AddMySqlConnection(String server, String datasource, String username, String password)
        {
            AddMySqlConnection("Default", server, datasource, username, password);
        }

        public static void AddMySqlConnection(String name, String server, String datasource, String username, String password)
        {
        	DatabaseConnection item = new DatabaseConnection();
            item.Name = name;
            item.DatabaseType = DatabaseType.MySql;
            item.Server = server;
            item.Port = null;
            item.Datasource = datasource;
            item.Username = username;
            item.Password = password;
            item.ConnectionString = "SERVER=" + server + ";DATABASE=" + datasource + ";UID=" + username + ";PASSWORD=" + password + ";Pooling=True;ConvertZeroDateTime=True;";

            if(!mConnections_.Contains(item))
            {
            	mConnections_.Add(item);
            }
        }

        public static void AddOracleConnection(String host, Int32 port, String datasource, String username, String password)
        {
            AddOracleConnection("Default", host, port, datasource, username, password);
        }

        public static void AddOracleConnection(String name, String host, Int32 port, String datasource, String username, String password)
        {
            DatabaseConnection item = new DatabaseConnection();
            item.Name = name;
            item.DatabaseType = DatabaseType.Oracle;
            item.Server = host;
            item.Port = port;
            item.Datasource = datasource;
            item.Username = username;
            item.Password = password;
            item.ConnectionString = "Data Source=(DESCRIPTION=(ADDRESS_LIST=(ADDRESS=(PROTOCOL=TCP)(HOST=" + host + ")(PORT=" + port + ")))(CONNECT_DATA=(SERVER=DEDICATED)(SERVICE_NAME=" + datasource + ")));User Id=" + username + ";Password=" + password + ";Pooling=True;";

            if(!mConnections_.Contains(item))
            {
            	mConnections_.Add(item);
            }
        }

        public static void AddSqliteConnection(String datasource)
        {
            AddSqliteConnection("Default", datasource);
        }

        public static void AddSqliteConnection(String name, String datasource)
        {
            DatabaseConnection item = new DatabaseConnection();
            item.Name = name;
            item.DatabaseType = DatabaseType.SQLite;
            item.Server = null;
            item.Port = null;
            item.Datasource = datasource;
            item.Username = null;
            item.Password = null;
            item.ConnectionString = "Data Source=" + datasource + ";Version=3;Pooling=True;";

            if(!mConnections_.Contains(item))
            {
            	mConnections_.Add(item);
            }
        }

        public static void AddFoxProConnection(String datasource)
        {
            AddFoxProConnection("Default", datasource);
        }

        public static void AddFoxProConnection(String name, String datasource)
        {
            DatabaseConnection item = new DatabaseConnection();
            item.Name = name;
            item.DatabaseType = DatabaseType.FoxPro;
            item.Server = null;
            item.Port = null;
            item.Datasource = datasource;
            item.Username = null;
            item.Password = null;
            item.ConnectionString = "Provider=vfpoledb.1;Data Source=" + datasource + ";Exclusive=No;Collate=Machine;NULL=YES;";
            
            if(!mConnections_.Contains(item))
            {
            	mConnections_.Add(item);
            }            
        }
        
        public static DatabaseConnection GetDatabaseConnection(String name)
        {
            foreach (DatabaseConnection item in mConnections_)
            {
                if (item.Name.Equals(name, StringComparison.CurrentCultureIgnoreCase))
                {
                    return item;
                }
            }
            return null;            
        }
	}
}
