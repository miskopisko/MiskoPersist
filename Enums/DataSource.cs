using System;

namespace MiskoPersist.Enums
{
    public class DataSource : AbstractEnum
    {
        #region Fields

        private static readonly DataSource mNULL_ = new DataSource(-1, "", "");
        private static readonly DataSource mLocal_ = new DataSource(0, "", "Local");
        private static readonly DataSource mOnline_ = new DataSource(1, "", "Online");

        private static readonly DataSource[] mElements_ = new[]
		{
		    mNULL_, mLocal_, mOnline_
		};

        #endregion

        #region Parameters

        public static DataSource[] Elements { get { return mElements_; } }
        public static DataSource NULL { get { return mNULL_; } }
        public static DataSource Local { get { return mLocal_; } }
        public static DataSource Online { get { return mOnline_; } }

        #endregion

        #region Constructors

        protected DataSource()
        {
        }

        protected DataSource(Int64 value, String code, String description) : base(value, code, description)
        {
        }

        #endregion

        #region Helpers

        public static DataSource GetElement(long index)
        {
            for (int i = 0; Elements != null && i < Elements.Length; i++)
            {
                if (Elements[i].Value == index)
                {
                    return Elements[i];
                }
            }

            return null;
        }

        public static DataSource GetElement(String descriptionCode)
        {
            for (int i = 0; descriptionCode != null && Elements != null && i < Elements.Length; i++)
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
