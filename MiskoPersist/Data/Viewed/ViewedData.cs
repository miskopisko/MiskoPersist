using System;
using log4net;
using MiskoPersist.Core;

namespace MiskoPersist.Data.Viewed
{
	public class ViewedData : Data
	{
		private static ILog Log = LogManager.GetLogger(typeof(ViewedData));
		
		#region Fields

		

		#endregion

		#region Viewed Properties
		
		
		
		#endregion

		#region Constructors
	
		public ViewedData()
		{
		}

		public ViewedData(Session session, Persistence persistence) 
			: base(session, persistence)
		{
		}
		
		#endregion

		#region Private Methods

		

		#endregion

		#region Public Methods
		
		public ViewedData Clone()
		{
			return (ViewedData)MemberwiseClone();
		}
		
		#endregion
	}
}
