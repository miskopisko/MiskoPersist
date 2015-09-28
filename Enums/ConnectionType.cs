using System;

namespace MiskoPersist.Enums
{
    public class ConnectionType : AbstractEnum
    {
        #region Fields

        private static readonly ConnectionType mNULL_ = new ConnectionType(-1, "", "");
        private static readonly ConnectionType mSQLite_ = new ConnectionType(0, "", "SQLite");
        private static readonly ConnectionType mMySql_ = new ConnectionType(1, "", "MySql");
        private static readonly ConnectionType mOracle_ = new ConnectionType(2, "", "Oracle");
        private static readonly ConnectionType mPostgres_ = new ConnectionType(3, "", "Postgres");
        private static readonly ConnectionType mFoxPro_ = new ConnectionType(4, "", "FoxPro");

        private static readonly ConnectionType[] mElements_ = new[]
		{
		    mNULL_, mSQLite_, mMySql_, mOracle_, mPostgres_, mFoxPro_
		};

        #endregion

        #region Parameters

        public static ConnectionType[] Elements { get { return mElements_; } }
        public static ConnectionType NULL { get { return mNULL_; } }
        public static ConnectionType SQLite { get { return mSQLite_; } }
        public static ConnectionType MySql { get { return mMySql_; } }
        public static ConnectionType Oracle { get { return mOracle_; } }
        public static ConnectionType Postgres { get { return mPostgres_; } }
        public static ConnectionType FoxPro { get { return mFoxPro_; } }

        #endregion

        #region Constructors

        public ConnectionType()
        {
        }

        public ConnectionType(Int64 value, String code, String description) : base(value, code, description)
        {
        }

        #endregion

        #region Helpers

        public static ConnectionType GetElement(long index)
        {
            for (Int32 i = 0; Elements != null && i < Elements.Length; i++)
            {
                if (Elements[i].Value == index)
                {
                    return Elements[i];
                }
            }

            return null;
        }

        public static ConnectionType GetElement(String descriptionCode)
        {
            for (Int32 i = 0; descriptionCode != null && Elements != null && i < Elements.Length; i++)
            {
                if (Elements[i].Description.ToLower().Equals(descriptionCode.ToLower()) || Elements[i].Code.ToLower().Equals(descriptionCode.ToLower()))
                {
                    return Elements[i];
                }
            }

            return null;
        }

        #endregion
    }
}
