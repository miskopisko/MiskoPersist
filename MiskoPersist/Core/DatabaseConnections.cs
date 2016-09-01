using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Data.OleDb;
using System.Data.SQLite;
using log4net;
using MiskoPersist.Enums;
using MySql.Data.MySqlClient;
using Oracle.ManagedDataAccess.Client;

namespace MiskoPersist.Core
{
	public static class DatabaseConnections
	{
		private static ILog Log = LogManager.GetLogger(typeof(DatabaseConnections));

		#region Fields
		
		private static readonly Dictionary<String, DatabaseConnection> mConnections_ = new Dictionary<String, DatabaseConnection>();		
		
		#endregion
		
		#region Properties
		
		public static Dictionary<String, DatabaseConnection> Connections
		{
			get
			{
				return mConnections_;
			}
		}
		
		#endregion
		
		#region Constructors
		
		
		
		#endregion
		
		#region Static Methods

		public static DbConnection GetConnection(String name)
		{
			name = name ?? "Default";
			
			if (mConnections_.ContainsKey(name))
			{
				DatabaseConnection databaseConnection = mConnections_[name];
				
				if (databaseConnection.DatabaseType.Equals(DatabaseType.SQLite))
				{
					return new SQLiteConnection(databaseConnection.ConnectionString);
				}
				if (databaseConnection.DatabaseType.Equals(DatabaseType.MySql))
				{
					return new MySqlConnection(databaseConnection.ConnectionString);
				}
				if (databaseConnection.DatabaseType.Equals(DatabaseType.Oracle))
				{
					return new OracleConnection(databaseConnection.ConnectionString);
				}
				if (databaseConnection.DatabaseType.Equals(DatabaseType.FoxPro))
				{
					return new OleDbConnection(databaseConnection.ConnectionString);
				}
			}
			return null;
		}
		
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

			if (!mConnections_.ContainsKey(name))
			{
				mConnections_.Add(name, item);
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

			if (!mConnections_.ContainsKey(name))
			{
				mConnections_.Add(name, item);
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

			if (!mConnections_.ContainsKey(name))
			{
				mConnections_.Add(name, item);
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
			
			if (!mConnections_.ContainsKey(name))
			{
				mConnections_.Add(name, item);
			}
		}
		
		#endregion
	}
}
