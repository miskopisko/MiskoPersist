using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.SQLite;
using System.Diagnostics;
using System.Text.RegularExpressions;
using log4net;
using MiskoPersist.Data.Stored;
using MiskoPersist.Enums;
using MiskoPersist.MoneyType;
using MiskoPersist.Resources;
using MySql.Data.MySqlClient;
using Oracle.ManagedDataAccess.Client;

namespace MiskoPersist.Core
{
	public abstract class Persistence : IDisposable
	{
		private static ILog Log = LogManager.GetLogger(typeof(Persistence));

		#region Fields

		protected Session mSession_ = null;
		protected DbDataReader mRs_ = null;
		protected DbCommand mCommand_ = null;
		protected String mSql_ = "";
		protected List<Object> mParameters_ = new List<Object>();
		protected Int32 mRecordCount_ = 0;
		protected String mParameterString_;
		protected Boolean mAutonomous_ = false;

		#endregion

		#region Properties
		
		public Boolean IsEof
		{
			get;
			private set;
		}
		
		public Int32 RecordCount
		{
			get
			{
				while (!IsEof)
				{
					Next();
				}
				return mRecordCount_;
			}
		}

		#endregion

		#region Constructors

		protected Persistence(Session session, DbCommand command, Boolean autonomous)
		{
			mSession_ = session;
			mCommand_ = command;
			mAutonomous_ = autonomous;

			mSession_.AddPersistence(this);
		}

		#endregion

		#region Public Methods

		public void Next()
		{
			IsEof = !mRs_.Read();
			mRecordCount_++;
		}

		public void SetSql(String sql)
		{
			SetSql(sql, null);
		}

		public void SetSql(String sql, params Object[] parameters)
		{
			mSql_ = sql;
			if (parameters != null)
			{
				mParameters_.AddRange(parameters);
			}
		}

		public void SqlWhere(Boolean condition, String expression, params Object[] parameters)
		{
			if (condition)
			{
				mSql_ = mSql_ + (!mSql_.Contains("WHERE") ? " WHERE " : " AND ") + expression;
				if (parameters != null)
				{
					mParameters_.AddRange(parameters);
				}
			}
		}

		public void SqlOrderBy(String columnName)
		{
			SqlOrderBy(columnName, SqlSortDirection.Ascending);
		}

		public void SqlOrderBy(String columnName, SqlSortDirection sortDirection)
		{
			mSql_ = mSql_ + (!mSql_.Contains("ORDER BY") ? " ORDER BY " : ", ") + columnName + sortDirection.Code;
		}

		#endregion

		#region Execute Query Methods

		public void ExecuteQuery()
		{
			ExecuteQuery(mSql_, mParameters_.ToArray());
		}

		public void ExecuteQuery(String sql)
		{
			ExecuteQuery(sql, null);
		}

		public void ExecuteQuery(String sql, params Object[] parameters)
		{
			if (String.IsNullOrEmpty(sql))
			{
				mSession_.Error(ErrorLevel.Error, ErrorStrings.errSqlNotSet);
			}
			
			mSql_ = sql;
			if (parameters != null)
			{
				mParameters_.AddRange(parameters);
			}

			mCommand_.CommandText = mSql_;
			SetParameters();
			
			mCommand_.Prepare();
			
			Log.Debug("Command: " + mCommand_.CommandText);
			Log.Debug("Parameters: " + mParameterString_);
			
			Stopwatch timer = Stopwatch.StartNew();
			mRs_ = mCommand_.ExecuteReader();
			IsEof = !mRs_.Read();
			mRecordCount_ = 0;
			timer.Stop();
			mSession_.SqlExecutionTime = mSession_.SqlExecutionTime.Add(timer.Elapsed);
			
			Log.Debug("Execution Time: " + timer.Elapsed);
		}
		
		#endregion
		
		#region Execute Update Methods 
		
		public static Int32 ExecuteAutonomousUpdate(Session session, String sql)
		{
			return ExecuteAutonomousUpdate(session, sql, null);
		}
		
		public static Int32 ExecuteAutonomousUpdate(Session session, String sql, params Object[] parameters)
		{
			using (Persistence persistence = session.GetPersistence(true))
			{
				return persistence.ExecuteUpdate(sql, parameters);
			}
		}
		
		public static Int32 ExecuteUpdate(Session session, String sql)
		{
			return ExecuteUpdate(session, sql, null);
		}

