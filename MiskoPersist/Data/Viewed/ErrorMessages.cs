using System;
using log4net;
using MiskoPersist.Enums;

namespace MiskoPersist.Data.Viewed
{
	public class ErrorMessages : ViewedDataList<ErrorMessage>
	{
		private static ILog Log = LogManager.GetLogger(typeof(ErrorMessages));

		#region Fields



		#endregion

		#region Properties

		

		#endregion

		#region Constructors

		

		#endregion

		#region Private Methods



		#endregion

		#region Public Methods
		
		public Boolean IsConfirmed(ErrorMessage errorMessage)
		{
			foreach (ErrorMessage message in this)
			{
				if (message.Equals(errorMessage))
				{
					return message.Confirmed.HasValue && message.Confirmed.Value;
				}
			}
			
			return false;
		}

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
