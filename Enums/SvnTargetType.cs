using System;

namespace MiskoPersist.Enums
{
	public class SvnTargetType : AbstractEnum
	{
		#region Fields

        private static readonly SvnTargetType mNULL_ = new SvnTargetType(-1, "", "");
        private static readonly SvnTargetType mFile_ = new SvnTargetType(0, "file", "File");
        private static readonly SvnTargetType mDirectory_ = new SvnTargetType(1, "dir", "Directory");
        
        private static readonly SvnTargetType[] mElements_ = new[]
        {
            mNULL_, mFile_, mDirectory_
        };

        #endregion

        #region Properties

        public static SvnTargetType[] Elements { get { return mElements_; } }
        public static SvnTargetType NULL { get { return mNULL_; } }
        public static SvnTargetType File { get { return mFile_; } }
        public static SvnTargetType Directory { get { return mDirectory_; } }

        #endregion	

        #region Constructor

        protected SvnTargetType()
        {
        }

        protected SvnTargetType(Int64 id, String Code, String Description) : base(id, Code, Description)
        {
        }

        #endregion

        #region Helpers

        public static SvnTargetType GetElement(long index)
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

        public static SvnTargetType GetElement(String descriptionCode)
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
