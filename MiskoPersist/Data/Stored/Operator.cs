using System;
using log4net;
using MiskoPersist.Attributes;
using MiskoPersist.Core;
using MiskoPersist.Data.Stored;
using MiskoPersist.Enums;
using MiskoPersist.Tools;

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
        	
        	if(String.IsNullOrEmpty(Password))
        	{
        		session.Error(ErrorLevel.Error, "Cannot have blank password");
        	}
        	
            if(String.IsNullOrEmpty(FirstName))
            {
                session.Error(ErrorLevel.Error, "First name cannot be blank");
            }

            if(String.IsNullOrEmpty(LastName))
            {
                session.Error(ErrorLevel.Error, "Last name cannot be blank");
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
        
        public void FetchByUsername(Session session, String name)
        {
			using (Persistence persistence = session.GetPersistence())
			{
				persistence.ExecuteQuery("SELECT * FROM Operator WHERE Username = ?", name);
				Set(session, persistence);
			}
        }

        public Guid Logon(Session session, String password)
        {
			Guid newSessionToken = Guid.Empty;

			if (Disabled)
        	{
				session.Error(ErrorLevel.Error, "Account is disabled.");
        	}
        	
        	if (LockedOut)
        	{
        		if (SecurityPolicy.LockOutDuration.Equals(0))
        		{
					session.Error(ErrorLevel.Error, "Account locked out. Contact administrator.");
        		}
        		else if (DateTime.Now.Subtract(LastLoginAttempt).Minutes >= SecurityPolicy.LockOutDuration)
        		{
        			LockedOut = false;
        			SetOperatorUnlocked(session);
        		}
        		else
        		{
        			session.Error(ErrorLevel.Error, "Account is locked out. Wait {0} minutes and try again.", (SecurityPolicy.LockOutDuration - DateTime.Now.Subtract(LastLoginAttempt).Minutes));
        		}
        	}
        	
        	if (DateTime.Now.Subtract(LastLoginAttempt).Minutes > SecurityPolicy.ResetLoginCount)
			{
				LoginAttempts = 0;
				ResetLoginAttempts(session);
			}
        	
        	if (PasswordHash.ValidatePassword(password, Password))
        	{
        		if (!PasswordNeverExpires && SecurityPolicy.MaximumPasswordAge > 0 && DateTime.Now.Subtract(PasswordChangeDate).Days > SecurityPolicy.MaximumPasswordAge)
        		{
					SetPasswordExpired(session);
					session.Error(ErrorLevel.Error, "Password change required.");
        		}
        		else if (PasswordExpired)
        		{
        			session.Error(ErrorLevel.Error, "Password has expired.");
        		}
        		else
        		{
					newSessionToken = Guid.NewGuid();
					LastLoginDate = DateTime.Now;
					LastLoginAttempt = DateTime.Now;
					LoginAttempts = 0;
					SetLoginPassed(session);
        		}
        	}
        	else
        	{
				LastLoginAttempt = DateTime.Now;
				LoginAttempts++;
				LockedOut = LoginAttempts >= SecurityPolicy.LockOutAttempts;
				SetLoginFalied(session);
				
				if (LockedOut)
				{
					session.Error(ErrorLevel.Error, "Account is locked out. Wait {0} minutes and try again.", (SecurityPolicy.LockOutDuration - DateTime.Now.Subtract(LastLoginAttempt).Minutes));
				}
				else
				{
					session.Error(ErrorLevel.Error, "Login failed.");
				}
        	}
        	
        	return newSessionToken;
        }

        #endregion
    }
}
