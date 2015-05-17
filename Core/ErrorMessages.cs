using System;
using MiskoPersist.Data;
using MiskoPersist.Enums;

namespace MiskoPersist.Core
{
    public class ErrorMessages : AbstractViewedDataList<ErrorMessage>
    {
        private static Logger Log = Logger.GetInstance(typeof(ErrorMessages));

        #region Fields



        #endregion

        #region Properties

        

        #endregion

        #region Constructors



        #endregion

        #region Private Methods



        #endregion

        #region Public Methods

        public Boolean Contains(ErrorLevel level)
        {
            if (level != null && Count > 0)
            {
                foreach (ErrorMessage message in this)
                {
                    if (message.ErrorLevel.Equals(level))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        public ErrorMessages ListOf(ErrorLevel level)
        {
            ErrorMessages list = new ErrorMessages();

            if (level != null && Count > 0)
            {
                foreach (ErrorMessage message in this)
                {
                    if (message.ErrorLevel.Equals(level))
                    {
                        list.Add(message);
                    }
                }
            }

            return list;
        }

        #endregion
    }
}
