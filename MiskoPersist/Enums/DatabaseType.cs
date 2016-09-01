using System;

namespace MiskoPersist.Enums
{
	public class DatabaseType : MiskoEnum
	{
		#region Fields

		private static readonly DatabaseType mNULL_ = new DatabaseType(-1, "", "");
		private static readonly DatabaseType mSQLite_ = new DatabaseType(0, "", "SQLite");
		private static readonly DatabaseType mMySql_ = new DatabaseType(1, "", "MySql");
		private static readonly DatabaseType mOracle_ = new DatabaseType(2, "", "Oracle");
		private static readonly DatabaseType mPostgres_ = new DatabaseType(3, "", "Postgres");
		private static readonly DatabaseType mFoxPro_ = new DatabaseType(4, "", "FoxPro");

		private static readonly DatabaseType[] mElements_ = new[]
		{
			mNULL_, mSQLite_, mMySql_, mOracle_, mPostgres_, mFoxPro_
		};

		#endregion

		#region Parameters

		public static DatabaseType[] Elements { get { return mElements_; } }
		public static DatabaseType NULL { get { return mNULL_; } }
		public static DatabaseType SQLite { get { return mSQLite_; } }
		public static DatabaseType MySql { get { return mMySql_; } }
		public static DatabaseType Oracle { get { return mOracle_; } }
		public static DatabaseType Postgres { get { return mPostgres_; } }
		public static DatabaseType FoxPro { get { return mFoxPro_; } }

		#endregion

		#region Constructors

		public DatabaseType()
		{
		}

		public DatabaseType(Int64 value, String code, String description)
			: base(value, code, description)
		{
		}

		#endregion
	}
}
