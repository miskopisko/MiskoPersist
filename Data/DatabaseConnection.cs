using System;
using System.Data;
using log4net;
using MiskoPersist.Attributes;
using MiskoPersist.Core;
using MiskoPersist.Enums;

namespace MiskoPersist.Data
{
	public class DatabaseConnection : ViewedData
	{
		private static ILog Log = LogManager.GetLogger(typeof(DatabaseConnection));
		
		#region Fields

		

        #endregion

        #region Viewed Properties

        [Viewed]
        public String Name { get; set; }
		[Viewed]
        public DatabaseType DatabaseType { get; set; }
		[Viewed]
        public String Server { get; set; }
		[Viewed]
        public Int32? Port { get; set; }
		[Viewed]
        public String Datasource { get; set; }
		[Viewed]
        public String Username { get; set; }
		[Viewed]
        public String Password { get; set; }
		[Viewed]
        public String ConnectionString { get; set; }	
        
        #endregion
        
        #region Properties
        
        public ConnectionState State
        {
        	get
        	{
        		return ServiceLocator.GetConnection(Name).State;
        	}
        }
        
        #endregion
	}
}