		public static Int32 ExecuteUpdate(Session session, String sql, params Object[] parameters)
		{
			using (Persistence persistence = session.GetPersistence())
			{
				return persistence.ExecuteUpdate(sql, parameters);
			}
		}
		
		public static void ExecuteAutonomousUpdate(Session session, StoredData clazz, Type type)
		{
			using (Persistence persistence = session.GetPersistence(true))
			{
				persistence.ExecuteUpdate(clazz, type);
			}
		}

		public static void ExecuteUpdate(Session session, StoredData clazz, Type type)
		{
			using (Persistence persistence = session.GetPersistence())
			{
				persistence.ExecuteUpdate(clazz, type);
			}
		}
		
		#endregion
		
		#region Insert Methods
		
		public static void ExecuteAutonomousInsert(Session session, StoredData clazz, Type type)
		{
			using (Persistence persistence = session.GetPersistence(true))
			{
				persistence.ExecuteInsert(clazz, type);
			}
		}

		public static void ExecuteInsert(Session session, StoredData clazz, Type type)
		{
			using (Persistence persistence = session.GetPersistence())
			{
				persistence.ExecuteInsert(clazz, type);
			}
		}
		
		#endregion
		
		#region Delete Methods
		
		public static void ExecuteAutonomousDelete(Session session,  StoredData clazz, Type type)
		{
			using (Persistence persistence = session.GetPersistence(true))
			{
				persistence.ExecuteDelete(clazz, type);
			}
		}

		public static void ExecuteDelete(Session session, StoredData clazz, Type type)
		{
			using (Persistence persistence = session.GetPersistence())
			{
				persistence.ExecuteDelete(clazz, type);
			}
		}
		
		#endregion
		
		#region Function methods
		
		public void ExecuteRSFunction(String function)
		{
			ExecuteRSFunction(function, null);
		}
		
		public void ExecuteRSFunction(String function, params Object[] parameters)
		{
			mSql_ = function;
			if (parameters != null)
			{
				mParameters_.AddRange(parameters);
			}

			mCommand_.CommandText = mSql_;
			mCommand_.CommandType = CommandType.StoredProcedure;
			SetParameters();
			
			mCommand_.Prepare();
			
			Log.Debug("Command: " + mSql_);
			Log.Debug("Parameters: " + mParameterString_);
			
			Stopwatch timer = Stopwatch.StartNew();
			mRs_ = mCommand_.ExecuteReader();
			IsEof = !mRs_.Read();
			mRecordCount_ = 0;
			timer.Stop();
			mSession_.SqlExecutionTime = mSession_.SqlExecutionTime.Add(timer.Elapsed);
			
			Log.Debug("Execution Time: " + timer.Elapsed);
		}

		#endregion

		#region Private Methods

		private void ExecuteInsert(StoredData clazz, Type type)
		{
			PrimaryKey newId = new PrimaryKey(0);
			
			GenerateInsertStatement(clazz, type);
			
			mCommand_.Prepare();
			
			Log.Debug("Command: " + mSql_);
			Log.Debug("Parameters: " + mParameterString_);

			Stopwatch timer = Stopwatch.StartNew();
			if (mCommand_ is MySqlCommand || mCommand_ is SQLiteCommand)
			{
				newId = new PrimaryKey(mCommand_.ExecuteScalar().ToString());
			}
			else if (mCommand_ is OracleCommand)
			{
				OracleParameter lastId = new OracleParameter();
				lastId.ParameterName = ":LASTID";
				lastId.DbType = DbType.Decimal;
				lastId.Direction = ParameterDirection.Output;
				((OracleCommand)mCommand_).Parameters.Add(lastId);

				mCommand_.ExecuteNonQuery();

				newId = new PrimaryKey(Convert.ToInt64(lastId.Value));
			}
			timer.Stop();
			mSession_.SqlExecutionTime = mSession_.SqlExecutionTime.Add(timer.Elapsed);
			
			Log.Debug("Execution Time: " + timer.Elapsed);
			
			if (mSession_.MessageMode.Equals(MessageMode.Normal))
			{
				clazz.Id = newId;
			}
		}

