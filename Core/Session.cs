﻿using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Diagnostics;
using System.Reflection;
using log4net;
using MiskoPersist.Data;
using MiskoPersist.Enums;
using MiskoPersist.Message.Request;
using MiskoPersist.Resources;

namespace MiskoPersist.Core
{
	public sealed class Session
	{
		private static ILog Log = LogManager.GetLogger(typeof(Session));

		#region Fields

		private DbConnection mConnection_;
		private DbTransaction mTransaction_;
		private List<Persistence> mPersistencePool_ = new List<Persistence>();
		private ErrorMessages mErrorMessages_ = new ErrorMessages();
		private ErrorLevel mStatus_ = ErrorLevel.Success;
		private MessageMode mMessageMode_ = MessageMode.Normal;
		private TimeSpan mSqlExecutionTime_ = TimeSpan.Zero;

		#endregion

		#region Properties

		public DbConnection Connection
		{
			get
			{
				return mConnection_;
			}
		}
		
		public List<Persistence> PersistencePool
		{
			get
			{
				return mPersistencePool_;
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
		
		public TimeSpan SqlExecutionTime
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

		public Session(RequestMessage request)
		{
			mConnection_ = DatabaseConnections.GetConnection(request.Connection ?? "Default");
			mMessageMode_ = request.MessageMode ?? MessageMode.Normal;
			mErrorMessages_.Concatenate(request.Confirmations);
			if (mConnection_ != null)
			{
				mConnection_.Open();
			}
		}
		
		#endregion

		#region Public Methods

		public void BeginTransaction()
		{
			if (mTransaction_ != null)
			{
				Error(ErrorLevel.Error, ErrorStrings.errTransactionAlreadyInProgress);
			}
			else
			{
				mTransaction_ = mConnection_ != null ? Connection.BeginTransaction() : null;
			}
		}

		public void EndTransaction()
		{
			if (mTransaction_ != null)
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
			}
		}

		public void FlushPersistence()
		{
			while (mPersistencePool_.Count > 0)
			{
				mPersistencePool_[mPersistencePool_.Count - 1].Close();
			}
		}

		public void Error(ErrorLevel errorLevel, String message)
		{
			StackFrame stackFrame = new StackFrame(1);
			Error(stackFrame.GetMethod().DeclaringType, stackFrame.GetMethod(), errorLevel, message, null);
		}

		public void Error(ErrorLevel errorLevel, String message, params Object[] parameters)
		{
			StackFrame stackFrame = new StackFrame(1);
			Error(stackFrame.GetMethod().DeclaringType, stackFrame.GetMethod(), errorLevel, message, parameters);
		}

		private void Error(Type clazz, MethodBase method, ErrorLevel errorLevel, String message, params Object[] parameters)
		{
			ErrorMessage errorMessage = new ErrorMessage(clazz, method, errorLevel, message, parameters);

			if (errorLevel.Equals(ErrorLevel.Confirmation) && ErrorMessages.IsConfirmed(errorMessage))
			{
				return;
			}

			if (Status.Value < errorLevel.Value)
			{
				mStatus_ = errorLevel;
			}

			ErrorMessages.Add(errorMessage);

			if (!errorLevel.Equals(ErrorLevel.Warning) && !errorLevel.Equals(ErrorLevel.Information))
			{
				throw new MiskoException("Houston we have a problem!");
			}
		}

		#endregion
	}
}
