using System;
using System.Diagnostics;
using System.Reflection;
using log4net;
using MiskoPersist.Data.Viewed;
using MiskoPersist.Enums;

namespace MiskoPersist.Core
{
	public class MiskoException : Exception
	{
		private static ILog Log = LogManager.GetLogger(typeof(MiskoException));

		#region Fields

		
		
		#endregion

		#region Properties

		public Type Class
		{
			get;
			set;
		}
		
		public MethodBase Method
		{
			get;
			set;
		}
		
		public Int32? LineNumber
		{
			get;
			set;
		}
		
		public ErrorMessage ErrorMessage
		{
			get;
			set;
		}
		
		public override String Message
		{
			get
			{
				return ErrorMessage.ToString();
			}
		}

		#endregion

		#region Constructors

		public MiskoException(ErrorMessage message)
			: base(message != null ? message.ToString() : "")
		{
			StackFrame stackFrame = new StackFrame(1, true);
			Class = stackFrame.GetMethod().DeclaringType;
			Method = stackFrame.GetMethod();
			LineNumber = stackFrame.GetFileLineNumber() > 0 ? stackFrame.GetFileLineNumber() : (Int32?)null;
			ErrorMessage = message;
		}

		public MiskoException(String message)
			: base(message)
		{
			StackFrame stackFrame = new StackFrame(1, true);
			Class = stackFrame.GetMethod().DeclaringType;
			Method = stackFrame.GetMethod();
			LineNumber = stackFrame.GetFileLineNumber() > 0 ? stackFrame.GetFileLineNumber() : (Int32?)null;
			ErrorMessage = new ErrorMessage(Class, Method, LineNumber, ErrorLevel.Error, message, null);
		}

		public MiskoException(String message, params Object[] parameters)
			: base(message)
		{
			StackFrame stackFrame = new StackFrame(1, true);
			Class = stackFrame.GetMethod().DeclaringType;
			Method = stackFrame.GetMethod();
			LineNumber = stackFrame.GetFileLineNumber() > 0 ? stackFrame.GetFileLineNumber() : (Int32?)null;
			ErrorMessage = new ErrorMessage(Class, Method, LineNumber, ErrorLevel.Error, message, parameters);
		}

		public MiskoException(String message, Exception inner)
			: base(message, inner)
		{
			StackFrame stackFrame = new StackFrame(1, true);
			Class = stackFrame.GetMethod().DeclaringType;
			Method = stackFrame.GetMethod();
			LineNumber = stackFrame.GetFileLineNumber() > 0 ? stackFrame.GetFileLineNumber() : (Int32?)null;
			ErrorMessage = new ErrorMessage(Class, Method, LineNumber, ErrorLevel.Error, message, null);
		}

		public MiskoException(String message, Exception inner, params Object[] parameters)
			: base(message, inner)
		{
			StackFrame stackFrame = new StackFrame(1, true);
			Class = stackFrame.GetMethod().DeclaringType;
			Method = stackFrame.GetMethod();
			LineNumber = stackFrame.GetFileLineNumber();
			ErrorMessage = new ErrorMessage(Class, Method, LineNumber, ErrorLevel.Error, message, parameters);
		}

		#endregion
	}
}
