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
        protected DataTable mRs_ = new DataTable("Unknown");
        protected DbCommand mCommand_ = null;
        protected String mSql_ = "";
        protected List<Object> mParameters_ = new List<Object>();
        protected DataRow mResult_;
        protected Int32 mCurrentResult_ = 0;

        #endregion

        #region Properties

        public Boolean HasNext
        {
        	get
        	{
        		return mRs_ != null && mCurrentResult_ < mRs_.Rows.Count;
        	}
        }
        
        public Int32 RecordCount
        {
        	get
        	{
        		return mRs_ != null ? mRs_.Rows.Count : 0;
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
            else if (session.Connection is MySqlConnection)
            {
                return new MySqlPersistence(session);
            }
            else if (session.Connection is SQLiteConnection)
            {
                return new SqlitePersistence(session);
            }
            else if (session.Connection is OleDbConnection)
            {
                return new FoxProPersistence(session);
            }
            else
            {
                session.Error(ErrorLevel.Error, ErrorStrings.errInvalicConnectionType);
                return null;
            }
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

        public Boolean Next()
        {
            if (HasNext)
            {
                mResult_ = mRs_.Rows[mCurrentResult_];
                mCurrentResult_++;
                return true;
            }

            return false;
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

        public void SqlWhere(bool condition, String expression, Object[] value)
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

        public Boolean ExecuteQuery()
        {
            if (String.IsNullOrEmpty(mSql_))
            {
                mSession_.Error(ErrorLevel.Error, ErrorStrings.errSqlNotSet);
            }

            return ExecuteQuery(mSql_, mParameters_.ToArray());
        }

        public Boolean ExecuteQuery(String sql)
        {
            return ExecuteQuery(sql, new Object[] { });
        }

        public Boolean ExecuteQuery(String sql, Object[] parameters)
        {
            mSession_.PersistencePool.Add(this);

            mSql_ = sql;
            mParameters_ = new List<object>(parameters);

            mCommand_.CommandText = mSql_;
            SetParameters();
            
            mCommand_.Prepare();

            Stopwatch timer = Stopwatch.StartNew();
            DataAdapter.Fill(mRs_);
            timer.Stop();
            mSession_.SqlExecutionTime += timer.ElapsedMilliseconds;

            return HasNext;
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
        
        public Boolean ExecuteRSFunction(String function)
        {
        	return ExecuteRSFunction(function, new Object[] { });
        }
        
        public Boolean ExecuteRSFunction(String function, Object[] parameters)
        {
    	 	mSession_.PersistencePool.Add(this);

            mSql_ = function;
            mParameters_ = new List<object>(parameters);

            mCommand_.CommandText = mSql_;
            mCommand_.CommandType = CommandType.StoredProcedure;
            SetParameters();
            
            mCommand_.Prepare();
            
            Stopwatch timer = Stopwatch.StartNew();            
            DataAdapter.Fill(mRs_);
            timer.Stop();
        	mSession_.SqlExecutionTime += timer.ElapsedMilliseconds;  

			return HasNext;        	
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
            mSession_.SqlExecutionTime += timer.ElapsedMilliseconds;
            
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
            mSession_.SqlExecutionTime += timer.ElapsedMilliseconds;

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
            int result = mCommand_.ExecuteNonQuery();
            timer.Stop();
            mSession_.SqlExecutionTime += timer.ElapsedMilliseconds;

            if (result == 0)
            {
                mSession_.Error(ErrorLevel.Error, ErrorStrings.errLockKeyFailed, new Object[] { GetType().Name });
            }
        }

        private Int32 ExecuteUpdate(String sql, Object[] parameters)
        {
            mSession_.PersistencePool.Add(this);

            mSql_ = sql;
            mParameters_ = new List<object>(parameters);

            mCommand_.CommandText = mSql_;
            SetParameters();
            
            mCommand_.Prepare();

            Stopwatch timer = Stopwatch.StartNew();
            int result = mCommand_.ExecuteNonQuery();
            timer.Stop();
            mSession_.SqlExecutionTime += timer.ElapsedMilliseconds;

            return result;
        }

        #endregion

        #region Abstract Methods

        protected abstract DbDataAdapter DataAdapter { get; }
        protected abstract void SetParameters();
        protected abstract void GenerateUpdateStatement(AbstractStoredData clazz, Type type);
        protected abstract void GenerateDeleteStatement(AbstractStoredData clazz, Type type);
        protected abstract void GenerateInsertStatement(AbstractStoredData clazz, Type type);

        #endregion

        #region Private Helpers

        private Object GetObject(String key)
        {
            Int32 ordinal = mRs_.Columns.IndexOf(key);
			return ordinal >= 0 ? (Object)mResult_.ItemArray[ordinal] : null;
        }

        #endregion

        #region Public Helpers

        public PrimaryKey GetPrimaryKey(String key)
        {
            Int64? value = GetLong(key);
            if (value != null && value.HasValue)
            {
                return new PrimaryKey(value.Value);
            }
            else
            {
                return null;
            }
        }

        public Money GetMoney(String key)
        {
            Decimal? value = GetDecimal(key);

			return value.HasValue ? new Money(value.Value) : null;
        }

        public String GetString(String key)
        {
            Object o = GetObject(key);

            if (o is String)
            {
                return ((String)o).Trim().Replace("\r\n", "\n").Replace("\r", "\n").Replace("\n", "\r\n");
            }
            else if (o is Byte[])
            {
                Byte[] asBytes = (Byte[])o;
                String asValue = "";

                for (int i = 0; i < asBytes.Length; i++)
                {
                    asValue += (Char)asBytes[i];
                }

                return asValue.Replace("\r\n", "\n").Replace("\r", "\n").Replace("\n", "\r\n");
            }
            else if (o != null)
            {
                return o.ToString().Trim().Length != 0 ? o.ToString().Trim().Replace("\r\n", "\n").Replace("\r", "\n").Replace("\n", "\r\n") : null;
            }

            return null;
        }

        public Int32? GetInt(String key)
        {
            Object o = GetObject(key);

            if (o == null)
            {
                return null;
            }
            else if (o is Int32)
            {
                return (Int32)o;
            }
            else if (o is sbyte || o is Byte || o is Int64 || o is Decimal)
            {
                return Convert.ToInt32(o);
            }
            else if (o is String)
            {
                try
                {
                    return Int32.Parse((String)o);
                }
                catch
                {
                }
            }

            return null;
        }

        public Int64? GetLong(String key)
        {
            Object o = GetObject(key);

            if (o == null)
            {
                return null;
            }
            else if (o is Int16)
            {
                return Convert.ToInt64((Int16)o);
            }
            else if (o is Int64)
            {
                return (Int64)o;
            }
            else if (o is UInt64)
            {
                return Convert.ToInt64((UInt64)o);
            }
            else if (o is Int32)
            {
                return Convert.ToInt64((Int32)o);
            }
            else if (o is uint)
            {
                return Convert.ToInt64((uint)o);
            }
            else if (o is Decimal)
            {
                return Convert.ToInt64((Decimal)o);
            }
            else if (o is String)
            {
				try 
				{
					return Int64.Parse((String)o);
				} 
				catch 
				{
				}
            }

            return null;
        }

        public Decimal? GetDecimal(String key)
        {
            Double? value = GetDouble(key);

            if (value.HasValue)
            {
                return Convert.ToDecimal(value);
            }

            return null;
        }

        public Double? GetDouble(String key)
        {
            Object o = GetObject(key);

            if (o == null)
            {
                return null;
            }
            else if (o is Double)
            {
                return (Double)o;
            }
            else if (o is Decimal)
            {
                return Decimal.ToDouble((Decimal)o);
            }
            else if (o is String)
            {
                try
                {
                    return Double.Parse((String)o);
                }
                catch
                {
                }
            }

            return null;
        }

        public DateTime? GetDate(String key)
        {
            Object o = GetObject(key);

            if (o == null)
            {
                return null;
            }
            else if (o is DateTime)
            {
                return (DateTime)o;
            }

            return null;
        }

        public Boolean? GetBoolean(String key)
        {
            Object o = GetObject(key);

            if (o == null)
            {
                return null;
            }
            else if (o is Boolean)
            {
                return (Boolean)o;
            }
            else if (o is Decimal || o is Int32 || o is Int64 || o is Byte || o is uint)
            {
                return Convert.ToInt16(o) == 1;
            }

            return null;
        }

        #endregion
    }
}
