using System;

namespace MiskoPersist.Enums
{
	public class SessionStatus : MiskoEnum
	{
		#region Fields
		
		private static readonly SessionStatus mNULL_ = new SessionStatus(-1, "", "");
		private static readonly SessionStatus mActive_ = new SessionStatus(0, "", "Active");
		private static readonly SessionStatus mClosed_ = new SessionStatus(1, "", "Closed");
		private static readonly SessionStatus mExpired_ = new SessionStatus(2, "", "Expired");
		
		private static readonly SessionStatus[] mElements_ = new[]
		{
			mNULL_, mActive_, mClosed_, mExpired_
		};
		
		#endregion
		
		#region Properties
		
		public static SessionStatus[] Elements { get { return mElements_; } }
		public static SessionStatus Active { get { return mActive_; } }
		public static SessionStatus Closed { get { return mClosed_; } }
		public static SessionStatus Expired { get { return mExpired_; } }
		
		#endregion
		
		#region Constructors

		public SessionStatus()
		{
		}

		public SessionStatus(Int64 value, String code, String description)
			: base(value, code, description)
		{
		}

		#endregion
	}
}