		private void ExecuteUpdate(StoredData clazz, Type type)
		{
			if (type.BaseType.Equals(typeof(StoredData)))
			{
				clazz.RowVer++;
			}

			GenerateUpdateStatement(clazz, type);
			
			mCommand_.Prepare();
			
			Log.Debug("Command: " + mSql_);
			Log.Debug("Parameters: " + mParameterString_);

			Stopwatch timer = Stopwatch.StartNew();
			Int32 result = mCommand_.ExecuteNonQuery();
			timer.Stop();
			mSession_.SqlExecutionTime = mSession_.SqlExecutionTime.Add(timer.Elapsed);
			
			Log.Debug("Execution Time: " + timer.Elapsed);

			if (result == 0)
			{
				mSession_.Error(ErrorLevel.Error, ErrorStrings.errLockKeyFailed, clazz.GetType().Name);
			}
		}

		private void ExecuteDelete(StoredData clazz, Type type)
		{
			GenerateDeleteStatement(clazz, type);
			
			mCommand_.Prepare();

			Log.Debug("Command: " + mSql_);
			Log.Debug("Parameters: " + mParameterString_);
			
			Stopwatch timer = Stopwatch.StartNew();
			Int32 result = mCommand_.ExecuteNonQuery();
			timer.Stop();
			mSession_.SqlExecutionTime = mSession_.SqlExecutionTime.Add(timer.Elapsed);
			
			Log.Debug("Execution Time: " + timer.Elapsed);

			if (result == 0)
			{
				mSession_.Error(ErrorLevel.Error, ErrorStrings.errLockKeyFailed, GetType().Name);
			}
		}

		private Int32 ExecuteUpdate(String sql, params Object[] parameters)
		{
			mSql_ = sql;
			if (parameters != null)
			{
				mParameters_.AddRange(parameters);
			}

			mCommand_.CommandText = mSql_;
			SetParameters();
			
			Log.Debug("Command: " + mSql_);
			Log.Debug("Parameters: " + mParameterString_);
			
			mCommand_.Prepare();

			Stopwatch timer = Stopwatch.StartNew();
			Int32 result = mCommand_.ExecuteNonQuery();
			timer.Stop();
			mSession_.SqlExecutionTime = mSession_.SqlExecutionTime.Add(timer.Elapsed);
			
			Log.Debug("Execution Time: " + timer.Elapsed);
			
			return result;
		}

		#endregion

		#region Abstract Methods

		protected abstract void SetParameters();
		protected abstract void GenerateUpdateStatement(StoredData clazz, Type type);
		protected abstract void GenerateDeleteStatement(StoredData clazz, Type type);
		protected abstract void GenerateInsertStatement(StoredData clazz, Type type);

		#endregion
		
		#region IDisposable Implementation
		
		public void Dispose()
		{
			try
			{
				Exception e = null;
				try
				{
					if (mRs_ != null)
					{
						mRs_.Dispose();
						mRs_ = null;
					}
				}
				catch (Exception ex)
				{
					e = ex;
					mRs_ = null;
				}

				try
				{
					if (mAutonomous_)
					{
						mCommand_.Transaction.Commit();
						mCommand_.Transaction.Dispose();
						mCommand_.Connection.Close();
						mCommand_.Connection.Dispose();
					}

					if (mCommand_ != null)
					{
						mCommand_.Dispose();
						mCommand_ = null;
					}
				}
				catch (Exception ex)
				{
					if (e == null)
					{
						e = ex;
					}
					mCommand_ = null;
				}

				if (e != null)
				{
					throw e;
				}
			}
			catch (Exception e)
			{
				throw new MiskoException("Error closing persistence", e);
			}
			finally
			{
				mSession_.RemovePersistence(this);
			}
		}
		
		#endregion
		
		#region Public Helpers

		public PrimaryKey GetPrimaryKey(String key)
		{
			Int64? value = GetLong(key);
			return value.HasValue ? new PrimaryKey(value.Value) : new PrimaryKey(0);
		}

		public Money GetMoney(String key)
		{
			Decimal? value = GetDecimal(key);
			return value.HasValue ? new Money(value.Value) : Money.ZERO;
		}
		
