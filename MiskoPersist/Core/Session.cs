using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.OleDb;
using System.Data.SQLite;
using System.Diagnostics;
using System.Reflection;
using log4net;
using MiskoPersist.Data.Stored;
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
		private readonly DatabaseConnection mDatabaseConnection_;
		private readonly DbConnection mConnection_;
		private DbTransaction mTransaction_;
		private List<Persistence> mPersistencePool_ = new List<Persistence>();
		
		#endregion

		#region Properties
		
		internal Guid? SessionToken
		{
			get;
			set;
		}
		
		internal MessageMode MessageMode
		{
			get;			
			private set;
		}
		
		internal ErrorMessages ErrorMessages
		{
			get;
			private set;
		}
		
		internal ErrorLevel Status
		{
			get;
			set;
		}
		
		internal TimeSpan SqlExecutionTime
		{
			get;
			set;
		}

		#endregion

		#region Constructors

		public Session(DatabaseConnection databaseConnection)
		{
			mDatabaseConnection_ = databaseConnection;
			mConnection_ = databaseConnection.Connect();
			
			Status = ErrorLevel.Success;
			MessageMode = MessageMode.Normal;
			ErrorMessages = new ErrorMessages();
			SqlExecutionTime = TimeSpan.Zero;
		}
		
		#endregion

		#region Public Methods
		
		public Persistence GetPersistence(Boolean isAutonomous = false)
		{
			if (mDatabaseConnection_.IsSet)
			{
				DbCommand command;
				
				if (isAutonomous)
				{
					DbConnection autonomousConnection = mDatabaseConnection_.Connect();
					command = autonomousConnection.CreateCommand();
					command.Transaction = autonomousConnection.BeginTransaction();
				}
				else
				{
					command = mConnection_.CreateCommand();
					command.Transaction = mTransaction_;	
				}
								
				if (mConnection_ is OracleConnection)
				{
					return new OraclePersistence(this, command, isAutonomous);
				}
				if (mConnection_ is MySqlConnection)
				{
					return new MySqlPersistence(this, command, isAutonomous);
				}
				if (mConnection_ is SQLiteConnection)
				{
					return new SqlitePersistence(this, command, isAutonomous);
				}
				if (mConnection_ is OleDbConnection)
				{
					return new FoxProPersistence(this, command, isAutonomous);
				}
			}
			throw new MiskoException("Unable to get datatabe connection");
		}

		internal void AddPersistence(Persistence persistence)
		{
			mPersistencePool_.Add(persistence);
		}

		internal void RemovePersistence(Persistence persistence)
		{
			mPersistencePool_.Remove(persistence);
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
						MessageMode = request.MessageMode ?? MessageMode.Normal;
						ErrorMessages.Add(request.Confirmations);
						SessionToken = request.SessionToken;
						
						if (SecurityPolicy.Instance.LoginRequired && !SessionToken.HasValue && !request.SecurityExempt)
						{
							Error(ErrorLevel.Error, "Invalid session token.");
						}
						if (SecurityPolicy.Instance.LoginRequired && SessionToken.HasValue && !request.SecurityExempt)
						{
							SessionLog sessionLog = SessionLog.GetInstanceBySessionToken(this, SessionToken.Value);
							
							if (sessionLog.IsSet && sessionLog.Status.Equals(SessionStatus.Active))
							{
								if (SecurityPolicy.Instance.SessionTokenExpiry > 0 && DateTime.Now.Subtract(sessionLog.LastTransmitted).Minutes > SecurityPolicy.Instance.SessionTokenExpiry)
								{
									sessionLog.LastTransmitted = DateTime.Now;
									sessionLog.LoggedOff = DateTime.Now;
									sessionLog.Status = SessionStatus.Expired;
									sessionLog.Save(this);
									
									Error(ErrorLevel.Error, "Invalid session token.");
								}
								
								sessionLog.LastTransmitted = DateTime.Now;
								sessionLog.Save(this);
							}
							else
							{
								Error(ErrorLevel.Error, "Invalid session token.");
							}
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

		internal void FlushPersistence()
		{
			lock (mLocker_)
			{
				while (mPersistencePool_.Count > 0)
				{
					mPersistencePool_[mPersistencePool_.Count - 1].Dispose();
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
					mConnection_.Dispose();
				}
			}
		}
		
		#endregion
	}
}
