using System;
using System.Collections.Generic;
using log4net;

namespace MiskoPersist.Data.Viewed
{
	public class ErrorMessageParameters : ViewedDataList<ErrorMessageParameter>
	{
		private static ILog Log = LogManager.GetLogger(typeof(ErrorMessageParameters));
		
		#region Constructors
		
		
		
		#endregion
		
		#region Public Methods
		
		public String[] ToStringArray()
		{
			List<String> result = new List<String>();
			foreach (ErrorMessageParameter element in this)
			{
				result.Add(element.Parameter);
			}
			return result.ToArray();
		}
		
		#endregion
	}
}
