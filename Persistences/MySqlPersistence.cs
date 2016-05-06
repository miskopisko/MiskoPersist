using System;
using System.Reflection;
using log4net;
using MiskoPersist.Core;
using MiskoPersist.Data;
using MiskoPersist.Enums;
using MiskoPersist.MoneyType;
using MiskoPersist.Serialization;
using MySql.Data.MySqlClient;

namespace MiskoPersist.Persistences
{
	internal class MySqlPersistence : Persistence
	{
		private static ILog Log = LogManager.GetLogger(typeof(MySqlPersistence));

		#region Fields



		#endregion

		#region Properties

		

		#endregion

		#region Constructors

		public MySqlPersistence(Session session)
			: base(session)
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

			position = 0;
			String paramString = "[";
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
					paramString += "NULL, ";
				}
				else if (parameter is StoredData)
				{
					param.IsNullable = false;
					param.MySqlDbType = MySqlDbType.Int64;
					param.Value = parameter != null ? (Object)((StoredData)parameter).Id.Value : DBNull.Value;
					mCommand_.Parameters.Add(param);
					paramString += param.Value + ", ";
				}
				else if (parameter is MiskoEnum)
				{
					param.IsNullable = true;
					param.MySqlDbType = MySqlDbType.Int64;
					param.Value = ((MiskoEnum)parameter).IsSet ? (Object)((MiskoEnum)parameter).Value : DBNull.Value;
					mCommand_.Parameters.Add(param);
					paramString += param.Value + ", ";
				}
				else if (parameter is Array)
				{
					String firstHalf = mCommand_.CommandText.Substring(0, mCommand_.CommandText.IndexOf(param.ParameterName));
					String secondHalf = mCommand_.CommandText.Substring(mCommand_.CommandText.IndexOf(param.ParameterName) + 7);
					String middle = "";

					foreach (Object o in ((Array)parameter))
					{
						MySqlParameter innerParam = new MySqlParameter();
						innerParam.ParameterName = "@param" + position;
						innerParam.Value = o;
						middle += innerParam.ParameterName + ", ";
						mCommand_.Parameters.Add(innerParam);
						paramString += innerParam.Value + ", ";
						position++;
					}

					mCommand_.CommandText = firstHalf + middle.Substring(0, middle.Length - 2) + secondHalf;
				}
				else if (parameter is Money)
				{
					param.MySqlDbType = MySqlDbType.Decimal;
					param.Value = ((Money)parameter).ToDecimal(null);
					mCommand_.Parameters.Add(param);
					paramString += param.Value + ", ";
				}
				else if (parameter is PrimaryKey)
				{
					param.MySqlDbType = MySqlDbType.Int64;
					param.Value = ((PrimaryKey)parameter).Value;
					mCommand_.Parameters.Add(param);
					paramString += param.Value + ", ";
				}
				else if (parameter is Int16)
				{
					param.MySqlDbType = MySqlDbType.Int16;
					param.Value = parameter;
					mCommand_.Parameters.Add(param);
					paramString += param.Value + ", ";
				}
				else if (parameter is Int32)
				{
					param.MySqlDbType = MySqlDbType.Int32;
					param.Value = parameter;
					mCommand_.Parameters.Add(param);
					paramString += param.Value + ", ";
				}
				else if (parameter is Int64)
				{
					param.MySqlDbType = MySqlDbType.Int64;
					param.Value = parameter;
					mCommand_.Parameters.Add(param);
					paramString += param.Value + ", ";
				}
				else if (parameter is Guid)
				{
					param.MySqlDbType = MySqlDbType.String;
					param.Value = ((Guid)parameter).ToString();
					mCommand_.Parameters.Add(param);
					paramString += param.Value + ", ";
				}
				else if (parameter is String)
				{
					param.Value = parameter;
					param.MySqlDbType = MySqlDbType.String;
					param.Size = ((String)parameter).Length;
					mCommand_.Parameters.Add(param);
					paramString += param.Value + ", ";
				}
				else if (parameter is DateTime)
				{
					param.Value = parameter;
					param.MySqlDbType = MySqlDbType.DateTime;
					mCommand_.Parameters.Add(param);
					paramString += ((DateTime)param.Value).ToString(Serializer.DATEFORMAT) + ", ";
				}
				else if (parameter is Boolean)
				{
					param.Value = parameter;
					param.MySqlDbType = MySqlDbType.Int16;
					mCommand_.Parameters.Add(param);
					paramString += param.Value + ", ";
				}
				else
				{
					param.Value = parameter;
					param.MySqlDbType = MySqlDbType.Blob;
					mCommand_.Parameters.Add(param);
					paramString += param.Value + ", ";
				}
				
				position++;
			}
			paramString = paramString.Substring(0, paramString.Length - 2) + "]";
			Log.Debug("Command: " + mCommand_.CommandText.Replace(Environment.NewLine, " "));
			Log.Debug("Parameters: " + paramString);
		}

		protected override void GenerateUpdateStatement(StoredData clazz, Type type)
		{
			if (clazz != null)
			{
				mSql_ += "UPDATE " + type.Name + Environment.NewLine + "SET    ";

				foreach (PropertyInfo property in StoredData.GetProperties(type))
				{
					mSql_ += StoredData.GetColumnName(property) + " = ?, " + Environment.NewLine + "       ";
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

		protected override void GenerateDeleteStatement(StoredData clazz, Type type)
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

		protected override void GenerateInsertStatement(StoredData clazz, Type type)
		{
			if (clazz != null)
			{
				mSql_ += "INSERT INTO " + type.Name + " (ID";

				foreach (PropertyInfo property in StoredData.GetProperties(type))
				{
					mSql_ += ", " + StoredData.GetColumnName(property);
				}

				mSql_ += ", DTCREATED, DTMODIFIED, ROWVER)" + Environment.NewLine + "VALUES (?, ";

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
					mSql_ += "?, ";
					mParameters_.Add(property.GetValue(clazz, null));
				}

				mSql_ += "NOW(), NOW(), 0);";

				if (type.BaseType.Equals(typeof(StoredData)))
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
