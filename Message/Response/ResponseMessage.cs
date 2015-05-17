using System;
using System.Runtime.Remoting.Messaging;
using Newtonsoft.Json;
using MiskoPersist.Core;
using MiskoPersist.Data;
using MiskoPersist.Enums;

namespace MiskoPersist.Message.Response
{
	[JsonObjectAttribute(MemberSerialization.OptOut)]
    public class ResponseMessage
    {
        private static Logger Log = Logger.GetInstance(typeof(ResponseMessage));

        #region Fields

        private ErrorMessages mErrors_;
        private ErrorMessages mInfos_;
        private ErrorMessages mWarnings_;
        private ErrorMessages mConfirmations_;

        #endregion

        #region Properties

        public ErrorLevel Status 
        { 
        	get; 
        	set; 
        }        
		
        public ErrorMessages Errors 
        {
			get 
			{
				return HasErrors ? mErrors_ : null;
			}
			set 
			{
				mErrors_ = value;
			}
		}
        
        public ErrorMessages Infos 
        { 
        	get 
			{
        		return HasInfos ? mInfos_ : null;
			}
			set 
			{
				mInfos_ = value;
			} 
        }
        
        public ErrorMessages Warnings 
        { 
        	get 
			{
				return HasWarnings ? mWarnings_ : null;
			}
			set 
			{
				mWarnings_ = value;
			} 
        }
        
        public ErrorMessages Confirmations 
        { 
        	get 
			{
				return HasConfirmations ? mConfirmations_ : null;
			}
			set 
			{
				mConfirmations_ = value;
			}
        }
        
        public Page Page 
        { 
        	get;
        	set;
        }
        
        [JsonIgnore]
        public Boolean HasErrors 
        { 
        	get 
        	{ 
        		return mErrors_ != null && mErrors_.Count > 0; 
        	} 
        }
        
        [JsonIgnore]
        public Boolean HasUnconfirmed 
        {
        	get
        	{
        		bool hasUnconfirmed = false;
        		if(HasConfirmations)
        		{
        			foreach (ErrorMessage confirmMessage in Confirmations) 
        			{
        				if(confirmMessage.Confirmed.HasValue && !confirmMessage.Confirmed.Value)
        				{
        					hasUnconfirmed = true;
        					break;
        				}
        			}
        		}
        		return hasUnconfirmed;
        	}
        }
        
        [JsonIgnore]
        public Boolean HasInfos 
        { 
        	get 
        	{ 
        		return mInfos_ != null && mInfos_.Count > 0; 
        	} 
        }
        
        [JsonIgnore]
        public Boolean HasWarnings 
        { 
        	get 
        	{ 
        		return mWarnings_ != null && mWarnings_.Count > 0; 
        	} 
        }
        
        [JsonIgnore]
        public Boolean HasConfirmations 
        { 
        	get 
        	{ 
        		return mConfirmations_ != null && mConfirmations_.Count > 0; 
        	} 
        }
        
        [JsonIgnore]
        public ErrorMessages AllMessages
        {
        	get
        	{
        		ErrorMessages all = new ErrorMessages();
        		
        		if(mErrors_ != null && mErrors_.Count > 0)
        		{
        			all.AddRange(Errors);
        		}
        		if(HasWarnings)
        		{
        			all.AddRange(Warnings);
        		}
        		if(HasConfirmations)
        		{
        			all.AddRange(Confirmations);
        		}
        		if(HasInfos)
        		{
        			all.AddRange(Infos);
        		}
        		
        		return all;
        	}
        }

        #endregion

        #region Constructors

        public  ResponseMessage()
        {
        }

        #endregion

        #region Private Properties



        #endregion
    }
}
