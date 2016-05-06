using System;
using log4net;
using MiskoPersist.Attributes;

namespace MiskoPersist.Data
{
	public class ErrorMessageParameter : ViewedData
	{
		private static ILog Log = LogManager.GetLogger(typeof(ErrorMessageParameter));
		
		#region Fields

		

		#endregion

		#region Viewed Properties
		
		[Viewed]
		public String Parameter { get; set; }
		
		#endregion
	}
}
