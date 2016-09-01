using System;

namespace MiskoPersist.Enums
{
	public class MessageMode : MiskoEnum
	{
		#region Fields

		private static readonly MessageMode mNULL_ = new MessageMode(-1, "", "");
		private static readonly MessageMode mNormal_ = new MessageMode(0, "", "Normal");
		private static readonly MessageMode mTrial_ = new MessageMode(1, "", "Trial");

		private static readonly MessageMode[] mElements_ = new[]
		{
			mNULL_, mNormal_, mTrial_
		};

		#endregion

		#region Parameters

		public static MessageMode[] Elements { get { return mElements_; } }
		public static MessageMode NULL { get { return mNULL_; } }
		public static MessageMode Normal { get { return mNormal_; } }
		public static MessageMode Trial { get { return mTrial_; } }

		#endregion

		#region Constructors

		public MessageMode()
		{
		}

		public MessageMode(Int64 value, String code, String description)
			: base(value, code, description)
		{
		}

		#endregion
	}
}
