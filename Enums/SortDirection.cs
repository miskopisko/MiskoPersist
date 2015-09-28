using System;

namespace MiskoPersist.Enums
{
    public class SortDirection : AbstractEnum
    {
        #region Fields

        private static readonly SortDirection mNULL_ = new SortDirection(-1, "", "");
        private static readonly SortDirection mAscending_ = new SortDirection(0, "", "Ascending");
        private static readonly SortDirection mDescending_ = new SortDirection(1, " Desc", "Descending");

        private static readonly SortDirection[] mElements_ = new[]
		{
		    mNULL_, mAscending_, mDescending_
		};

        #endregion

        #region Parameters

        public static SortDirection[] Elements { get { return mElements_; } }
        public static SortDirection NULL { get { return mNULL_; } }
        public static SortDirection Ascending { get { return mAscending_; } }
        public static SortDirection Descending { get { return mDescending_; } }

        #endregion

        #region Constructors

        public SortDirection()
        {
        }

        public SortDirection(Int64 value, String code, String description) : base(value, code, description)
        {
        }

        #endregion

        #region Helpers

        public static SortDirection GetElement(long index)
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

        public static SortDirection GetElement(String descriptionCode)
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
