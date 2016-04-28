using System;
using System.Data;
using System.Data.SQLite;
using System.Reflection;
using log4net;
using MiskoPersist.Core;
using MiskoPersist.Data;
using MiskoPersist.Enums;
using MiskoPersist.MoneyType;
using MiskoPersist.Serialization;

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

		public SqlitePersistence(Session session) : base(session)
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
			
			String paramString = "[";
			position = 0;
			foreach (Object parameter in mParameters_)
			{
				SQLiteParameter param = (SQLiteParameter)mCommand_.CreateParameter();
				param.ParameterName = "@param" + position;

				if(parameter == null || (parameter is String && String.IsNullOrEmpty((String)parameter)))
				{
					param.IsNullable = true;
					param.Value = DBNull.Value;
					param.DbType = DbType.Object;
					mCommand_.Parameters.Add(param);
					paramString += "NULL, ";
				}
				else if(parameter is StoredData)
				{
					param.IsNullable = false;
					param.DbType = DbType.Int64;
					param.Value = parameter != null ? (Object)((StoredData)parameter).Id.Value : DBNull.Value;
					mCommand_.Parameters.Add(param);
					paramString += param.Value + ", ";
				}
				else if(parameter is MiskoEnum)
				{
					param.IsNullable = true;
					param.DbType = DbType.Int32;
					param.Value = ((MiskoEnum)parameter).IsSet ? (Object)((MiskoEnum)parameter).Value : DBNull.Value;
					mCommand_.Parameters.Add(param);
					paramString += param.Value + ", ";
				}
				else if(parameter is Array)
				{
					String firstHalf = mCommand_.CommandText.Substring(0, mCommand_.CommandText.IndexOf(param.ParameterName));
					String secondHalf = mCommand_.CommandText.Substring(mCommand_.CommandText.IndexOf(param.ParameterName) + 7);
					String middle = "";

					foreach (Object o in ((Array)parameter))
					{
						SQLiteParameter innerParam = new SQLiteParameter();
						innerParam.ParameterName = "@param" + position;
						innerParam.Value = o;
						middle += innerParam.ParameterName + ", ";
						mCommand_.Parameters.Add(innerParam);
						paramString += innerParam.Value + ", ";
						position++;
					}

					mCommand_.CommandText = firstHalf + middle.Substring(0, middle.Length - 2) + secondHalf;
				}
				else if(parameter is Money)
				{
					param.DbType = DbType.Currency;
					param.Value = ((Money)parameter).ToDecimal(null);
					mCommand_.Parameters.Add(param);
					paramString += param.Value + ", ";
				}
				else if(parameter is PrimaryKey)
				{
					param.DbType = DbType.Int64;
					param.Value = ((PrimaryKey)parameter).Value;
					mCommand_.Parameters.Add(param);
					paramString += param.Value + ", ";
				}
				else if(parameter is Int16)
				{
					param.DbType = DbType.Int16;
					param.Value = parameter;
					mCommand_.Parameters.Add(param);
					paramString += param.Value + ", ";
				}
				else if(parameter is Int32)
				{
					param.DbType = DbType.Int32;
					param.Value = parameter;
					mCommand_.Parameters.Add(param);
					paramString += param.Value + ", ";
				}
				else if(parameter is Int64)
				{
					param.DbType = DbType.Int64;
					param.Value = parameter;
					mCommand_.Parameters.Add(param);
					paramString += param.Value + ", ";
				}
				else if(parameter is Guid)
				{
					param.DbType = DbType.Guid;
					param.Value = ((Guid)parameter).ToString();
					mCommand_.Parameters.Add(param);
					paramString += param.Value + ", ";
				}
				else if(parameter is String)
				{
					param.Value = parameter;
					param.DbType = DbType.String;
					param.Size = ((String)parameter).Length;
					mCommand_.Parameters.Add(param);
					paramString += param.Value + ", ";
				}
				else if(parameter is DateTime)
				{
					param.Value = parameter;
					param.DbType = DbType.DateTime;
					mCommand_.Parameters.Add(param);
					paramString += ((DateTime)param.Value).ToString(Serializer.DATEFORMAT) + ", ";
				}
				else if(parameter is Boolean)
				{
					param.Value = parameter;
					param.DbType = DbType.Int16;
					mCommand_.Parameters.Add(param);
					paramString += param.Value + ", ";
				}
				else
				{
					param.Value = parameter;
					param.DbType = DbType.Object;
					mCommand_.Parameters.Add(param);
					paramString += param.Value + ", ";
				}

				position++;
			}
			paramString = paramString.Substring(0, paramString.Length-2) + "]";
			Log.Debug("Command: " + mCommand_.CommandText.Replace(Environment.NewLine, " "));
			Log.Debug("Parameters: " + paramString);
		}

		protected override void GenerateUpdateStatement(StoredData clazz, Type type)
		{
			if(clazz != null)
			{
				mSql_ += "UPDATE " + type.Name.ToUpper() + Environment.NewLine + "SET    ";

				foreach (PropertyInfo property in StoredData.GetProperties(type))
				{
					mSql_ += StoredData.GetColumnName(property) + " = ?, " + Environment.NewLine + "         ";
					mParameters_.Add(property.GetValue(clazz, null));
				}

				mSql_ += "DTMODIFIED = DATETIME('NOW')," + Environment.NewLine;
				mSql_ += "       ROWVER = ?" + Environment.NewLine;
				mSql_ += "WHERE  ID = ?" + Environment.NewLine;
				mSql_ += "AND    ROWVER = ?;";

				mParameters_.Add(clazz.RowVer);
				mParameters_.Add(clazz.Id);
				mParameters_.Add(clazz.RowVer-1);

				mCommand_.CommandText = mSql_;
				SetParameters();
			}
		}

		protected override void GenerateDeleteStatement(StoredData clazz, Type type)
		{
			if(clazz != null)
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

		protected override void GenerateInsertStatement(StoredData clazz, Type type)
		{
			if(clazz != null)
			{
				mSql_ += "INSERT INTO " + type.Name.ToUpper() + " (ID";

				foreach (PropertyInfo property in StoredData.GetProperties(type))
				{
					mSql_ += ", " + StoredData.GetColumnName(property);
				}

				mSql_ += ", DTCREATED, DTMODIFIED, ROWVER)" + Environment.NewLine + "VALUES (?, ";

				if(clazz.Id > 0)
				{
					mParameters_.Add(clazz.Id);
				}
				else
				{
					mParameters_.Add(DBNull.Value);
				}

				foreach (PropertyInfo property in StoredData.GetProperties(type))
				{
					mSql_ += "?, ";
					mParameters_.Add(property.GetValue(clazz, null));
				}

				mSql_ += "DATETIME('NOW'), DATETIME('NOW'), 0);";

				if(type.BaseType.Equals(typeof(StoredData)))
				{
					mSql_ += Environment.NewLine + "SELECT LAST_INSERT_ROWID() AS ID;";
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
