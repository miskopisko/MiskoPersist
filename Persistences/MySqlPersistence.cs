using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Reflection;
using MiskoPersist.Core;
using MiskoPersist.Data;
using MiskoPersist.Enums;
using MiskoPersist.MoneyType;
using MySql.Data.MySqlClient;

namespace MiskoPersist.Persistences
{
    class MySqlPersistence : Persistence
    {
        private static Logger Log = Logger.GetInstance(typeof(MySqlPersistence));

        #region Fields



        #endregion

        #region Properties

		protected override DbDataAdapter DataAdapter
		{
			get
			{
				return new MySqlDataAdapter((MySqlCommand)mCommand_);
			}
		}

        #endregion

        #region Constructors

        public MySqlPersistence(Session session) : base(session)
        {
        }

        #endregion

        #region Private Methods



        #endregion

        #region Public Methods

        protected override void SetParameters()
        {
            // Normalize the parameters by replacing typical ? marks with @param values
            Int32 end = 0;
            Int32 position = 0;
            while ((end = mCommand_.CommandText.IndexOf("?")) > 0)
            {
                mCommand_.CommandText = mCommand_.CommandText.Substring(0, end) + ("@param" + position++) + mCommand_.CommandText.Substring(end + 1);
                end = -1;
            }

            position = 0;
            foreach (Object parameter in mParameters_)
            {
                MySqlParameter param = new MySqlParameter();
                param.ParameterName = "@param" + position;

                if (parameter == null || (parameter is String && String.IsNullOrEmpty((String)parameter)))
                {
                    param.IsNullable = true;
                    param.Value = DBNull.Value;
                    param.MySqlDbType = MySqlDbType.Binary;
                    mCommand_.Parameters.Add(param);
                }
                else if (parameter is AbstractStoredData)
                {
                    param.IsNullable = false;
                    param.MySqlDbType = MySqlDbType.Int64;
                    param.Value = parameter != null ? (Object)((AbstractStoredData)parameter).Id.Value : DBNull.Value;
                    mCommand_.Parameters.Add(param);
                }
                else if (parameter is AbstractEnum)
                {
                    param.IsNullable = true;
                    param.MySqlDbType = MySqlDbType.Int64;
                    param.Value = ((AbstractEnum)parameter).IsSet ? (Object)((AbstractEnum)parameter).Value : DBNull.Value;
                    mCommand_.Parameters.Add(param);
                }
                else if (parameter is Array)
                {
                    String firstHalf = mCommand_.CommandText.Substring(0, mCommand_.CommandText.IndexOf(param.ParameterName));
                    String secondHalf = mCommand_.CommandText.Substring(mCommand_.CommandText.IndexOf(param.ParameterName)+7);
                    String middle = "";

                    foreach (Object o in ((Array)parameter))
                    {                        
                        MySqlParameter innerParam = new MySqlParameter();
                        innerParam.ParameterName = "@param" + position;
                        innerParam.Value = o;
                        middle += innerParam.ParameterName + ", ";
                        mCommand_.Parameters.Add(innerParam);
                        position++;
                    }

                    mCommand_.CommandText = firstHalf + middle.Substring(0, middle.Length-2) + secondHalf;
                }
                else if (parameter is Money)
                {
                    param.MySqlDbType = MySqlDbType.Decimal;
                    param.Value = ((Money)parameter).ToDecimal(null);
                    mCommand_.Parameters.Add(param);
                }
                else if (parameter is PrimaryKey)
                {
                    param.MySqlDbType = MySqlDbType.Int64;
                    param.Value = ((PrimaryKey)parameter).Value;
                    mCommand_.Parameters.Add(param);
                }                
                else if (parameter is Int16)
                {
                    param.MySqlDbType = MySqlDbType.Int16;
                    param.Value = parameter;
                    mCommand_.Parameters.Add(param);
                }
                else if (parameter is Int32)
                {
                    param.MySqlDbType = MySqlDbType.Int32;
                    param.Value = parameter;
                    mCommand_.Parameters.Add(param);
                }
                else if (parameter is Int64)
                {
                    param.MySqlDbType = MySqlDbType.Int64;
                    param.Value = parameter;
                    mCommand_.Parameters.Add(param);
                }                
                else if (parameter is Guid)
                {
                    param.MySqlDbType = MySqlDbType.String;
                    param.Value = ((Guid)parameter).ToString();
                    mCommand_.Parameters.Add(param);
                }
                else if(parameter is String)
                {
                    param.Value = parameter;
                    param.MySqlDbType = MySqlDbType.String;
                    param.Size = ((String)parameter).Length;
                    mCommand_.Parameters.Add(param);
                }
                else if(parameter is DateTime)
                {
                    param.Value = parameter;
                    param.MySqlDbType = MySqlDbType.DateTime;
                    mCommand_.Parameters.Add(param);
                }
                else if(parameter is Boolean || parameter is Boolean)
                {
                	param.Value = parameter;
                    param.MySqlDbType = MySqlDbType.Int16;
                    mCommand_.Parameters.Add(param);
                }
                else
                {
                    param.Value = parameter;
                    param.MySqlDbType = MySqlDbType.Blob;
                    mCommand_.Parameters.Add(param);
                }
                
                position++;
            }
        }

