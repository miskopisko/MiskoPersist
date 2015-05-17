using System;
using System.Data;
using System.Data.Common;
using System.IO;
using MiskoPersist.Core;
using MiskoPersist.Enums;

namespace MiskoPersist.SVN
{
    public class SvnConnection : DbConnection
    {
        private static Logger Log = Logger.GetInstance(typeof(SvnConnection));

        #region Fields

        private String mConnectionString_ = "";
        private String mDataSource_ = "";
        private String mDatabase_ = "";
        private ConnectionState mConnectionState_ = ConnectionState.Closed;

        private FileInfo mWorkingCopy_;
        private FileInfo mWorkingCopyRoot_;
        private Uri mRepoRoot_;
        private Guid mRepoUUID_;

        #endregion

        #region Properties
        
        public override String ConnectionString { get { return mConnectionString_; } set { mConnectionString_ = value; } }
        public override String DataSource { get { return mDataSource_; } }
        public override String Database { get { return mDatabase_; } }
        public override String ServerVersion { get { return Subversion.InstalledVersion(); } }
        public override ConnectionState State { get { return mConnectionState_; } }

        public FileInfo WorkingCopy { get { return mWorkingCopy_; } }
        public FileInfo WorkingCopyRoot { get { return mWorkingCopyRoot_; } }
        public Uri RepoRoot { get { return mRepoRoot_; } }
        public Guid RepoUUID { get { return mRepoUUID_; } }

        #endregion

        #region Constructors

        public SvnConnection(String workingCopy)
        {
            mConnectionString_ = workingCopy;
            mDataSource_ = workingCopy;
            mDatabase_ = workingCopy;
        }

        #endregion

        #region Override Methods

        protected override DbTransaction BeginDbTransaction(IsolationLevel isolationLevel)
        {
            return new SvnDbTransaction();
        }

        public override void ChangeDatabase(String workingCopy)
        {
            mConnectionString_ = workingCopy;
            mDataSource_ = workingCopy;
            mDatabase_ = workingCopy;
        }

        public override void Close()
        {
            mConnectionState_ = ConnectionState.Closed;
        }

        protected override DbCommand CreateDbCommand()
        {
            return new SvnDbCommand();
        }

        public override void Open()
        {
            mWorkingCopy_ = new FileInfo(mConnectionString_);

            bool revert = false;
            SvnStatus status = new SvnStatus(mWorkingCopy_.FullName);
            if (status.Count.Equals(1) && status.Target.Entries[0].Path.Equals(mWorkingCopy_) && status.Target.Entries[0].WorkingCopyStatus.Status.Equals(SvnStatusType.NotVersioned))
            {
                revert = Subversion.Add(mWorkingCopy_);
            }

            SvnTarget target = new SvnTarget(mWorkingCopy_.FullName.TrimEnd(new char[] { '\\' }));

            if (target.IsSet)
            {
                mRepoUUID_ = target.Entry.Repository.UUID;
                mRepoRoot_ = target.Entry.Repository.Root;
                mWorkingCopyRoot_ = target.Entry.WorkingCopyInfo.AbsolutePath;
            }

            if (revert)
            {
                Subversion.Revert(mWorkingCopy_);
            }

            if (mWorkingCopyRoot_ != null)
            {
                mConnectionState_ = ConnectionState.Open;
            }
        }

        #endregion        
    }
}