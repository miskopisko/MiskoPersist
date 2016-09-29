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
			DbConnection result = null;
			
			if (DatabaseType.Equals(DatabaseType.SQLite))
			{
				result = new SQLiteConnection(ConnectionString);
			}
			else if (DatabaseType.Equals(DatabaseType.MySql))
			{
				result = new MySqlConnection(ConnectionString);
			}
			else if (DatabaseType.Equals(DatabaseType.Oracle))
			{
				result = new OracleConnection(ConnectionString);
			}
			else if (DatabaseType.Equals(DatabaseType.FoxPro))
			{
				result = new OleDbConnection(ConnectionString);
			}
			
			if (result != null)
			{
				result.Open();
			}
			
			return result;
		}
		
		#endregion
		
		#region Private Methods

		

		#endregion
	}
}
