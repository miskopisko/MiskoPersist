using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.OleDb;
using System.Data.SQLite;
using System.Diagnostics;
using MiskoPersist.Data;
using MiskoPersist.Enums;
using MiskoPersist.MoneyType;
using MiskoPersist.Persistences;
using MiskoPersist.Resources;
using MySql.Data.MySqlClient;
using Oracle.DataAccess.Client;

namespace MiskoPersist.Core
{
	public abstract class Persistence
	{
		private static Logger Log = Logger.GetInstance(typeof(Persistence));

		#region Fields

		protected Session mSession_ = null;
		protected DbDataReader mRs_ = null;
		protected DbCommand mCommand_ = null;
		protected String mSql_ = "";
		protected List<Object> mParameters_ = new List<Object>();
		protected Boolean mEof_ = true;
		protected Int32 mRecordCount_ = 0;

		#endregion

		#region Properties
		
		public Boolean IsEof
		{
			get
			{
				return mEof_;
			}
		}
		
		public Int32 RecordCount
		{
			get
			{
				while(!IsEof)
				{
					Next();
				}
				return mRecordCount_;
			}
		}

		#endregion

		#region Constructors

		protected Persistence(Session session)
		{
			mSession_ = session;
			mCommand_ = mSession_.Connection.CreateCommand();
			mCommand_.Transaction = session.Transaction;
		}

		#endregion

		#region Public Methods

		public static Persistence GetInstance(Session session)
		{
			if (session.Connection is OracleConnection)
			{
				return new OraclePersistence(session);
			}
			if (session.Connection is MySqlConnection)
			{
				return new MySqlPersistence(session);
			}
			if (session.Connection is SQLiteConnection)
			{
				return new SqlitePersistence(session);
			}
			if (session.Connection is OleDbConnection)
			{
				return new FoxProPersistence(session);
			}
			
			session.Error(ErrorLevel.Error, ErrorStrings.errInvalicConnectionType);
			return null;
		}

