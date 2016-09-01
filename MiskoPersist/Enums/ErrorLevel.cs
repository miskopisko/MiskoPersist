using System;

namespace MiskoPersist.Enums
{
	public class ErrorLevel : MiskoEnum
	{
		#region Fields

		private static readonly ErrorLevel mNULL_ = new ErrorLevel(-1, "", "");
		private static readonly ErrorLevel mSuccess_ = new ErrorLevel(0, "", "Success");
		private static readonly ErrorLevel mInformation_ = new ErrorLevel(1, "", "Information");
		private static readonly ErrorLevel mWarning_ = new ErrorLevel(2, "", "Warning");
		private static readonly ErrorLevel mConfirmation_ = new ErrorLevel(3, "", "Confirmation");
		private static readonly ErrorLevel mError_ = new ErrorLevel(4, "", "Error");

		private static readonly ErrorLevel[] mElements_ = new[]
		{
			mNULL_, mSuccess_, mInformation_, mWarning_, mConfirmation_, mError_
		};

		#endregion

		#region Parameters

		public static ErrorLevel[] Elements { get { return mElements_; } }
		public static ErrorLevel NULL { get { return mNULL_; } }
		public static ErrorLevel Success { get { return mSuccess_; } }
		public static ErrorLevel Information { get { return mInformation_; } }
		public static ErrorLevel Warning { get { return mWarning_; } }
		public static ErrorLevel Confirmation { get { return mConfirmation_; } }
		public static ErrorLevel Error { get { return mError_; } }

		public Boolean IsCommitable
		{
			get
			{
				return this == Success || this == Warning || this == Information;
			}
		}
		
		#endregion

		#region Constructors

		public ErrorLevel()
		{
		}

		public ErrorLevel(Int64 value, String code, String description)
			: base(value, code, description)
		{
		}

		#endregion
	}
}
