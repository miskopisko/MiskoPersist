using System;
using System.Data.Common;
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

		public static readonly DatabaseConnection NULL = new DatabaseConnection();

		#endregion

		#region Properties

		public Boolean IsSet
		{
			get;
			set;
		}

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
		
		public String ConnectionString 
		{ 
			get; 
			set; 
		}
		
		#endregion
		
		#region Public Methods
		
		public DbConnection Connect()
		{
			DbConnection result = null;
			
			if (IsSet)
			{
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
