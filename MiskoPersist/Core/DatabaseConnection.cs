using System;
using System.Data.Common;
using System.Data.OleDb;
using System.Data.SQLite;
using log4net;
using MiskoPersist.Enums;
using MySql.Data.MySqlClient;
using Oracle.ManagedDataAccess.Client;

namespace MiskoPersist.Core
{
	public struct DatabaseConnection
	{
		private static ILog Log = LogManager.GetLogger(typeof(DatabaseConnection));
		
		#region Fields

		

		#endregion

		#region Properties

		public String Name 
		{ 
			get; 
			set; 
		}
		
		public DatabaseType DatabaseType 
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
		
		#region Public Methods
		
		public DbConnection GetConnection()
		{
			if (DatabaseType.Equals(DatabaseType.SQLite))
			{
				return new SQLiteConnection(ConnectionString);
			}
			if (DatabaseType.Equals(DatabaseType.MySql))
			{
				return new MySqlConnection(ConnectionString);
			}
			if (DatabaseType.Equals(DatabaseType.Oracle))
			{
				return new OracleConnection(ConnectionString);
			}
			if (DatabaseType.Equals(DatabaseType.FoxPro))
			{
				return new OleDbConnection(ConnectionString);
			}
			return null;
		}
		
		#endregion
		
		#region Private Methods

		

		#endregion
	}
}
