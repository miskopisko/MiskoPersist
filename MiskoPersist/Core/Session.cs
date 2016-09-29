using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.OleDb;
using System.Data.SQLite;
using System.Diagnostics;
using System.Reflection;
using System.Threading;
using log4net;
using MiskoPersist.Data.Viewed;
using MiskoPersist.Enums;
using MiskoPersist.Message.Requests;
using MiskoPersist.Persistences;
using MiskoPersist.Resources;
using MySql.Data.MySqlClient;
using Oracle.ManagedDataAccess.Client;

namespace MiskoPersist.Core
{
	public sealed class Session : IDisposable
	{
		private static ILog Log = LogManager.GetLogger(typeof(Session));

		#region Fields
		
		private readonly Object mLocker_ = new Object();
		private readonly DbConnection mConnection_;
		private DbTransaction mTransaction_;
		
		#endregion

		#region Properties
		
		public List<Persistence> PersistencePool
		{
			get;
			private set;
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
		
		public ErrorLevel Status
		{
			get;
			set;
		}
		
		public TimeSpan SqlExecutionTime
		{
			get;
			set;
		}

		#endregion

		#region Constructors

        public Session(DbConnection conn)
        {
        	mConnection_ = conn;
            PersistencePool = new List<Persistence>();
            Status = ErrorLevel.Success;
            MessageMode = MessageMode.Normal;
            ErrorMessages = new ErrorMessages();
            SqlExecutionTime = TimeSpan.Zero;
        }
		
		#endregion

		#region Public Methods
		
		public Persistence GetPersistence()
		{
			if (mConnection_ != null)
			{
				DbCommand command = mConnection_.CreateCommand();
				command.Transaction = mTransaction_;
				
				if (mConnection_ is OracleConnection)
				{
					return new OraclePersistence(this, command);
				}
				if (mConnection_ is MySqlConnection)
				{
					return new MySqlPersistence(this, command);
				}
				if (mConnection_ is SQLiteConnection)
				{
					return new SqlitePersistence(this, command);
				}
				if (mConnection_ is OleDbConnection)
				{
					return new FoxProPersistence(this, command);
				}
			}
			throw new MiskoException("Unable to get datatabe connection");
		}

        public void BeginTransaction()
        {
			BeginTransaction(null);
        }

		public void BeginTransaction(RequestMessage request)
		{
			lock (mLocker_)
			{
				if (mTransaction_ != null)
				{
					Error(ErrorLevel.Error, ErrorStrings.errTransactionAlreadyInProgress);
				}
				else
				{
					Status = ErrorLevel.Success;
					
					if (request != null)
					{
						Thread.CurrentThread.Name = request.WrapperClass.Name;
						
						MessageMode = request.MessageMode ?? MessageMode.Normal;
						ErrorMessages.Add(request.Confirmations);
						
						if (SecurityPolicy.LoginRequired && request.SessionToken == null && !(request is LogonRQ))
						{
							throw new MiskoException("Application requires logon");
						}
						if (SecurityPolicy.LoginRequired && request.SessionToken != null)
						{
							// Validate security here
						}
					}
					
					if (mConnection_ != null)
					{
						mTransaction_ = mConnection_.BeginTransaction();
					}
				}
			}
		}

		public void EndTransaction()
		{
			lock (mLocker_)
			{
				if (mTransaction_ != null)
				{
					if (!Status.IsCommitable() || MessageMode.Equals(MessageMode.Trial))
					{
						mTransaction_.Rollback();
					}
					else
					{
						mTransaction_.Commit();
					}
					mTransaction_.Dispose();
					mTransaction_ = null;
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
		
		#region IDisposable Implementation
		
		public void Dispose()
		{
			FlushPersistence();
			EndTransaction();
			
			lock (mLocker_)
			{
				if (mConnection_ != null && !mConnection_.State.Equals(ConnectionState.Closed))
				{
					mConnection_.Close();
				}
			}
		}
		
		#endregion
	}
}
