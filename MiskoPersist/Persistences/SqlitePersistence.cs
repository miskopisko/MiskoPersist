using System;
using System.Data;
using System.Data.Common;
using System.Data.SQLite;
using System.Reflection;
using log4net;
using MiskoPersist.Core;
using MiskoPersist.Data.Stored;
using MiskoPersist.Enums;
using MiskoPersist.MoneyType;

namespace MiskoPersist.Persistences
{
	internal class SqlitePersistence : Persistence
	{
		private static ILog Log = LogManager.GetLogger(typeof(SqlitePersistence));

		#region Fields



		#endregion

		#region Properties

		

		#endregion

		#region Constructors

		public SqlitePersistence(Session session, DbCommand command)
			: base(session, command)
		{
		}

		#endregion

		#region Private Methods



		#endregion

		#region Public Methods

		protected override void SetParameters()
		{
			if (mParameters_.Count == 0)
			{
				return;
			}
			
			// Normalize the parameters by replacing typical ? marks with @param values
			Int32 end = 0;
			Int32 position = 0;
			while ((end = mCommand_.CommandText.IndexOf("?")) > 0)
			{
				mCommand_.CommandText = mCommand_.CommandText.Substring(0, end) + ("@param" + position++) + mCommand_.CommandText.Substring(end + 1);
				end = -1;
			}
			
			mParameterString_ = "[";
			position = 0;
			foreach (Object parameter in mParameters_)
			{
				SQLiteParameter param = (SQLiteParameter)mCommand_.CreateParameter();
				param.ParameterName = "@param" + position;

				if (parameter == null || (parameter is String && String.IsNullOrEmpty((String)parameter)))
				{
					param.IsNullable = true;
					param.Value = DBNull.Value;
					param.DbType = DbType.Object;
					mCommand_.Parameters.Add(param);
					mParameterString_ += "NULL, ";
				}
				else if (parameter is StoredData)
				{
					param.IsNullable = false;
					param.DbType = DbType.Int64;
					param.Value = parameter != null ? (Object)((StoredData)parameter).Id.Value : DBNull.Value;
					mCommand_.Parameters.Add(param);
					mParameterString_ += param.Value + ", ";
				}
				else if (parameter is MiskoEnum)
				{
					param.IsNullable = true;
					param.DbType = DbType.Int32;
					param.Value = ((MiskoEnum)parameter).IsSet ? (Object)((MiskoEnum)parameter).Value : DBNull.Value;
					mCommand_.Parameters.Add(param);
					mParameterString_ += param.Value + ", ";
				}
				else if (parameter is Array)
				{
					String firstHalf = mCommand_.CommandText.Substring(0, mCommand_.CommandText.IndexOf(param.ParameterName));
					String secondHalf = mCommand_.CommandText.Substring(mCommand_.CommandText.IndexOf(param.ParameterName) + 7);
					String middle = "";

					foreach (Object o in (Array)parameter)
					{
						SQLiteParameter innerParam = new SQLiteParameter();
						innerParam.ParameterName = "@param" + position;
						innerParam.Value = o;
						middle += innerParam.ParameterName + ", ";
						mCommand_.Parameters.Add(innerParam);
						mParameterString_ += innerParam.Value + ", ";
						position++;
					}

					mCommand_.CommandText = firstHalf + middle.Substring(0, middle.Length - 2) + secondHalf;
				}
				else if (parameter is Money)
				{
					param.DbType = DbType.Currency;
					param.Value = ((Money)parameter).ToDecimal(null);
					mCommand_.Parameters.Add(param);
					mParameterString_ += param.Value + ", ";
				}
				else if (parameter is PrimaryKey)
				{
					param.DbType = DbType.Int64;
					param.Value = ((PrimaryKey)parameter).Value;
					mCommand_.Parameters.Add(param);
					mParameterString_ += param.Value + ", ";
				}
				else if (parameter is Int16)
				{
					param.DbType = DbType.Int16;
					param.Value = parameter;
					mCommand_.Parameters.Add(param);
					mParameterString_ += param.Value + ", ";
				}
				else if (parameter is Int32)
				{
					param.DbType = DbType.Int32;
					param.Value = parameter;
					mCommand_.Parameters.Add(param);
					mParameterString_ += param.Value + ", ";
				}
				else if (parameter is Int64)
				{
					param.DbType = DbType.Int64;
					param.Value = parameter;
					mCommand_.Parameters.Add(param);
					mParameterString_ += param.Value + ", ";
				}
				else if (parameter is Guid)
				{
					param.DbType = DbType.Guid;
					param.Value = ((Guid)parameter).ToString();
					mCommand_.Parameters.Add(param);
					mParameterString_ += param.Value + ", ";
				}
				else if (parameter is String)
				{
					param.Value = parameter;
					param.DbType = DbType.String;
					param.Size = ((String)parameter).Length;
					mCommand_.Parameters.Add(param);
					mParameterString_ += param.Value + ", ";
				}
				else if (parameter is DateTime)
				{
					param.Value = parameter;
					param.DbType = DbType.DateTime;
					mCommand_.Parameters.Add(param);
					mParameterString_ += ((DateTime)param.Value) + ", ";
				}
				else if (parameter is Boolean)
				{
					param.Value = parameter;
					param.DbType = DbType.Int16;
					mCommand_.Parameters.Add(param);
					mParameterString_ += param.Value + ", ";
				}
				else
				{
					param.Value = parameter;
					param.DbType = DbType.Object;
					mCommand_.Parameters.Add(param);
					mParameterString_ += param.Value + ", ";
				}

				position++;
			}
			mParameterString_ = mParameterString_.Substring(0, mParameterString_.Length - 2) + "]";
		}

