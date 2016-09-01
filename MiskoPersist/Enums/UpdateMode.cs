using System;

namespace MiskoPersist.Enums
{
	public class UpdateMode : MiskoEnum
	{
		#region Fields

		private static readonly UpdateMode mNULL_ = new UpdateMode(-1, "", "");
		private static readonly UpdateMode mInsert_ = new UpdateMode(0, "I", "Insert");
		private static readonly UpdateMode mUpdate_ = new UpdateMode(1, "U", "Update");
		private static readonly UpdateMode mDelete_ = new UpdateMode(2, "D", "Delete");

		private static readonly UpdateMode[] mElements_ = new[]
		{
			mNULL_, mInsert_, mUpdate_, mDelete_
		};

		#endregion

		#region Parameters

		public static UpdateMode[] Elements { get { return mElements_; } }
		public static UpdateMode NULL { get { return mNULL_; } }
		public static UpdateMode Insert { get { return mInsert_; } }
		public static UpdateMode Update { get { return mUpdate_; } }
		public static UpdateMode Delete { get { return mDelete_; } }

		#endregion

		#region Constructors

		public UpdateMode()
		{
		}

		public UpdateMode(Int64 value, String code, String description)
			: base(value, code, description)
		{
		}

		#endregion
	}
}
