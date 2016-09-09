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
        public String Username { get; set; }
        [Stored(Length = 32)]
        public String Password { get; set; }
        [Stored(Length = 128)]
        public String FirstName { get; set; }
        [Stored(Length = 128)]
        public String LastName { get; set; }

        #endregion

        #region Other Properties

        

        #endregion

        #region Constructors

        public Operator()
        {
        }

        public Operator(Session session, Persistence persistence) : base(session, persistence)
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



        #endregion

        #region Public Methods
        
        public void FetchByUsername(Session session, String name)
        {
			Persistence persistence = Persistence.GetInstance(session);
			persistence.ExecuteQuery("SELECT * FROM Operator WHERE Username = ?", name);
			Set(session, persistence);
			persistence.Close();
			persistence = null;
        }

        public Guid Logon(Session session, String password)
        {			
        	if(!PasswordHash.ValidatePassword(password, Password))
        	{
        		session.Error(ErrorLevel.Error, "Invalid username or password. Please try again.");
        	}
        	
        	return Guid.Empty;
        }

        #endregion
    }
}
