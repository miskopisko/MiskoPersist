using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Diagnostics;
using System.Reflection;
using MiskoPersist.Enums;
using MiskoPersist.Resources;

namespace MiskoPersist.Core
{
    public sealed class Session
    {
        private static readonly Logger Log = Logger.GetInstance(typeof(Session));

        #region Fields

        private readonly DbConnection mConn_;
        private readonly String mConnectionName_;
        private Boolean mTransactionInProgress_ = false;
        private List<Persistence> MiskoPersistencePool_ = new List<Persistence>();
        private DbTransaction mTransaction_;
        private ErrorMessages mErrorMessages_ = new ErrorMessages();
        private ErrorLevel mStatus_ = ErrorLevel.Success;
        private MessageMode mMessageMode_ = MessageMode.Normal;
        private long mSqlExecutionTime_ = 0;

        #endregion

        #region Properties

        public DbConnection Connection
        {
        	get
        	{
        		return mConn_;
        	}
        }
        
        public String ConnectionName
        {
        	get
        	{
        		return mConnectionName_;
        	}
        }
        
        public Boolean TransactionInProgress
        {
        	get
        	{
        		return mTransactionInProgress_;
        	}
        }
        
        public List<Persistence> PersistencePool
        {
        	get
        	{
        		return MiskoPersistencePool_;
        	}
        }
        
        public DbTransaction Transaction
        {
        	get
        	{
        		return mTransaction_;
        	}
        }
        
        public ErrorLevel Status
        {
        	get
        	{
        		return mStatus_;
        	}
        	set
        	{
        		mStatus_ = value;
        	}
        }
        
        public MessageMode MessageMode
        {
        	get
        	{
        		return mMessageMode_;
        	}
        	set
        	{
        		mMessageMode_ = value;
        	}
        }
        
        public ErrorMessages ErrorMessages
        {
        	get
        	{
        		return mErrorMessages_;
        	}
        }
        
        public long SqlExecutionTime
        {
        	get
        	{
        		return mSqlExecutionTime_;
        	}
        	set
        	{
        		mSqlExecutionTime_ = value;
        	}
        }

        #endregion

        #region Constructors

        public Session(String connectionName)
        {
            mConnectionName_ = connectionName;
            mConn_ = ServiceLocator.GetConnection(connectionName);
        }

        #endregion

        #region Public Methods

        public void BeginTransaction()
        {
            if (mTransactionInProgress_)
            {
                Error(ErrorLevel.Error, ErrorStrings.errTransactionAlreadyInProgress);
            }
            else
            {
                mTransactionInProgress_ = true;
                mTransaction_ = mConn_.BeginTransaction();
            }
        }

        public void EndTransaction()
        {
            if (TransactionInProgress)
            {
                if (!Status.IsCommitable || MessageMode.Equals(MessageMode.Trial))
                {
                    mTransaction_.Rollback();
                }
                else
                {
                    mTransaction_.Commit();
                }

                mTransaction_ = null;
                mTransactionInProgress_ = false;
            }
        }

        public void FlushPersistence()
        {
            while (MiskoPersistencePool_.Count > 0)
            {
                MiskoPersistencePool_[MiskoPersistencePool_.Count - 1].Close();
            }
        }

        public void Error(ErrorLevel errorLevel, String message)
        {
            StackFrame stackFrame = new StackFrame(1);
            Error(stackFrame.GetMethod().DeclaringType, stackFrame.GetMethod(), errorLevel, message, null);
        }

        public void Error(ErrorLevel errorLevel, String message, Object[] parameters)
        {
            StackFrame stackFrame = new StackFrame(1);
            Error(stackFrame.GetMethod().DeclaringType, stackFrame.GetMethod(), errorLevel, message, parameters);
        }

        private void Error(Type clazz, MethodBase method, ErrorLevel errorLevel, String message, Object[] parameters)
        {
            ErrorMessage errorMessage = new ErrorMessage(clazz, method, errorLevel, message, parameters);

            if (errorLevel.Equals(ErrorLevel.Confirmation) && ErrorMessages.Contains(errorMessage) && ErrorMessages[ErrorMessages.IndexOf(errorMessage)].Confirmed.Value)
            {
                return;
            }

            if (Status.Value < errorLevel.Value)
            {
                mStatus_ = errorLevel;
            }

            ErrorMessages.Add(errorMessage);

            if (!errorLevel.Equals(ErrorLevel.Warning) && !errorLevel.Equals(ErrorLevel.Info))
            {
                if (errorLevel.Equals(ErrorLevel.Error))
                {
                    Log.Error(errorMessage.Message);
                }
                throw new MiskoException("Houston we have a problem!");
            }
        }

        #endregion
    }
}
