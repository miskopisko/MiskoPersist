using System;
using System.Data.Common;
using System.Data.OleDb;
using MiskoPersist.Core;
using MiskoPersist.Data;
using MiskoPersist.Enums;
using MiskoPersist.MoneyType;

namespace MiskoPersist.Persistences
{
    class FoxProPersistence : Persistence
    {
        private static Logger Log = Logger.GetInstance(typeof(FoxProPersistence));

        #region Variable Declarations



        #endregion

        #region Properties

		protected override DbDataAdapter DataAdapter
		{
			get
			{
				return new OleDbDataAdapter((OleDbCommand)mCommand_);
			}
		}

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
            Int32 position = 0;
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
                }
                else if (parameter is AbstractStoredData)
                {
                    param.IsNullable = false;
                    param.OleDbType = OleDbType.Integer;
                    param.Value = parameter != null ? (Object)((AbstractStoredData)parameter).Id : DBNull.Value;
                    mCommand_.Parameters.Add(param);
                }
                else if (parameter is AbstractEnum)
                {
                    param.IsNullable = true;
                    param.OleDbType = OleDbType.Integer;
                    param.Value = ((AbstractEnum)parameter).IsSet ? (Object)((AbstractEnum)parameter).Value : DBNull.Value;
                    mCommand_.Parameters.Add(param);
                }
                else if (parameter is Array)
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
                        position++;
                    }

                    mCommand_.CommandText = firstHalf + middle.Substring(0, middle.Length - 2) + secondHalf;
                }
                else if (parameter is Money)
                {
                    param.OleDbType = OleDbType.Currency;
                    param.Value = ((Money)parameter).ToDecimal(null);
                    mCommand_.Parameters.Add(param);
                }
                else if (parameter is PrimaryKey)
                {
                    param.OleDbType = OleDbType.BigInt;
                    param.Value = ((PrimaryKey)parameter).Value;
                    mCommand_.Parameters.Add(param);
                }
                else if (parameter is Int16)
                {
                    param.OleDbType = OleDbType.SmallInt;
                    param.Value = parameter;
                    mCommand_.Parameters.Add(param);
                }
                else if (parameter is Int32)
                {
                    param.OleDbType = OleDbType.Integer;
                    param.Value = parameter;
                    mCommand_.Parameters.Add(param);
                }
                else if (parameter is Int64)
                {
                    param.OleDbType = OleDbType.BigInt;
                    param.Value = parameter;
                    mCommand_.Parameters.Add(param);
                }
                else if(parameter is String)
                {
                	param.Value = parameter;
                    param.OleDbType = OleDbType.VarChar;
                    param.Size = ((String)parameter).Length;
                    mCommand_.Parameters.Add(param);
                }
                else if(parameter is DateTime)
                {
                	param.Value = parameter;
                    param.OleDbType = OleDbType.DBDate;
                    mCommand_.Parameters.Add(param);
                }
                else if(parameter is Boolean || parameter is Boolean)
                {
                	param.Value = parameter;
                    param.OleDbType = OleDbType.SmallInt;
                    mCommand_.Parameters.Add(param);
                }
                else
                {
                    param.Value = parameter;
                    param.OleDbType = OleDbType.IUnknown;
                    mCommand_.Parameters.Add(param);
                }

                position++;
            }
        }

        protected override void GenerateUpdateStatement(AbstractStoredData clazz, Type type)
        {
            throw new MiskoException("FoxPro UPDATEs are implemented on a class by class basis by overriding the Store(session) method");
        }

        protected override void GenerateDeleteStatement(AbstractStoredData clazz, Type type)
        {
            throw new MiskoException("FoxPro DELETEs are implemented on a class by class basis by overriding the Remove(session) method");
        }

        protected override void GenerateInsertStatement(AbstractStoredData clazz, Type type)
        {
            throw new MiskoException("FoxPro INSERTSs are implemented on a class by class basis by overriding the Create(session) method");
        }

        #endregion
    }
}
