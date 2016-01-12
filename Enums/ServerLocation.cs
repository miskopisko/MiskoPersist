using System;

namespace MiskoPersist.Enums
{
	public class ServerLocation : AbstractEnum
    {
        #region Fields

        private static readonly ServerLocation mNULL_ = new ServerLocation(-1, "", "");
        private static readonly ServerLocation mLocal_ = new ServerLocation(0, "", "Local");
        private static readonly ServerLocation mOnline_ = new ServerLocation(1, "", "Online");

        private static readonly ServerLocation[] mElements_ = new[]
		{
		    mNULL_, mLocal_, mOnline_
		};

        #endregion

        #region Parameters

        public static ServerLocation[] Elements { get { return mElements_; } }
        public static ServerLocation NULL { get { return mNULL_; } }
        public static ServerLocation Local { get { return mLocal_; } }
        public static ServerLocation Online { get { return mOnline_; } }

        #endregion

        #region Constructors

        public ServerLocation()
        {
        }

        public ServerLocation(Int64 value, String code, String description) : base(value, code, description)
        {
        }

        #endregion

        #region Helpers

        public static ServerLocation GetElement(long index)
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

        public static ServerLocation GetElement(String descriptionCode)
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
