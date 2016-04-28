using System;
using System.Diagnostics;
using System.Reflection;
using log4net;
using MiskoPersist.Data;
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
		
		public ErrorMessage ErrorMessage
		{
			get;
			set;
		}
		
		public override String Message
		{
			get
			{
				return ErrorMessage.Message;
			}
		}

		#endregion

		#region Constructors

		public MiskoException(ErrorMessage message) : base(message != null ? message.ToString() : "")
		{
			StackFrame stackframe = new StackFrame(1);
			Class = stackframe.GetMethod().DeclaringType;
			Method = stackframe.GetMethod();
			ErrorMessage = message;
		}

		public MiskoException(String message) : base(message)
		{
			StackFrame stackframe = new StackFrame(1);
			Class = stackframe.GetMethod().DeclaringType;
			Method = stackframe.GetMethod();
			ErrorMessage = new ErrorMessage(Class, Method, ErrorLevel.Error, message, null);
		}

		public MiskoException(String message, params Object[] parameters) : base(message)
		{
			StackFrame stackframe = new StackFrame(1);
			Class = stackframe.GetMethod().DeclaringType;
			Method = stackframe.GetMethod();
			ErrorMessage = new ErrorMessage(Class, Method, ErrorLevel.Error, message, parameters);
		}

		public MiskoException(String message, Exception inner) : base(message, inner)
		{
			StackFrame stackframe = new StackFrame(1);
			Class = stackframe.GetMethod().DeclaringType;
			Method = stackframe.GetMethod();
			ErrorMessage = new ErrorMessage(Class, Method, ErrorLevel.Error, message, null);
		}

		public MiskoException(String message, Exception inner, params Object[] parameters) : base(message, inner)
		{
			StackFrame stackframe = new StackFrame(1);
			Class = stackframe.GetMethod().DeclaringType;
			Method = stackframe.GetMethod();
			ErrorMessage = new ErrorMessage(Class, Method, ErrorLevel.Error, message, parameters);
		}

		#endregion
	}
}
