using System;
using MiskoPersist.Core;
using MiskoPersist.Data;
using MiskoPersist.Enums;

namespace MiskoPersist.Message.Request
{
    public class RequestMessage
    {
        private static Logger Log = Logger.GetInstance(typeof(RequestMessage));

        #region Fields

		

        #endregion

        #region Properties

        public MessageMode MessageMode 
        { 
        	get;
        	set; 
        }
        
        public String Command 
        { 
        	get;
        	set; 
        }
        
        public String Connection 
        { 
        	get;
        	set; 
        }
        
        public Page Page 
        { 
        	get;
        	set; 
        }
        
        public ErrorMessages Confirms
        {
        	get;
        	set;
        }

        #endregion

        #region Constructors

        public RequestMessage()
        {
        }

        #endregion

        #region Private Properties

        
        
        #endregion
    }
}
