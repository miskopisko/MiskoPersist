using System;
using System.Collections.Generic;
using System.Reflection;
using Newtonsoft.Json;
using MiskoPersist.Data;
using MiskoPersist.Enums;
using MiskoPersist.Tools;

namespace MiskoPersist.Core
{
	[JsonObjectAttribute(MemberSerialization.OptOut)]
    public class ErrorMessage : AbstractViewedData
    {
        private static Logger Log = Logger.GetInstance(typeof(ErrorMessage));

        #region Fields

        private String mErrorMessage_;
        private Boolean? mConfirmed_ = false;

        #endregion

        #region Properties

		public String Class 
		{
			get;
			set;
		}
		
		public String Method 
		{
			get;
			set;
		}
		
		public List<Object> Parameters 
		{
			get;
			set;
		}
		
		public ErrorLevel ErrorLevel 
		{
			get;
			set;
		}
		
        public String Message 
        { 
        	get 
        	{ 
        		return ToString(); 
        	} 
        	set 
        	{ 
        		mErrorMessage_ = value;
        	} 
        }
        
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
        }

        public ErrorMessage(Exception e) 
        	: this(e.TargetSite.DeclaringType, e.TargetSite, ErrorLevel.Error, e.Message, null)
        {
        }

        public ErrorMessage(Type clazz, MethodBase method, ErrorLevel level, String message) 
        	: this(clazz, method, level, message, null)
        {
        }

        public ErrorMessage(Type clazz, MethodBase method, ErrorLevel level, String message, Object[] parameters)
        {
        	IsSet = true;
            Class = clazz.Name;
            Method = method.Name;
            ErrorLevel = level;
            mErrorMessage_ = message;
            Parameters = parameters != null ? new List<Object>(parameters) : null;
        }

        #endregion

        #region Override Methods

        public override string ToString()
        {
            return Utils.ResolveTextParameters(mErrorMessage_, Parameters != null ? Parameters.ToArray() : null);
        }
        
        #endregion

		#region Equals and GetHashCode implementation
		
		public override int GetHashCode()
		{
			int hashCode = 0;
			
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

		public override bool Equals(object obj)
		{
			ErrorMessage other = obj as ErrorMessage;
			
			if(other == null)
			{
				return false;
			}
			
			bool parametersEqual = true;
			if((Parameters != null && other.Parameters != null) && (Parameters.Count.Equals(other.Parameters.Count)))
			{
				parametersEqual = new HashSet<Object>(Parameters).SetEquals(new HashSet<Object>(other.Parameters));
			}
						
			return mErrorMessage_.Equals(other.mErrorMessage_) && Class.Equals(other.Class) && Method.Equals(other.Method) && ErrorLevel.Equals(other.ErrorLevel) && parametersEqual;
		}

		public static bool operator ==(ErrorMessage lhs, ErrorMessage rhs) 
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

		public static bool operator !=(ErrorMessage lhs, ErrorMessage rhs) 
		{
			return !(lhs == rhs);
		}

        #endregion
    }
}
