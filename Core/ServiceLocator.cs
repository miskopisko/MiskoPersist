using System;
using System.Data.Common;
using System.Data.OleDb;
using System.Data.SQLite;
using MiskoPersist.Enums;
using MiskoPersist.Resources;
using MiskoPersist.SVN;
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
            ConnectionSettings connectionSettings = ConnectionSettings.GetConnectionSettings(name);

            if (connectionSettings != null && connectionSettings.ConnectionType.Equals(ConnectionType.SQLite))
            {
                return GetSqliteConnection(connectionSettings.ConnectionString);
            }
            else if (connectionSettings != null && connectionSettings.ConnectionType.Equals(ConnectionType.MySql))
            {
                return GetMysqlConnection(connectionSettings.ConnectionString);
            }
            else if (connectionSettings != null && connectionSettings.ConnectionType.Equals(ConnectionType.Oracle))
            {
                return GetOracleConnection(connectionSettings.ConnectionString);
            }
            else if (connectionSettings != null && connectionSettings.ConnectionType.Equals(ConnectionType.Postgres))
            {
                return GetPostgresConnection(connectionSettings.ConnectionString);
            }
            else if (connectionSettings != null && connectionSettings.ConnectionType.Equals(ConnectionType.FoxPro))
            {
                return GetFoxProConnection(connectionSettings.ConnectionString);
            }
            else if (connectionSettings != null && connectionSettings.ConnectionType.Equals(ConnectionType.SVN))
            {
                return GetSvnConnection(connectionSettings.ConnectionString);
            }
            else
            {
                throw new MiskoException(ErrorStrings.errInvalidConnectionString, new String[] { name });
            }
        }

        #endregion

        #region Private Methods

        private static DbConnection GetMysqlConnection(String connectionString)
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
            
            DbCommand command = connection.CreateCommand();
			command.CommandText = "SET EXCLUSIVE OFF";
			command.ExecuteNonQuery();
			
            return connection;
        }

        private static DbConnection GetSvnConnection(String connectionString)
        {
            DbConnection connection = SvnConnectionPool.GetByConnectionString(connectionString);

            if(connection == null)
            {
                connection = new SvnConnection(connectionString);
                connection.Open();
                SvnConnectionPool.AddConnection((SvnConnection)connection);
            }

            return connection;
        }

        #endregion
    }
}
