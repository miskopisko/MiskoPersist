using System;
using log4net;
using MiskoPersist.Attributes;
using MiskoPersist.Core;
using MiskoPersist.Enums;

namespace MiskoPersist.Data.Stored
{
	public class SessionLog : StoredData
	{
		private static ILog Log = LogManager.GetLogger(typeof(SessionLog));
		
		#region Fields

		

		#endregion

		#region Properties
		
		[Stored]
		public Guid SessionToken
		{
			get;
			set;
		}
		
		[Stored]
		public PrimaryKey Operator
		{
			get;
			set;
		}
		
		[Stored]
		public DateTime LoggedOn
		{
			get;
			set;
		}
		
		[Stored]
		public DateTime? LoggedOff
		{
			get;
			set;
		}
		
		[Stored]
		public DateTime LastTransmitted
		{
			get;
			set;
		}
		
		[Stored]
		public SessionStatus Status
		{
			get;
			set;
		}

		#endregion
		
		#region Constructors

        public SessionLog()
        {
        }

        public SessionLog(Session session, Persistence persistence) 
        	: base(session, persistence)
        {
        }

        #endregion

        #region Override Methods

        public override StoredData Create(Session session)
        {
            PreSave(session, UpdateMode.Insert);
            Persistence.ExecuteAutonomousInsert(session, this, typeof(SessionLog));
            PostSave(session, UpdateMode.Insert);
            return this;
        }

        public override StoredData Store(Session session)
        {
            PreSave(session, UpdateMode.Update);
            Persistence.ExecuteAutonomousUpdate(session, this, typeof(SessionLog));
            PostSave(session, UpdateMode.Update);
            return this;
        }

        public override StoredData Remove(Session session)
        {
            Persistence.ExecuteAutonomousDelete(session, this, typeof(SessionLog));
            PostSave(session, UpdateMode.Delete);
            return this;
        }

        public override void PreSave(Session session, UpdateMode mode)
        {
        }

        public override void PostSave(Session session, UpdateMode mode)
        {
        }

        #endregion

        #region Private Methods



        #endregion

        #region Public Methods
        
        public static SessionLog GetInstanceBySessionToken(Session session, Guid sessionToken)
        {
        	using (Persistence persistence = session.GetPersistence(true))
        	{
				persistence.ExecuteQuery("SELECT * FROM SessionLog WHERE SessionToken = ?", sessionToken);
				return new SessionLog(session, persistence);
        	}
        }

        public static Guid AddNew(Session session, PrimaryKey o)
        {
        	SessionLog sessionLog = new SessionLog();
			sessionLog.SessionToken = Guid.NewGuid();
			sessionLog.Operator = o;
			sessionLog.LoggedOn = DateTime.Now;
			sessionLog.LastTransmitted = DateTime.Now;
			sessionLog.Status = SessionStatus.Active;
			sessionLog.Save(session);
			
			return sessionLog.SessionToken;
        }
        
        public static void LogOff(Session session)
        {
        	using (Persistence persistence = session.GetPersistence())
        	{
        		persistence.ExecuteQuery("UPDATE SessionLog SET Status = ?, LoggedOff = ? WHERE SessionToken = ?", SessionStatus.Closed, DateTime.Now, session.SessionToken);
				session.SessionToken = null;
        	}
        }

        #endregion
	}
}
