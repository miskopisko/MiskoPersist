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

		#endregion
		
		#region Constructors

        public SessionLog()
        {
        }

        public SessionLog(Session session, Persistence persistence) : base(session, persistence)
        {
        }

        #endregion

        #region Override Methods

        public override StoredData Create(Session session)
        {
            PreSave(session, UpdateMode.Insert);
            Persistence.ExecuteInsert(session, this, typeof(SessionLog));
            PostSave(session, UpdateMode.Insert);
            return this;
        }

        public override StoredData Store(Session session)
        {
            PreSave(session, UpdateMode.Update);
            Persistence.ExecuteUpdate(session, this, typeof(SessionLog));
            PostSave(session, UpdateMode.Update);
            return this;
        }

        public override StoredData Remove(Session session)
        {
            Persistence.ExecuteDelete(session, this, typeof(SessionLog));
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

        

        #endregion
	}
	
	/* SQLITE 
	CREATE TABLE SessionLog 
	(
    Id              INTEGER  PRIMARY KEY AUTOINCREMENT UNIQUE NOT NULL,
    SessionToken    VARCHAR (36) NOT NULL,
    Operator        INTEGER  NOT NULL,
    LoggedOn        DATE     NOT NULL,
    LoggedOff       DATETIME,
    LastTransmitted DATETIME NOT NULL,
    DtCreated       DATETIME NOT NULL DEFAULT (DATETIME('NOW') ),
    DtModified      DATETIME NOT NULL DEFAULT (DATETIME('NOW') ),
    RowVer          INTEGER  NOT NULL DEFAULT (0) 
	);
	*/
}
