using System;
using log4net;
using MiskoPersist.Attributes;
using MiskoPersist.Core;
using MiskoPersist.Enums;
using Sodium;

namespace MiskoPersist.Data.Stored
{
	public class Operator : StoredData
	{
		private static ILog Log = LogManager.GetLogger(typeof(Operator));

		#region Fields

		

		#endregion

		#region Stored Properties

		[Stored(Length = 128)]
		public String Username
		{ 
			get;
			set; 
		}
		
		[Stored(Length = 32)]
		public String Password
		{ 
			get;
			set; 
		}
		
		[Stored(Length = 128)]
		public String FirstName
		{ 
			get;
			set; 
		}
		
		[Stored(Length = 128)]
		public String LastName
		{ 
			get;
			set; 
		}
		
		[Stored]
		public DateTime LastLoginDate
		{
			get;
			set;
		}
		
		[Stored]
		public DateTime LastLoginAttempt
		{
			get;
			set;
		}
		
		[Stored]
		public DateTime PasswordChangeDate
		{
			get;
			set;
		}
		
		[Stored]
		public Boolean Disabled
		{
			get;
			set;
		}
		
		[Stored]
		public Boolean LockedOut
		{
			get;
			set;
		}
		
		[Stored]
		public Boolean PasswordNeverExpires
		{
			get;
			set;
		}
		
		[Stored]
		public Boolean PasswordExpired
		{
			get;
			set;
		}
		
		[Stored]
		public Int32 LoginAttempts
		{
			get;
			set;
		}
		
		#endregion

		#region Other Properties

		

		#endregion

		#region Constructors

		public Operator()
		{
		}

		public Operator(Session session, Persistence persistence) 
			: base(session, persistence)
		{
		}

		#endregion

		#region Override Methods

		public override StoredData Create(Session session)
		{
			PreSave(session, UpdateMode.Insert);
			Persistence.ExecuteInsert(session, this, typeof(Operator));
			PostSave(session, UpdateMode.Insert);
			return this;
		}

		public override StoredData Store(Session session)
		{
			PreSave(session, UpdateMode.Update);
			Persistence.ExecuteUpdate(session, this, typeof(Operator));
			PostSave(session, UpdateMode.Update);
			return this;
		}

		public override StoredData Remove(Session session)
		{
			Persistence.ExecuteDelete(session, this, typeof(Operator));
			PostSave(session, UpdateMode.Delete);
			return this;
		}

		public override void PreSave(Session session, UpdateMode mode)
		{
			if(String.IsNullOrEmpty(Username))
			{
				session.Error(ErrorLevel.Error, "Username name cannot be blank");
			}
			
			if(String.IsNullOrEmpty(FirstName))
			{
				session.Error(ErrorLevel.Error, "First name cannot be blank");
			}

			if(String.IsNullOrEmpty(LastName))
			{
				session.Error(ErrorLevel.Error, "Last name cannot be blank");
			}

            if (String.IsNullOrEmpty(Password))
            {
                session.Error(ErrorLevel.Error, "Password cannot be blank");
            }
        }

		public override void PostSave(Session session, UpdateMode mode)
		{
		}

		#endregion

		#region Private Methods

		private void SetOperatorUnlocked(Session session)
		{
			Persistence.ExecuteAutonomousUpdate(session, "UPDATE Operator SET LockedOut = ? WHERE Id = ?", LockedOut, Id);
		}
		
		private void ResetLoginAttempts(Session session)
		{
			Persistence.ExecuteAutonomousUpdate(session, "UPDATE Operator SET LoginAttempts = ? WHERE Id = ?", LoginAttempts, Id);
		}
		
		private void SetLoginFalied(Session session)
		{
			Persistence.ExecuteAutonomousUpdate(session, "UPDATE Operator SET LastLoginAttempt = ?, LoginAttempts = ?, LockedOut = ? WHERE Id =  ?", LastLoginAttempt, LoginAttempts, LockedOut, Id);
		}
		
		private void SetPasswordExpired(Session session)
		{
			Persistence.ExecuteAutonomousUpdate(session, "UPDATE Operator SET PasswordExpired = ? WHERE Id = ?", PasswordExpired, Id);
		}
		
