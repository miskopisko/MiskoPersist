using System;
using System.Collections.Generic;
using log4net;

namespace MiskoPersist.Data
{
	public class ErrorMessageParameters : ViewedDataList
	{
		private static ILog Log = LogManager.GetLogger(typeof(ErrorMessageParameters));
		
		#region Constructors
		
		public ErrorMessageParameters() : base(typeof(ErrorMessageParameter))
		{
		}
		
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
