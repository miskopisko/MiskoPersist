using System;
using Newtonsoft.Json;

namespace MiskoPersist.Enums
{
    public class ErrorLevel : AbstractEnum
    {
        #region Fields

        private static readonly ErrorLevel mNULL_ = new ErrorLevel(-1, "", "");
        private static readonly ErrorLevel mSuccess_ = new ErrorLevel(0, "", "Success");
        private static readonly ErrorLevel mInfo_ = new ErrorLevel(1, "", "Information");
        private static readonly ErrorLevel mWarning_ = new ErrorLevel(2, "", "Warning");
        private static readonly ErrorLevel mConfirmation_ = new ErrorLevel(3, "", "Confirmation");
        private static readonly ErrorLevel mError_ = new ErrorLevel(4, "", "Error");

        private static readonly ErrorLevel[] mElements_ = new[]
		{
		    mNULL_, mSuccess_, mInfo_, mWarning_,mConfirmation_, mError_
		};

        #endregion

        #region Parameters

        public static ErrorLevel[] Elements { get { return mElements_; } }
        public static ErrorLevel NULL { get { return mNULL_; } }
        public static ErrorLevel Success { get { return mSuccess_; } }
        public static ErrorLevel Info { get { return mInfo_; } }
        public static ErrorLevel Warning { get { return mWarning_; } }
        public static ErrorLevel Confirmation { get { return mConfirmation_; } }
        public static ErrorLevel Error { get { return mError_; } }        

        [JsonIgnore]
        public Boolean IsCommitable 
        { 
        	get 
        	{ 
        		return this == Success || this == Warning || this == Info; 
        	} 
        }
        
        #endregion

        #region Constructors

        protected ErrorLevel()
        {
        }

        protected ErrorLevel(Int64 value, String code, String description) : base(value, code, description)
        {
        }

        #endregion

        #region Helpers

        public static ErrorLevel GetElement(long index)
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

        public static ErrorLevel GetElement(String descriptionCode)
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
