using System;
using log4net;

namespace MiskoPersist.Core
{
	public static class SecurityPolicy
	{
		private static ILog Log = LogManager.GetLogger(typeof(SecurityPolicy));
		
		#region Fields
		
		private static Boolean mLoginRequired_ = false;
		private static Int16 mMinimumPasswordAge_ = 0;
		private static Int16 mMaximumPasswordAge_ = 0;
		private static Int16 mMinimumPasswordLength_ = 0;
		private static Int16 mLockOutAttempts_ = 0;
		private static Int16 mLockOutDuration_ = 0;
		private static Int16 mResetLoginCount_ = 0;
		private static Int16 mSessionTokenExpiry_ = 0;
		
		#endregion
		
		#region Properties
		
		public static Boolean LoginRequired
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
		
		public static Int16 MinimumPasswordAge
		{
			get
			{
				return mMinimumPasswordAge_;
			}
			set
			{
				mMinimumPasswordAge_ = value;
			}
		}

		public static Int16 MaximumPasswordAge
		{
			get
			{
				return mMaximumPasswordAge_;
			}
			set
			{
				mMaximumPasswordAge_ = value;
			}
		}
		
		public static Int16 MinimumPasswordLength
		{
			get
			{
				return mMinimumPasswordLength_;
			}
			set
			{
				mMinimumPasswordLength_ = value;
			}
		}
		
		public static Int16 LockOutAttempts
		{
			get
			{
				return mLockOutAttempts_;
			}
			set
			{
				mLockOutAttempts_ = value;
			}
		}
		
		public static Int16 LockOutDuration
		{
			get
			{
				return mLockOutDuration_;
			}
			set
			{
				mLockOutDuration_ = value;
			}
		}
		
		public static Int16 ResetLoginCount
		{
			get
			{
				return mResetLoginCount_;
			}
			set
			{
				mResetLoginCount_ = value > 0 ? value : (Int16)1;
			}
		}
		
		public static Int16 SessionTokenExpiry
		{
			get
			{
				return mSessionTokenExpiry_;
			}
			set
			{
				mSessionTokenExpiry_ = value;
			}
		}
		
		#endregion
	}
}
