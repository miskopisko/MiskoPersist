using System;

namespace MiskoPersist.Enums
{
	public class SvnPropsStatusType : AbstractEnum
	{
		#region Fields

        private static readonly SvnPropsStatusType mNULL_ = new SvnPropsStatusType(-1, "", "");
        private static readonly SvnPropsStatusType mNoChange_ = new SvnPropsStatusType(0, "none", "None");
        private static readonly SvnPropsStatusType mAdded_ = new SvnPropsStatusType(1, "normal", "No Change");
        private static readonly SvnPropsStatusType mModified_ = new SvnPropsStatusType(2, "modified", "Modified");
        private static readonly SvnPropsStatusType mConflicted_ = new SvnPropsStatusType(3, "conflicted", "Conflicted");
        
        private static readonly SvnPropsStatusType[] mElements_ = new[]
        {
            mNULL_, mNoChange_, mAdded_, mModified_, mConflicted_
        };

        #endregion

        #region Parameters

        public static SvnPropsStatusType[] Elements { get { return mElements_; } }
        public static SvnPropsStatusType NULL { get { return mNULL_; } }
        public static SvnPropsStatusType NoChange { get { return mNoChange_; } }        
        public static SvnPropsStatusType Added { get { return mAdded_; } }
        public static SvnPropsStatusType Modified { get { return mModified_; } }
        public static SvnPropsStatusType Conflicted { get { return mConflicted_;} }

        #endregion

        #region Constructors

        protected SvnPropsStatusType()
        {
        }

        protected SvnPropsStatusType(Int64 id, String Code, String Description) : base(id, Code, Description)
        {
        }

        #endregion

        #region Helpers

        public static SvnPropsStatusType GetElement(long index)
        {
            for (int i = 0; mElements_ != null && i < mElements_.Length; i++)
            {
                if (mElements_[i].Value == index)
                {
                    return mElements_[i];
                }
            }

            return null;
        }

        public static SvnPropsStatusType GetElement(String descriptionCode)
        {
            for (int i = 0; descriptionCode != null && mElements_ != null && i < mElements_.Length; i++)
            {
                if (mElements_[i].Description.ToLower().Equals(descriptionCode.ToLower()) || mElements_[i].Code.ToLower().Equals(descriptionCode.ToLower()))
                {
                    return mElements_[i];
                }
            }

            return null;
        }

        #endregion
    }
}