        protected override void GenerateUpdateStatement(AbstractStoredData clazz, Type type)
        {
        	if (clazz != null)
            {
                mSql_ += "UPDATE " + type.Name + Environment.NewLine + "SET    ";

                foreach (PropertyInfo property in AbstractData.GetProperties(type))
                {
                    mSql_ += AbstractData.GetColumnName(property) + " = ?, " + Environment.NewLine + "       ";
                    mParameters_.Add(property.GetValue(clazz, null));
                }

                mSql_ += "DTMODIFIED = NOW()," + Environment.NewLine;
                mSql_ += "       ROWVER = ?" + Environment.NewLine;
                mSql_ += "WHERE  ID = ?" + Environment.NewLine;
                mSql_ += "AND    ROWVER = ?;";

                mParameters_.Add(clazz.RowVer);
                mParameters_.Add(clazz.Id);
                mParameters_.Add(clazz.RowVer - 1);

                mCommand_.CommandText = mSql_;
                SetParameters();
            }
        }

        protected override void GenerateDeleteStatement(AbstractStoredData clazz, Type type)
        {
            if (clazz != null)
            {
                mSql_ += "DELETE" + Environment.NewLine;
                mSql_ += "FROM   " + type.Name + Environment.NewLine;
                mSql_ += "WHERE  ID = ?" + Environment.NewLine;
                mSql_ += "AND    ROWVER = ?;";

                mParameters_.Add(-clazz.Id);
                mParameters_.Add(clazz.RowVer);

                mCommand_.CommandText = mSql_;
                SetParameters();
            }
        }

        protected override void GenerateInsertStatement(AbstractStoredData clazz, Type type)
        {
            if (clazz != null)
            {
                List<PropertyInfo> properties = AbstractData.GetProperties(type);

                mSql_ += "INSERT INTO " + type.Name + " (ID";

                foreach (PropertyInfo property in properties)
                {
                    mSql_ += ", " + AbstractData.GetColumnName(property);
                }

                mSql_ += ", DTCREATED, DTMODIFIED, ROWVER)" + Environment.NewLine + "VALUES (?, ";

                if (clazz.Id > 0)
                {
                    mParameters_.Add(clazz.Id);
                }
                else
                {
                    mParameters_.Add(DBNull.Value);
                }

                foreach (PropertyInfo property in properties)
                {
                    mSql_ += "?, ";
                    mParameters_.Add(property.GetValue(clazz, null));
                }

                mSql_ += "NOW(), NOW(), 0);";

                if (type.BaseType.Equals(typeof(AbstractStoredData)))
                {
                    mSql_ += Environment.NewLine + "SELECT LAST_INSERT_ID() AS ID;";
                }
                else
                {
                    mSql_ += Environment.NewLine + "SELECT ? AS ID;";
                    mParameters_.Add(clazz.Id);
                }

                mCommand_.CommandText = mSql_;
                SetParameters();
            }
        }

        #endregion
    }
}
