using System;

namespace MiskoPersist.Enums
{
	public class MessageStatus : MiskoEnum
	{
		#region Fields

		private static readonly MessageStatus mNULL_ = new MessageStatus(-1, "", "");
		private static readonly MessageStatus mProcessing_ = new MessageStatus(0, "", "Processing...");
		private static readonly MessageStatus mError_ = new MessageStatus(1, "", "Error!");
		private static readonly MessageStatus mWarning_ = new MessageStatus(2, "", "Warning!");
		private static readonly MessageStatus mInformation_ = new MessageStatus(3, "", "Information!");
		private static readonly MessageStatus mConfirmation_ = new MessageStatus(4, "", "Confirm?");
		private static readonly MessageStatus mSuccess_ = new MessageStatus(5, "", "Success!");

		private static readonly MessageStatus[] mElements_ = new[]
		{
			mNULL_, mProcessing_, mError_, mWarning_, mInformation_, mConfirmation_, mSuccess_
		};

		#endregion

		#region Parameters

		public static MessageStatus[] Elements { get { return mElements_; } }
		public static MessageStatus NULL { get { return mNULL_; } }
		public static MessageStatus Processing { get { return mProcessing_; } }
		public static MessageStatus Error { get { return mError_; } }
		public static MessageStatus Warning { get { return mWarning_; } }
		public static MessageStatus Information { get { return mInformation_; } }
		public static MessageStatus Confirmation { get { return mConfirmation_; } }
		public static MessageStatus Success { get { return mSuccess_; } }

		#endregion

		#region Constructors

		public MessageStatus()
		{
		}

		public MessageStatus(Int64 value, String code, String description)
			: base(value, code, description)
		{
		}

		#endregion
	}
}
