using System;
using System.Data.Common;
using System.Data.OleDb;
using System.Data.SQLite;
using MiskoPersist.Data;
using MiskoPersist.Enums;
using MiskoPersist.Resources;
using MySql.Data.MySqlClient;
using Oracle.DataAccess.Client;

namespace MiskoPersist.Core
{
	public static class ServiceLocator
    {
        private static Logger Log = Logger.GetInstance(typeof(ServiceLocator));

        #region Public Methods

        public static DbConnection GetConnection()
        {
            return GetConnection("Default");
        }

        public static DbConnection GetConnection(String name)
        {
            DatabaseConnection databaseConnection = DatabaseConnections.GetDatabaseConnection(name);

            if (databaseConnection != null && databaseConnection.DatabaseType.Equals(DatabaseType.SQLite))
            {
                return GetSqliteConnection(databaseConnection.ConnectionString);
            }
            else if (databaseConnection != null && databaseConnection.DatabaseType.Equals(DatabaseType.MySql))
            {
                return GetMySqlConnection(databaseConnection.ConnectionString);
            }
            else if (databaseConnection != null && databaseConnection.DatabaseType.Equals(DatabaseType.Oracle))
            {
                return GetOracleConnection(databaseConnection.ConnectionString);
            }
            else if (databaseConnection != null && databaseConnection.DatabaseType.Equals(DatabaseType.Postgres))
            {
                return GetPostgresConnection(databaseConnection.ConnectionString);
            }
            else if (databaseConnection != null && databaseConnection.DatabaseType.Equals(DatabaseType.FoxPro))
            {
                return GetFoxProConnection(databaseConnection.ConnectionString);
            }
            else
            {
                throw new MiskoException(ErrorStrings.errInvalidConnectionString, new String[] { name });
            }
        }

        #endregion

        #region Private Methods

        private static DbConnection GetMySqlConnection(String connectionString)
        {
            MySqlConnection connection = new MySqlConnection(connectionString);
            connection.Open();

            return connection;
        }

        private static DbConnection GetOracleConnection(String connectionString)
        {
            OracleConnection connection = new OracleConnection(connectionString);
            connection.Open();

            return connection;
        }

        private static DbConnection GetSqliteConnection(String connectionString)
        {
            SQLiteConnection connection = new SQLiteConnection(connectionString);
            connection.Open();

            return connection;
        }

        private static DbConnection GetPostgresConnection(String connectionString)
        {
            throw new NotImplementedException();
        }

        private static DbConnection GetFoxProConnection(String connectionString)
        {
            DbConnection connection = new OleDbConnection(connectionString);
            connection.Open();
			
            return connection;
        }

        #endregion
    }
}
