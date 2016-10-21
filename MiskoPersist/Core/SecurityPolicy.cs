using System;
using System.Configuration;
using log4net;

namespace MiskoPersist.Core
{
	public class SecurityPolicy : ConfigurationSection
	{
		private static ILog Log = LogManager.GetLogger(typeof(SecurityPolicy));
		
		#region Fields	
		
		private Boolean mLoginRequired_ = true;
		
		private static SecurityPolicy mInstance_;
		
		#endregion
		
		#region Properties
		
		public static SecurityPolicy Instance
		{
			get
			{
				if (mInstance_ == null)
				{
					mInstance_ = ConfigurationManager.GetSection("SecurityPolicy") as SecurityPolicy ?? new SecurityPolicy();
				}
				return mInstance_;
			}
		}
		
		public Boolean LoginRequired
		{
			get
			{
				return mLoginRequired_;
			}
			set
			{
				mLoginRequired_ = value;
			}
		}
		
		[ConfigurationProperty("MinimumPasswordAge", DefaultValue = (Int16)1, IsRequired = false)]
		public Int16 MinimumPasswordAge
		{
			get
			{
				return (Int16)this["MinimumPasswordAge"];
			}
			set
			{
				this["MinimumPasswordAge"] = value;
			}
		}

		[ConfigurationProperty("MaximumPasswordAge", DefaultValue = (Int16)30, IsRequired = false)]
		public Int16 MaximumPasswordAge
		{
			get
			{
				return (Int16)this["MaximumPasswordAge"];
			}
			set
			{
				this["MaximumPasswordAge"] = value;
			}
		}
		
		[ConfigurationProperty("MinimumPasswordLength", DefaultValue = (Int16)8, IsRequired = false)]
		public Int16 MinimumPasswordLength
		{
			get
			{
				return (Int16)this["MinimumPasswordLength"];
			}
			set
			{
				this["MinimumPasswordLength"] = value;
			}
		}
		
		[ConfigurationProperty("LockOutAttempts", DefaultValue = (Int16)5, IsRequired = false)]
		public Int16 LockOutAttempts
		{
			get
			{
				return (Int16)this["LockOutAttempts"];
			}
			set
			{
				this["LockOutAttempts"] = value;
			}
		}
		
		[ConfigurationProperty("LockOutDuration", DefaultValue = (Int16)15, IsRequired = false)]
		public Int16 LockOutDuration
		{
			get
			{
				return (Int16)this["LockOutDuration"];
			}
			set
			{
				this["LockOutDuration"] = value;
			}
		}
		
		[ConfigurationProperty("ResetLoginCount", DefaultValue = (Int16)15, IsRequired = false)]
		public Int16 ResetLoginCount
		{
			get
			{
				return (Int16)this["ResetLoginCount"];
			}
			set
			{
				this["ResetLoginCount"] = value;
			}
		}
		
		[ConfigurationProperty("SessionTokenExpiry", DefaultValue = (Int16)15, IsRequired = false)]
		public Int16 SessionTokenExpiry
		{
			get
			{
				return (Int16)this["SessionTokenExpiry"];
			}
			set
			{
				this["SessionTokenExpiry"] = value;
			}
		}
		
		#endregion
	}
}
