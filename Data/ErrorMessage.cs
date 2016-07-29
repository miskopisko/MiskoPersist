using System;
using System.Diagnostics;
using System.Reflection;
using log4net;
using MiskoPersist.Attributes;
using MiskoPersist.Enums;

namespace MiskoPersist.Data
{
	public class ErrorMessage : ViewedData
	{
		private static ILog Log = LogManager.GetLogger(typeof(ErrorMessage));

		#region Fields

		private Boolean? mConfirmed_ = false;

		#endregion

		#region Properties
		
		[Viewed]
		public ErrorLevel ErrorLevel
		{
			get;
			set;
		}

		[Viewed]
		public String Class
		{
			get;
			set;
		}

		[Viewed]
		public String Method
		{
			get;
			set;
		}
		
		[Viewed]
		public String Message
		{
			get;
			set;
		}

		[Viewed]
		public ErrorMessageParameters Parameters
		{
			get;
			set;
		}

		[Viewed]
		public Boolean? Confirmed
		{
			get
			{
				return ErrorLevel.Equals(ErrorLevel.Confirmation) ? mConfirmed_ : null;
			}
			set
			{
				mConfirmed_ = value;
			}
		}

		#endregion

		#region Constructors
		
		public ErrorMessage()
		{
			ErrorLevel = ErrorLevel.Success;
		}

		public ErrorMessage(Exception e)
		{
			if (e.TargetSite == null)
			{
				StackFrame stackFrame = new StackFrame(1);
				Class = stackFrame.GetMethod().DeclaringType.Name;
				Method = stackFrame.GetMethod().Name;
			}
			else
			{
				Class = e.TargetSite.DeclaringType.Name;
				Method = e.TargetSite.Name;
			}
			
			ErrorLevel = ErrorLevel.Error;
			Message = e.Message;
			Parameters = new ErrorMessageParameters();
		}
		
		public ErrorMessage(String message, Exception e)
			: this(e)
		{
			Message = message + Environment.NewLine + e.Message;
		}

		public ErrorMessage(Type clazz, MethodBase method, ErrorLevel level, String message)
			: this(clazz, method, level, message, null)
		{
		}

		public ErrorMessage(Type clazz, MethodBase method, ErrorLevel level, String message, params Object[] parameters)
		{
			Class = clazz.Name;
			Method = method.Name;
			ErrorLevel = level;
			Message = message;
			Parameters = new ErrorMessageParameters();

			if (parameters != null)
			{
				foreach (Object parameter in parameters)
				{
					Parameters.Add(new ErrorMessageParameter() { Parameter = parameter.ToString() });
				}
			}
		}

		#endregion

		#region Override Methods

		public override String ToString()
		{
			return String.Format(Message, Parameters.ToStringArray());
		}
		
		public override Int32 GetHashCode()
		{
			Int32 hashCode = 0;
			
			unchecked
			{
				if (Message != null)
				{
					hashCode += 1000000007 * Message.GetHashCode();
				}
				
				if (Class != null)
				{
					hashCode += 1000000021 * Class.GetHashCode();
				}
				
				if (Method != null)
				{
					hashCode += 1000000033 * Method.GetHashCode();
				}
				
				if (Parameters != null)
				{
					hashCode += 1000000087 * Parameters.GetHashCode();
				}
				
				if (ErrorLevel != null)
				{
					hashCode += 1000000093 * ErrorLevel.GetHashCode();
				}
			}
			
			return hashCode;
		}

		public override Boolean Equals(Object obj)
		{
			ErrorMessage other = obj as ErrorMessage;
			return other != null && Message.Equals(other.Message) && Class.Equals(other.Class) && Method.Equals(other.Method) && ErrorLevel.Equals(other.ErrorLevel) && Parameters.Equals(other.Parameters);
		}
		
		#endregion

		#region Operators

		public static Boolean operator ==(ErrorMessage lhs, ErrorMessage rhs)
		{
			if (ReferenceEquals(lhs, rhs))
			{
				return true;
			}
			
			if (ReferenceEquals(lhs, null) || ReferenceEquals(rhs, null))
			{
				return false;
			}
			
			return lhs.Equals(rhs);
		}

		public static Boolean operator !=(ErrorMessage lhs, ErrorMessage rhs)
		{
			return !(lhs == rhs);
		}

		#endregion
	}
}
