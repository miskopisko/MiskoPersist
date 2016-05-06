using System;

namespace MiskoPersist.Enums
{
	public class SqlSortDirection : MiskoEnum
	{
		#region Fields

		private static readonly SqlSortDirection mNULL_ = new SqlSortDirection(-1, "", "");
		private static readonly SqlSortDirection mAscending_ = new SqlSortDirection(0, "", "Ascending");
		private static readonly SqlSortDirection mDescending_ = new SqlSortDirection(1, " Desc", "Descending");

		private static readonly SqlSortDirection[] mElements_ = new[]
		{
			mNULL_, mAscending_, mDescending_
		};

		#endregion

		#region Parameters

		public static SqlSortDirection[] Elements { get { return mElements_; } }
		public static SqlSortDirection NULL { get { return mNULL_; } }
		public static SqlSortDirection Ascending { get { return mAscending_; } }
		public static SqlSortDirection Descending { get { return mDescending_; } }

		#endregion

		#region Constructors

		public SqlSortDirection()
		{
		}

		public SqlSortDirection(Int64 value, String code, String description)
			: base(value, code, description)
		{
		}

		#endregion

		#region Helpers

		public static SqlSortDirection GetElement(long index)
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

		public static SqlSortDirection GetElement(String descriptionCode)
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
