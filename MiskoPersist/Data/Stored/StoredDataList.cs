using System.Collections;
using System.Collections.Generic;
using log4net;
using MiskoPersist.Core;

namespace MiskoPersist.Data.Stored
{
	public class StoredDataList<T> : List<T> where T : StoredData, new()
	{
		private static ILog Log = LogManager.GetLogger(typeof(StoredDataList<T>));
		
		#region Fields

		

		#endregion

		#region Properties

		

		#endregion

		#region Constructors

				
		
		#endregion

		#region Private Methods



		#endregion

		#region Public Methods

		public void Save(Session session)
		{
			foreach (StoredData item in this)
			{
				item.Save(session);
			}
		}
		
		public void Set(Session session, Persistence persistence)
		{
			while (!persistence.IsEof)
			{
				T data = new T();
				data.Set(session, persistence);
				((IList)this).Add(data);
				persistence.Next();
			}
		}

		public void FetchAll(Session session)
		{
			using (Persistence persistence = session.GetPersistence())
			{
				persistence.ExecuteQuery("SELECT * FROM " + typeof(T).Name);
				Set(session, persistence);
			}
		}

		public void Add(StoredDataList<T> list)
		{
			foreach (StoredData item in list)
			{
				((IList)this).Add(item);
			}
		}

		#endregion
	}
}