		private void SetLoginPassed(Session session)
		{
			Persistence.ExecuteAutonomousUpdate(session, "UPDATE Operator SET LastLoginDate = ?, LastLoginAttempt = ?, LoginAttempts = ? WHERE Id = ?", LastLoginDate, LastLoginAttempt, LoginAttempts, Id);
		}
		
		#endregion

		#region Public Methods
		
		public Boolean ValidatePassword(String password)
		{
			return PasswordHash.ScryptHashStringVerify(Password, password);
		}
		
		public void FetchByUsername(Session session, String name)
		{
			using (Persistence persistence = session.GetPersistence())
			{
				persistence.ExecuteQuery("SELECT * FROM Operator WHERE Username = ?", name);
				Set(session, persistence);
			}
		}

		public void Logon(Session session, String password)
		{
			if (Disabled)
			{
				session.Error(ErrorLevel.Error, "Account is disabled.");
			}
			
			if (LockedOut)
			{
				if (SecurityPolicy.Instance.LockOutDuration.Equals(0))
				{
					session.Error(ErrorLevel.Error, "Account locked out. Contact administrator.");
				}
				else if (DateTime.Now.Subtract(LastLoginAttempt).Minutes >= SecurityPolicy.Instance.LockOutDuration)
				{
					LockedOut = false;
					SetOperatorUnlocked(session);
				}
				else
				{
					session.Error(ErrorLevel.Error, "Account is locked out. Wait {0} minutes and try again.", (SecurityPolicy.Instance.LockOutDuration - DateTime.Now.Subtract(LastLoginAttempt).Minutes));
				}
			}
			
			if (DateTime.Now.Subtract(LastLoginAttempt).Minutes > SecurityPolicy.Instance.ResetLoginCount)
			{
				LoginAttempts = 0;
				ResetLoginAttempts(session);
			}

			if (ValidatePassword(password))
			{
				PasswordExpired = PasswordExpired || (!PasswordNeverExpires && SecurityPolicy.Instance.MaximumPasswordAge > 0 && DateTime.Now.Subtract(PasswordChangeDate).Days > SecurityPolicy.Instance.MaximumPasswordAge); 
				
				if (PasswordExpired)
				{
					SetPasswordExpired(session);
					session.Error(ErrorLevel.Error, "Password has expired.");
				}
				else
				{
					LastLoginDate = DateTime.Now;
					LastLoginAttempt = DateTime.Now;
					LoginAttempts = 0;
					SetLoginPassed(session);
					session.SessionToken = SessionLog.AddNew(session, Id);
				}
			}
			else
			{
				LastLoginAttempt = DateTime.Now;
				LoginAttempts++;
				LockedOut = LoginAttempts >= SecurityPolicy.Instance.LockOutAttempts;
				SetLoginFalied(session);
				
				if (LockedOut)
				{
					session.Error(ErrorLevel.Error, "Account is locked out. Wait {0} minutes and try again.", (SecurityPolicy.Instance.LockOutDuration - DateTime.Now.Subtract(LastLoginAttempt).Minutes));
				}
				else
				{
					session.Error(ErrorLevel.Error, "Login failed.");
				}
			}
		}
		
		public void SetPassword(Session session, String newPassword, String confirmPassword)
		{			
			if (String.Equals(newPassword, confirmPassword))
		    {
				if (SecurityPolicy.Instance.MinimumPasswordAge > 0 && (DateTime.Now.Subtract(PasswordChangeDate).Days < SecurityPolicy.Instance.MinimumPasswordAge))
				{
					session.Error(ErrorLevel.Error, "Minimum password age not achieved.");
				}
				
				if (SecurityPolicy.Instance.MinimumPasswordLength > 0 && newPassword.Length < SecurityPolicy.Instance.MinimumPasswordLength)
				{
					session.Error(ErrorLevel.Error, "New password must be at least {0} characters.", SecurityPolicy.Instance.MinimumPasswordLength);
				}
				
				Password = PasswordHash.ScryptHashString(newPassword);
				PasswordExpired = false;
				PasswordChangeDate = DateTime.Now;
		    }
			else
			{
				session.Error(ErrorLevel.Error, "Passwords do not match.");
			}
		}

		#endregion
	}
}
