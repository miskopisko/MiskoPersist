using System;
using System.Data.Common;
using System.Data.OleDb;
using log4net;
using MiskoPersist.Core;
using MiskoPersist.Data.Stored;
using MiskoPersist.Enums;
using MiskoPersist.MoneyType;

namespace MiskoPersist.Persistences
{
	internal class FoxProPersistence : Persistence
	{
		private static ILog Log = LogManager.GetLogger(typeof(FoxProPersistence));

		#region Variable Declarations



		#endregion

		#region Properties

		

		#endregion

		#region Constructors

		public FoxProPersistence(Session session, DbCommand command)
			: base(session, command)
		{
		}

		#endregion

		#region Private Methods



		#endregion

		#region Override Methods

		protected override void SetParameters()
		{
			if (mParameters_.Count == 0)
			{
				return;
			}
			
			Int32 position = 0;
			mParameterString_ = "[";
			foreach (Object parameter in mParameters_)
			{
				OleDbParameter param = new OleDbParameter();
				param.ParameterName = position.ToString();

				if (parameter == null || (parameter is String && String.IsNullOrEmpty((String)parameter)))
				{
					param.IsNullable = true;
					param.Value = DBNull.Value;
					param.OleDbType = OleDbType.IUnknown;
					mCommand_.Parameters.Add(param);
					mParameterString_ += "NULL, ";
				}
				else if (parameter is StoredData)
				{
					param.IsNullable = false;
					param.OleDbType = OleDbType.Integer;
					param.Value = parameter != null ? (Object)((StoredData)parameter).Id : DBNull.Value;
					mCommand_.Parameters.Add(param);
					mParameterString_ += param.Value + ", ";
				}
				else if (parameter is MiskoEnum)
				{
					param.IsNullable = true;
					param.OleDbType = OleDbType.Integer;
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
						OleDbParameter innerParam = new OleDbParameter();
						innerParam.ParameterName = position.ToString();
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
					param.OleDbType = OleDbType.Currency;
					param.Value = ((Money)parameter).ToDecimal(null);
					mCommand_.Parameters.Add(param);
					mParameterString_ += param.Value + ", ";
				}
				else if (parameter is PrimaryKey)
				{
					param.OleDbType = OleDbType.BigInt;
					param.Value = ((PrimaryKey)parameter).Value;
					mCommand_.Parameters.Add(param);
					mParameterString_ += param.Value + ", ";
				}
				else if (parameter is Int16)
				{
					param.OleDbType = OleDbType.SmallInt;
					param.Value = parameter;
					mCommand_.Parameters.Add(param);
					mParameterString_ += param.Value + ", ";
				}
				else if (parameter is Int32)
				{
					param.OleDbType = OleDbType.Integer;
					param.Value = parameter;
					mCommand_.Parameters.Add(param);
					mParameterString_ += param.Value + ", ";
				}
				else if (parameter is Int64)
				{
					param.OleDbType = OleDbType.BigInt;
					param.Value = parameter;
					mCommand_.Parameters.Add(param);
					mParameterString_ += param.Value + ", ";
				}
				else if (parameter is String)
				{
					param.Value = parameter;
					param.OleDbType = OleDbType.VarChar;
					param.Size = ((String)parameter).Length;
					mCommand_.Parameters.Add(param);
					mParameterString_ += param.Value + ", ";
				}
				else if (parameter is DateTime)
				{
					param.Value = parameter;
					param.OleDbType = OleDbType.DBDate;
					mCommand_.Parameters.Add(param);
					mParameterString_ += ((DateTime)param.Value) + ", ";
				}
				else if (parameter is Boolean)
				{
					param.Value = parameter;
					param.OleDbType = OleDbType.SmallInt;
					mCommand_.Parameters.Add(param);
					mParameterString_ += param.Value + ", ";
				}
				else
				{
					param.Value = parameter;
					param.OleDbType = OleDbType.IUnknown;
					mCommand_.Parameters.Add(param);
					mParameterString_ += param.Value + ", ";
				}

				position++;
			}
			mParameterString_ = mParameterString_.Substring(0, mParameterString_.Length - 2) + "]";
		}

		protected override void GenerateUpdateStatement(StoredData clazz, Type type)
		{
			throw new MiskoException("FoxPro UPDATEs are implemented on a class by class basis by overriding the Store(session) method");
		}

		protected override void GenerateDeleteStatement(StoredData clazz, Type type)
		{
			throw new MiskoException("FoxPro DELETEs are implemented on a class by class basis by overriding the Remove(session) method");
		}

		protected override void GenerateInsertStatement(StoredData clazz, Type type)
		{
			throw new MiskoException("FoxPro INSERTSs are implemented on a class by class basis by overriding the Create(session) method");
		}

		#endregion
	}
}
