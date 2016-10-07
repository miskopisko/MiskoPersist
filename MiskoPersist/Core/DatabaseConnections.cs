using System;
using System.Collections.Generic;
using System.Data;
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
		
		
		
		#endregion
		
		#region Constructors
		
		
		
		#endregion
		
		#region Static Methods
		
		public static DatabaseConnection GetDatabaseConnection(String name)
		{
			return mConnections_.ContainsKey(name ?? "Default") ? mConnections_[name ?? "Default"] : DatabaseConnection.NULL;
		}
		
		public static void Clear()
		{
			mConnections_.Clear();
		}
		
		public static IEnumerable<DatabaseConnection> GetConnections()
		{
			return mConnections_.Values;
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
			
			MySqlConnectionStringBuilder connectionstringBuilder = new MySqlConnectionStringBuilder();
			connectionstringBuilder.Server = server;
			connectionstringBuilder.Database = datasource;
			connectionstringBuilder.UserID = username;
			connectionstringBuilder.Password = password;
			connectionstringBuilder.Pooling = true;
			connectionstringBuilder.MaximumPoolSize = 10;
			connectionstringBuilder.MinimumPoolSize = 5;
			connectionstringBuilder.ConvertZeroDateTime = true;
			
			item.ConnectionString = connectionstringBuilder.ToString();

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
			
			OracleConnectionStringBuilder connectionStringBuilder = new OracleConnectionStringBuilder();
			connectionStringBuilder.Pooling = true;
			connectionStringBuilder.MaxPoolSize = 10;
			connectionStringBuilder.MinPoolSize = 5;
			connectionStringBuilder.DataSource = "(DESCRIPTION=(ADDRESS_LIST=(ADDRESS=(PROTOCOL=TCP)(HOST=" + host + ")(PORT=" + port + ")))(CONNECT_DATA=(SERVER=DEDICATED)(SERVICE_NAME=" + datasource + ")))";
			connectionStringBuilder.UserID = username;
			connectionStringBuilder.Password = password;
			
			item.ConnectionString = connectionStringBuilder.ToString();

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
			
			SQLiteConnectionStringBuilder connectionStringBuilder = new SQLiteConnectionStringBuilder();
			connectionStringBuilder.FullUri = datasource;
			connectionStringBuilder.DefaultIsolationLevel = IsolationLevel.ReadCommitted;
			connectionStringBuilder.DateTimeFormatString = "yyyy-MM-dd HH:mm:ss";
			connectionStringBuilder.DefaultTimeout = 10;
			connectionStringBuilder.Pooling = true;
			connectionStringBuilder.FailIfMissing = true;
			connectionStringBuilder.LegacyFormat = false;
			connectionStringBuilder.JournalMode = SQLiteJournalModeEnum.Wal;
			connectionStringBuilder.SyncMode = SynchronizationModes.Normal;
			connectionStringBuilder.DateTimeKind = DateTimeKind.Local;
			connectionStringBuilder.ForeignKeys = true;
			connectionStringBuilder.UseUTF16Encoding = true;
			
			item.ConnectionString = connectionStringBuilder.ToString();

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
			
			OleDbConnectionStringBuilder connectionStringBuilder = new OleDbConnectionStringBuilder();
			connectionStringBuilder.Provider = "vfpoledb.1";
			connectionStringBuilder.DataSource = datasource;

			item.ConnectionString = connectionStringBuilder.ToString();
			
			if (!mConnections_.ContainsKey(name))
			{
				mConnections_.Add(name, item);
			}
		}
		
		#endregion
	}
}