		public void Close()
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
				mSession_.PersistencePool.Remove(this);
			}
		}

		public void Next()
		{
			mEof_ = !mRs_.Read();
			mRecordCount_ ++;
		}

		public void SetSql(String sql)
		{
			mSql_ = sql;
		}

		public void SetSql(String sql, Object[] values)
		{
			mSql_ = sql;
			mParameters_.AddRange(values);
		}

		public void SqlWhere(Boolean condition, String expression, Object[] value)
		{
			if (condition)
			{
				mSql_ = mSql_ + Environment.NewLine + (!mSql_.Contains("WHERE") ? "WHERE " : "AND ") + expression;
				foreach (Object item in value)
				{
					mParameters_.Add(item);
				}
			}
		}

		public void SqlOrderBy(String columnName)
		{
			SqlOrderBy(columnName, SortDirection.Ascending);
		}

		public void SqlOrderBy(String columnName, SortDirection sortDirection)
		{
			mSql_ = mSql_ + (!mSql_.Contains("ORDER BY") ? Environment.NewLine + "ORDER BY " : ", ") + columnName + sortDirection.Code;
		}

		#endregion

		#region Execute Methods

		public void ExecuteQuery()
		{
			if (String.IsNullOrEmpty(mSql_))
			{
				mSession_.Error(ErrorLevel.Error, ErrorStrings.errSqlNotSet);
			}

			ExecuteQuery(mSql_, mParameters_.ToArray());
		}

		public void ExecuteQuery(String sql)
		{
			ExecuteQuery(sql, new Object[] { });
		}

		public void ExecuteQuery(String sql, Object[] parameters)
		{
			mSession_.PersistencePool.Add(this);

			mSql_ = sql;
			mParameters_ = new List<Object>(parameters);

			mCommand_.CommandText = mSql_;
			SetParameters();
			
			mCommand_.Prepare();
			
			Stopwatch timer = Stopwatch.StartNew();
			mRs_ = mCommand_.ExecuteReader();
			mEof_ = !mRs_.Read();
			mRecordCount_ = 0;
			timer.Stop();
			mSession_.SqlExecutionTime = mSession_.SqlExecutionTime.Add(timer.Elapsed);
		}

		public static Int32 ExecuteUpdate(Session session, String sql)
		{
			return ExecuteUpdate(session, sql, null);
		}

		public static Int32 ExecuteUpdate(Session session, String sql, Object[] parameters)
		{
			Persistence persistence = Persistence.GetInstance(session);
			Int32 result = persistence.ExecuteUpdate(sql, parameters);
			persistence.Close();
			persistence = null;
			return result;
		}

		public static void ExecuteUpdate(Session session, AbstractStoredData clazz, Type type)
		{
			Persistence persistence = Persistence.GetInstance(session);
			persistence.ExecuteUpdate(clazz, type);
			persistence.Close();
			persistence = null;
		}

		public static void ExecuteInsert(Session session, AbstractStoredData clazz, Type type)
		{
			Persistence persistence = Persistence.GetInstance(session);
			persistence.ExecuteInsert(clazz, type);
			persistence.Close();
			persistence = null;
		}

		public static void ExecuteDelete(Session session, AbstractStoredData clazz, Type type)
		{
			Persistence persistence = Persistence.GetInstance(session);
			persistence.ExecuteDelete(clazz, type);
			persistence.Close();
			persistence = null;
		}
		
		public void ExecuteRSFunction(String function)
		{
			ExecuteRSFunction(function, new Object[] { });
		}
		
		public void ExecuteRSFunction(String function, Object[] parameters)
		{
			mSession_.PersistencePool.Add(this);

			mSql_ = function;
			mParameters_ = new List<Object>(parameters);

			mCommand_.CommandText = mSql_;
			mCommand_.CommandType = CommandType.StoredProcedure;
			SetParameters();
			
			mCommand_.Prepare();
			
			Stopwatch timer = Stopwatch.StartNew();            
			mRs_ = mCommand_.ExecuteReader();
			mEof_ = !mRs_.Read();
			mRecordCount_ = 0;
			timer.Stop();
			mSession_.SqlExecutionTime = mSession_.SqlExecutionTime.Add(timer.Elapsed);
		}

		#endregion

		#region Private Methods

		private void ExecuteInsert(AbstractStoredData clazz, Type type)
		{
			mSession_.PersistencePool.Add(this);

			PrimaryKey newId = new PrimaryKey();
			
			GenerateInsertStatement(clazz, type);
			
			mCommand_.Prepare();           

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
			else if (mCommand_ is OleDbCommand)
			{
				// FoxPro INSERT is done on a class by class basis by overriding the Create method
			}
			timer.Stop();            
			mSession_.SqlExecutionTime = mSession_.SqlExecutionTime.Add(timer.Elapsed);
			
			if (mSession_.MessageMode.Equals(MessageMode.Normal))
			{
				clazz.Id = newId;
			}
		}

		private void ExecuteUpdate(AbstractStoredData clazz, Type type)
		{
			mSession_.PersistencePool.Add(this);

			if (type.BaseType.Equals(typeof(AbstractStoredData)))
			{
				clazz.RowVer++;
			}

			GenerateUpdateStatement(clazz, type);
			
			mCommand_.Prepare();

			Stopwatch timer = Stopwatch.StartNew();
			Int32 result = mCommand_.ExecuteNonQuery();
			timer.Stop();
			mSession_.SqlExecutionTime = mSession_.SqlExecutionTime.Add(timer.Elapsed);

			if (result == 0)
			{
				mSession_.Error(ErrorLevel.Error, ErrorStrings.errLockKeyFailed, new Object[] { clazz.GetType().Name });
			}
		}

		private void ExecuteDelete(AbstractStoredData clazz, Type type)
		{
			mSession_.PersistencePool.Add(this);

			GenerateDeleteStatement(clazz, type);
			
			mCommand_.Prepare();

			Stopwatch timer = Stopwatch.StartNew();
			Int32 result = mCommand_.ExecuteNonQuery();
			timer.Stop();
			mSession_.SqlExecutionTime = mSession_.SqlExecutionTime.Add(timer.Elapsed);

			if (result == 0)
			{
				mSession_.Error(ErrorLevel.Error, ErrorStrings.errLockKeyFailed, new Object[] { GetType().Name });
			}
		}

		private Int32 ExecuteUpdate(String sql, Object[] parameters)
		{
			mSession_.PersistencePool.Add(this);

			mSql_ = sql;
			mParameters_ = new List<Object>(parameters);

			mCommand_.CommandText = mSql_;
			SetParameters();
			
			mCommand_.Prepare();

			Stopwatch timer = Stopwatch.StartNew();
			Int32 result = mCommand_.ExecuteNonQuery();
			timer.Stop();
			mSession_.SqlExecutionTime = mSession_.SqlExecutionTime.Add(timer.Elapsed);

			return result;
		}

		#endregion

		#region Abstract Methods

		protected abstract void SetParameters();
		protected abstract void GenerateUpdateStatement(AbstractStoredData clazz, Type type);
		protected abstract void GenerateDeleteStatement(AbstractStoredData clazz, Type type);
		protected abstract void GenerateInsertStatement(AbstractStoredData clazz, Type type);

		#endregion
		
		#region Public Helpers

		public PrimaryKey GetPrimaryKey(String key)
		{
			Int64? value = GetLong(key);
			return (value != null && value.HasValue && value.Value > 0) ? new PrimaryKey(value.Value) : new PrimaryKey(0);
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
				return !mRs_.IsDBNull(ordinal) ? mRs_.GetString(ordinal).Trim() : "";
			}
			catch(IndexOutOfRangeException)
			{
				return "";
			}
			catch(InvalidCastException)
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
			catch(IndexOutOfRangeException)
			{
				return null;
			}
			catch(InvalidCastException)
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
			catch(InvalidCastException)
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
			catch(InvalidCastException)
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
			catch(InvalidCastException)
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
			catch(InvalidCastException)
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
			catch(InvalidCastException)
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