		protected override void GenerateUpdateStatement(StoredData clazz, Type type)
		{
			if (clazz != null)
			{
				mSql_ += "UPDATE " + type.Name.ToUpper() + " SET ";

				foreach (PropertyInfo property in StoredData.GetProperties(type))
				{
					mSql_ += clazz.GetColumnName(property) + " = ?, ";
					mParameters_.Add(property.GetValue(clazz, null));
				}

				mSql_ += "DTMODIFIED = DATETIME('NOW'), ROWVER = ? WHERE ID = ? AND ROWVER = ?;";

				mParameters_.Add(clazz.RowVer);
				mParameters_.Add(clazz.Id);
				mParameters_.Add(clazz.RowVer - 1);

				mCommand_.CommandText = mSql_;
				SetParameters();
			}
		}

		protected override void GenerateDeleteStatement(StoredData clazz, Type type)
		{
			if (clazz != null)
			{
				mSql_ += "DELETE FROM " + type.Name + " WHERE ID = ? AND ROWVER = ?;";

				mParameters_.Add(-clazz.Id);
				mParameters_.Add(clazz.RowVer);

				mCommand_.CommandText = mSql_;
				SetParameters();
			}
		}

		protected override void GenerateInsertStatement(StoredData clazz, Type type)
		{
			if (clazz != null)
			{
				mSql_ += "INSERT INTO " + type.Name.ToUpper() + " (ID";

				foreach (PropertyInfo property in StoredData.GetProperties(type))
				{
					mSql_ += ", " + clazz.GetColumnName(property);
				}

				mSql_ += ") VALUES (?";

				if (clazz.Id > 0)
				{
					mParameters_.Add(clazz.Id);
				}
				else
				{
					mParameters_.Add(null);
				}

				foreach (PropertyInfo property in StoredData.GetProperties(type))
				{
					mSql_ += ", ?";
					mParameters_.Add(property.GetValue(clazz, null));
				}

				if (type.BaseType.Equals(typeof(StoredData)))
				{
					mSql_ += "); SELECT LAST_INSERT_ROWID() AS ID;";
				}
				else
				{
					mSql_ += "); SELECT ? AS ID;";
					mParameters_.Add(clazz.Id);
				}

				mCommand_.CommandText = mSql_;
				SetParameters();
			}
		}
		
		#endregion
	}
}
