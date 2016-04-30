using System;
using System.Data.OleDb;
using log4net;
using MiskoPersist.Core;
using MiskoPersist.Data;
using MiskoPersist.Enums;
using MiskoPersist.MoneyType;
using MiskoPersist.Serialization;

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

        public FoxProPersistence(Session session) : base(session)
        {
        }

        #endregion

        #region Private Methods



        #endregion

        #region Override Methods

        protected override void SetParameters()
        {
        	if(mParameters_.Count == 0)
        	{
        		return;
        	}
        	
            Int32 position = 0;
            String paramString = "[";
            foreach (Object parameter in mParameters_)
            {
                OleDbParameter param = new OleDbParameter();
                param.ParameterName = position.ToString();

                if(parameter == null || (parameter is String && String.IsNullOrEmpty((String)parameter)))
                {
                    param.IsNullable = true;
                    param.Value = DBNull.Value;
                    param.OleDbType = OleDbType.IUnknown;
                    mCommand_.Parameters.Add(param);
                    paramString += "NULL, ";
                }
                else if(parameter is StoredData)
                {
                    param.IsNullable = false;
                    param.OleDbType = OleDbType.Integer;
                    param.Value = parameter != null ? (Object)((StoredData)parameter).Id : DBNull.Value;
                    mCommand_.Parameters.Add(param);
                    paramString += param.Value + ", ";
                }
                else if(parameter is MiskoEnum)
                {
                    param.IsNullable = true;
                    param.OleDbType = OleDbType.Integer;
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
                        OleDbParameter innerParam = new OleDbParameter();
                        innerParam.ParameterName = position.ToString();
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
                    param.OleDbType = OleDbType.Currency;
                    param.Value = ((Money)parameter).ToDecimal(null);
                    mCommand_.Parameters.Add(param);
                    paramString += param.Value + ", ";
                }
                else if(parameter is PrimaryKey)
                {
                    param.OleDbType = OleDbType.BigInt;
                    param.Value = ((PrimaryKey)parameter).Value;
                    mCommand_.Parameters.Add(param);
                    paramString += param.Value + ", ";
                }
                else if(parameter is Int16)
                {
                    param.OleDbType = OleDbType.SmallInt;
                    param.Value = parameter;
                    mCommand_.Parameters.Add(param);
                    paramString += param.Value + ", ";
                }
                else if(parameter is Int32)
                {
                    param.OleDbType = OleDbType.Integer;
                    param.Value = parameter;
                    mCommand_.Parameters.Add(param);
                    paramString += param.Value + ", ";
                }
                else if(parameter is Int64)
                {
                    param.OleDbType = OleDbType.BigInt;
                    param.Value = parameter;
                    mCommand_.Parameters.Add(param);
                    paramString += param.Value + ", ";
                }
                else if(parameter is String)
                {
                	param.Value = parameter;
                    param.OleDbType = OleDbType.VarChar;
                    param.Size = ((String)parameter).Length;
                    mCommand_.Parameters.Add(param);
                    paramString += param.Value + ", ";
                }
                else if(parameter is DateTime)
                {
                	param.Value = parameter;
                    param.OleDbType = OleDbType.DBDate;
                    mCommand_.Parameters.Add(param);
                    paramString += ((DateTime)param.Value).ToString(Serializer.DATEFORMAT) + ", ";
                }
                else if(parameter is Boolean)
                {
                	param.Value = parameter;
                    param.OleDbType = OleDbType.SmallInt;
                    mCommand_.Parameters.Add(param);
                    paramString += param.Value + ", ";
                }
                else
                {
                    param.Value = parameter;
                    param.OleDbType = OleDbType.IUnknown;
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
