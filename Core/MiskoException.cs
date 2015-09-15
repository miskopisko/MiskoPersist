using System;
using System.Diagnostics;
using System.Reflection;
using MiskoPersist.Data;
using MiskoPersist.Enums;

namespace MiskoPersist.Core
{
	[Serializable]
	public class MiskoException : Exception
	{
		private static Logger Log = Logger.GetInstance(typeof(MiskoException));

		#region Fields

		private readonly Type mClass_;
		private readonly MethodBase mMethod_;
		private readonly ErrorMessage mErrorMessage_;

		#endregion

		#region Properties

		public Type Class 
		{
			get 
			{
				return mClass_;
			}
		}
		
		public MethodBase Method
		{
			get
			{
				return mMethod_;
			}
		}
		
		public ErrorMessage ErrorMessage
		{
			get
			{
				return mErrorMessage_;
			}
		}
		
		public override string Message
		{
			get
			{
				return ErrorMessage.Message;
			}
		}

		#endregion

		#region Constructors

		public MiskoException(ErrorMessage message) 
			: base(message != null ? message.ToString() : "")
		{
			StackFrame stackframe = new StackFrame(1);
			mClass_ = stackframe.GetMethod().DeclaringType;
			mMethod_ = stackframe.GetMethod();
			mErrorMessage_ = message;
		}

		public MiskoException(String message) 
			: base(message)
		{
			StackFrame stackframe = new StackFrame(1);
			mClass_ = stackframe.GetMethod().DeclaringType;
			mMethod_ = stackframe.GetMethod();
			mErrorMessage_ = new ErrorMessage(mClass_, mMethod_, ErrorLevel.Error, message, null);
		}

		public MiskoException(String message, String[] parameters) 
			: base(message)
		{
			StackFrame stackframe = new StackFrame(1);
			mClass_ = stackframe.GetMethod().DeclaringType;
			mMethod_ = stackframe.GetMethod();
			mErrorMessage_ = new ErrorMessage(mClass_, mMethod_, ErrorLevel.Error, message, parameters);
		}

		public MiskoException(String message, Exception inner) 
			: base(message, inner)
		{
			StackFrame stackframe = new StackFrame(1);
			mClass_ = stackframe.GetMethod().DeclaringType;
			mMethod_ = stackframe.GetMethod();
			mErrorMessage_ = new ErrorMessage(mClass_, mMethod_, ErrorLevel.Error, message, null);
		}

		public MiskoException(String message, String[] parameters, Exception inner) 
			: base(message, inner)
		{
			StackFrame stackframe = new StackFrame(1);
			mClass_ = stackframe.GetMethod().DeclaringType;
			mMethod_ = stackframe.GetMethod();
			mErrorMessage_ = new ErrorMessage(mClass_, mMethod_, ErrorLevel.Error, message, parameters);
		}

		#endregion
	}
}
