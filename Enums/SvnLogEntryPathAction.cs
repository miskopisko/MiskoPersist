using System;

namespace MiskoPersist.Enums
{
    public class SvnLogEntryPathActionType : AbstractEnum
    {
        #region Fields

        private static readonly SvnLogEntryPathActionType mNULL_ = new SvnLogEntryPathActionType(-1, "", "");
        private static readonly SvnLogEntryPathActionType mAdded_ = new SvnLogEntryPathActionType(0, "A", "Added");
        private static readonly SvnLogEntryPathActionType mDeleted_ = new SvnLogEntryPathActionType(1, "D", "Deleted");
        private static readonly SvnLogEntryPathActionType mReplaced_ = new SvnLogEntryPathActionType(2, "R", "Replaced");
        private static readonly SvnLogEntryPathActionType mModified_ = new SvnLogEntryPathActionType(3, "M", "Modified");
        
        private static readonly SvnLogEntryPathActionType[] mElements_ = new[]
        {
            mNULL_, mAdded_, mDeleted_, mReplaced_, mModified_
        };

        #endregion

        #region Properties

        public static SvnLogEntryPathActionType[] Elements { get { return mElements_; } }
        public static SvnLogEntryPathActionType NULL { get { return mNULL_; } }
        public static SvnLogEntryPathActionType Added { get { return mAdded_; } }
        public static SvnLogEntryPathActionType Deleted { get { return mDeleted_; } }
        public static SvnLogEntryPathActionType Replaced { get { return mReplaced_; } }
        public static SvnLogEntryPathActionType Modified { get { return mModified_; } }

        #endregion	

        #region Constructor

        public SvnLogEntryPathActionType()
        {
        }

        public SvnLogEntryPathActionType(Int64 id, String Code, String Description) : base(id, Code, Description)
        {
        }

        #endregion

        #region Helpers

        public static SvnLogEntryPathActionType GetElement(long index)
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

        public static SvnLogEntryPathActionType GetElement(String descriptionCode)
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