		public String GetString(String key)
		{
			try
			{
				Int32 ordinal = mRs_.GetOrdinal(key);
				String value = !mRs_.IsDBNull(ordinal) ? mRs_.GetString(ordinal).Trim() : "";
				return Regex.Replace(value, @"\r\n|\n\r|\n|\r", Environment.NewLine);
			}
			catch (IndexOutOfRangeException)
			{
				return "";
			}
			catch (InvalidCastException)
			{
				Int32 ordinal = mRs_.GetOrdinal(key);
				try
				{
					return !mRs_.IsDBNull(ordinal) ? mRs_.GetValue(ordinal).ToString().Trim() : "";
				}
				catch
				{
					return null;
				}
			}
		}

		public Int32? GetInt(String key)
		{
			try
			{
				Int32 ordinal = mRs_.GetOrdinal(key);
				return !mRs_.IsDBNull(ordinal) ? (Int32?)mRs_.GetInt32(ordinal) : null;
			}
			catch (IndexOutOfRangeException)
			{
				return null;
			}
			catch (InvalidCastException)
			{
				Int32 ordinal = mRs_.GetOrdinal(key);
				try
				{
					return !mRs_.IsDBNull(ordinal) ? Convert.ToInt32(mRs_.GetValue(ordinal)) : (Int32?)null;
				}
				catch
				{
					return null;
				}
			}
		}

		public Int64? GetLong(String key)
		{
			try
			{
				Int32 ordinal = mRs_.GetOrdinal(key);
				return !mRs_.IsDBNull(ordinal) ? (Int64?)mRs_.GetInt64(ordinal) : null;
			}
			catch (IndexOutOfRangeException)
			{
				return null;
			}
			catch (InvalidCastException)
			{
				Int32 ordinal = mRs_.GetOrdinal(key);
				try
				{
					return !mRs_.IsDBNull(ordinal) ? Convert.ToInt64(mRs_.GetValue(ordinal)) : (Int64?)null;
				}
				catch
				{
					return null;
				}
			}
		}

		public Decimal? GetDecimal(String key)
		{
			try
			{
				Int32 ordinal = mRs_.GetOrdinal(key);
				return !mRs_.IsDBNull(ordinal) ? (Decimal?)mRs_.GetDecimal(ordinal) : null;
			}
			catch (IndexOutOfRangeException)
			{
				return null;
			}
			catch (InvalidCastException)
			{
				Int32 ordinal = mRs_.GetOrdinal(key);
				try
				{
					return !mRs_.IsDBNull(ordinal) ? Convert.ToDecimal(mRs_.GetValue(ordinal)) : (Decimal?)null;
				}
				catch
				{
					return null;
				}
			}
		}

		public Double? GetDouble(String key)
		{
			try
			{
				Int32 ordinal = mRs_.GetOrdinal(key);
				return !mRs_.IsDBNull(ordinal) ? (Double?)mRs_.GetDouble(ordinal) : null;
			}
			catch (IndexOutOfRangeException)
			{
				return null;
			}
			catch (InvalidCastException)
			{
				Int32 ordinal = mRs_.GetOrdinal(key);
				try
				{
					return !mRs_.IsDBNull(ordinal) ? Convert.ToDouble(mRs_.GetValue(ordinal)) : (Double?)null;
				}
				catch
				{
					return null;
				}
			}
		}

		public DateTime? GetDate(String key)
		{
			try
			{
				Int32 ordinal = mRs_.GetOrdinal(key);
				return !mRs_.IsDBNull(ordinal) ? (DateTime?)mRs_.GetDateTime(ordinal) : null;
			}
			catch (IndexOutOfRangeException)
			{
				return null;
			}
			catch (InvalidCastException)
			{
				Int32 ordinal = mRs_.GetOrdinal(key);
				try
				{
					return !mRs_.IsDBNull(ordinal) ? Convert.ToDateTime(mRs_.GetValue(ordinal)) : (DateTime?)null;
				}
				catch
				{
					return null;
				}
			}
		}

		public Boolean? GetBoolean(String key)
		{
			try
			{
				Int32 ordinal = mRs_.GetOrdinal(key);
				return !mRs_.IsDBNull(ordinal) ? (Boolean?)mRs_.GetBoolean(ordinal) : null;
			}
			catch (IndexOutOfRangeException)
			{
				return null;
			}
			catch (InvalidCastException)
			{
				Int32 ordinal = mRs_.GetOrdinal(key);
				try
				{
					return !mRs_.IsDBNull(ordinal) ? Convert.ToBoolean(mRs_.GetValue(ordinal)) : (Boolean?)null;
				}
				catch
				{
					return null;
				}
			}
		}

		#endregion
	}
}
