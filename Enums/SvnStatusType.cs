using System;

namespace MiskoPersist.Enums
{
	public class SvnStatusType : AbstractEnum
	{
		#region Fields

        private static readonly SvnStatusType mNULL_ = new SvnStatusType(-1, "", "");
        private static readonly SvnStatusType mNoChange_ = new SvnStatusType(0, "normal", "No Change");
        private static readonly SvnStatusType mAdded_ = new SvnStatusType(1, "added", "Added");
        private static readonly SvnStatusType mDeleted_ = new SvnStatusType(2, "deleted", "Deleted");
        private static readonly SvnStatusType mModified_ = new SvnStatusType(3, "modified", "Modified");
        private static readonly SvnStatusType mReplaced_ = new SvnStatusType(4, "replaced", "Replaced");
        private static readonly SvnStatusType mConflicted_ = new SvnStatusType(5, "conflicted", "Conflicted");
        private static readonly SvnStatusType mExternal_ = new SvnStatusType(6, "external", "External");
        private static readonly SvnStatusType mIgnored_ = new SvnStatusType(7, "ignored", "Ignored");
        private static readonly SvnStatusType mNotVersioned_ = new SvnStatusType(8, "unversioned", "Not Versioned");
        private static readonly SvnStatusType mMissing_ = new SvnStatusType(9, "missing", "Missing");
        private static readonly SvnStatusType mIncomplete_ = new SvnStatusType(10, "incomplete", "Incomplete");
        private static readonly SvnStatusType mMerged_ = new SvnStatusType(11, "merged", "Merged");
        private static readonly SvnStatusType mNone_ = new SvnStatusType(12, "none", "None");
        private static readonly SvnStatusType mObstructed_ = new SvnStatusType(13, "obstructed", "Obstructed");
        private static readonly SvnStatusType mModifiedProperty_ = new SvnStatusType(14, "modifiedProperty", "Modified (Property Only)");
        private static readonly SvnStatusType mLocked_ = new SvnStatusType(15, "locked", "Locked (No Change)");

        private static readonly SvnStatusType[] mElements_ = new[]
        {
            mNoChange_, mAdded_, mDeleted_, mModified_,
            mReplaced_, mConflicted_, mExternal_, mIgnored_,
            mNotVersioned_, mMissing_, mIncomplete_,
            mMerged_, mNone_, mObstructed_, mModifiedProperty_, mLocked_
        };

        #endregion

        #region Properties

        public static SvnStatusType[] Elements { get { return mElements_; } }
        public static SvnStatusType NULL { get { return mNULL_; } }
        public static SvnStatusType NoChange { get { return mNoChange_; } }
        public static SvnStatusType Added { get { return mAdded_; } }
        public static SvnStatusType Deleted { get { return mDeleted_; } }
        public static SvnStatusType Modified { get { return mModified_; } }
        public static SvnStatusType Replaced { get { return mReplaced_; } }
        public static SvnStatusType Conflicted { get { return mConflicted_; } }
        public static SvnStatusType External { get { return mExternal_; } }
        public static SvnStatusType Ignored { get { return mIgnored_; } }
        public static SvnStatusType NotVersioned { get { return mNotVersioned_; } }
        public static SvnStatusType Missing { get { return mMissing_; } }
        public static SvnStatusType Incomplete { get { return mIncomplete_; } }
        public static SvnStatusType Merged { get { return mMerged_; } }
        public static SvnStatusType None { get { return mNone_; } }
        public static SvnStatusType Obstructed { get { return mObstructed_; } }
        public static SvnStatusType ModifiedProperty { get { return mModifiedProperty_; } }
        public static SvnStatusType Locked { get { return mLocked_; } }

        #endregion

        #region Constructor

        public SvnStatusType()
        {
        }

        public SvnStatusType(Int64 id, String Code, String Description) : base(id, Code, Description)
        {
        }

        #endregion

        #region Helpers

        public static SvnStatusType GetElement(long index)
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
        
        public static SvnStatusType GetElement(String descriptionCode)
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

        public Boolean IsModified()
        {
            return this.Equals(SvnStatusType.Modified) || this.Equals(SvnStatusType.ModifiedProperty) || this.Equals(SvnStatusType.Replaced);
        }

        #endregion
    }
}
