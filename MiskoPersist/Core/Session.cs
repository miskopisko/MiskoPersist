using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Diagnostics;
using System.Reflection;
using log4net;
using MiskoPersist.Data.Viewed;
using MiskoPersist.Enums;
using MiskoPersist.Message.Requests;
using MiskoPersist.Resources;

namespace MiskoPersist.Core
{
	public sealed class Session
	{
		private static ILog Log = LogManager.GetLogger(typeof(Session));

		#region Fields

		private readonly Object mLocker_ = new Object();

		#endregion

		#region Properties
		
		public DbConnection Connection
		{
			get;
			private set;
		}
		
		public List<Persistence> PersistencePool
		{
			get;
			private set;
		}
		
		public DbTransaction Transaction
		{
			get;
			private set;
		}
		
		public ErrorLevel Status
		{
			get;
			set;
		}
		
		public MessageMode MessageMode
		{
			get;			
			private set;
		}
		
		public ErrorMessages ErrorMessages
		{
			get;
			private set;
		}
		
		public TimeSpan SqlExecutionTime
		{
			get;
			set;
		}

		#endregion

		#region Constructors

        public Session(DatabaseConnection? databaseConnection)
        {
        	Connection = databaseConnection.HasValue ? databaseConnection.Value.GetConnection() : null;
            PersistencePool = new List<Persistence>();
            Status = ErrorLevel.Success;
            MessageMode = MessageMode.Normal;
            ErrorMessages = new ErrorMessages();
            SqlExecutionTime = TimeSpan.Zero;
        }
		
		#endregion

		#region Public Methods

        public void BeginTransaction()
        {
			BeginTransaction(null);
        }

		public void BeginTransaction(RequestMessage request)
		{
			lock (mLocker_)
			{
				if (Transaction != null)
				{
					Error(ErrorLevel.Error, ErrorStrings.errTransactionAlreadyInProgress);
				}
				else
				{
					if (request != null)
					{
						MessageMode = request.MessageMode ?? MessageMode.Normal;
						ErrorMessages.Add(request.Confirmations);
					}
					
					if (Connection != null)
					{
						if (!Connection.State.Equals(ConnectionState.Open))
						{
							Connection.Open();
						}
						Transaction = Connection.BeginTransaction();
					}
				}
			}
		}

		public void EndTransaction()
		{
			lock (mLocker_)
			{
				if (Transaction != null)
				{
					if (!Status.IsCommitable() || MessageMode.Equals(MessageMode.Trial))
					{
						Transaction.Rollback();
					}
					else
					{
						Transaction.Commit();
					}
					Transaction = null;
				}
			}
		}

		public void FlushPersistence()
		{
			lock (mLocker_)
			{
				while (PersistencePool.Count > 0)
				{
					PersistencePool[PersistencePool.Count - 1].Close();
				}
			}
		}

		public void Error(ErrorLevel errorLevel, String message)
		{
			StackFrame stackFrame = new StackFrame(1, true);
			Error(stackFrame.GetMethod().DeclaringType, stackFrame.GetMethod(), stackFrame.GetFileLineNumber(), errorLevel, message, null);
		}

		public void Error(ErrorLevel errorLevel, String message, params Object[] parameters)
		{
			StackFrame stackFrame = new StackFrame(1, true);
			Error(stackFrame.GetMethod().DeclaringType, stackFrame.GetMethod(), stackFrame.GetFileLineNumber(), errorLevel, message, parameters);
		}

		private void Error(Type clazz, MethodBase method, Int32 lineNumber, ErrorLevel errorLevel, String message, params Object[] parameters)
		{
			ErrorMessage errorMessage = new ErrorMessage(clazz, method, lineNumber, errorLevel, message, parameters);

			lock (mLocker_)
			{
				if (errorLevel.Equals(ErrorLevel.Confirmation) && ErrorMessages.IsConfirmed(errorMessage))
				{
					return;
				}
	
				if (Status.Value < errorLevel.Value)
				{
					Status = errorLevel;
				}
	
				ErrorMessages.Add(errorMessage);
			}

			if (!errorLevel.Equals(ErrorLevel.Warning) && !errorLevel.Equals(ErrorLevel.Information))
			{
				throw new MiskoException("Houston we have a problem!");
			}
		}

		#endregion
	}
}
