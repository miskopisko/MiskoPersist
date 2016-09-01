using System;
using log4net;
using MiskoPersist.Enums;

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
		
		
		
		#endregion
		
		#region Public Methods

		

		#endregion
	}
}
