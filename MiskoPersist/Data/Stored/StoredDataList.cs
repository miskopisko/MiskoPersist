using System;
using System.Collections;
using log4net;
using MiskoPersist.Core;

namespace MiskoPersist.Data.Stored
{
	public class StoredDataList : CollectionBase
	{
		private static ILog Log = LogManager.GetLogger(typeof(StoredDataList));
		
		#region Fields

		

		#endregion

		#region Properties

		public Type StoredDataType
		{
			get;
			private set;
		}

		#endregion

		#region Constructors

		public StoredDataList(Type itemType)
		{
			if (!typeof(StoredData).IsAssignableFrom(itemType))
			{
				throw new MiskoException("Cannot add {0} to a StoredDataList", itemType);
			}
			
			StoredDataType = itemType;
		}

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
				StoredData data = (StoredData)Activator.CreateInstance(StoredDataType);
				data.Set(session, persistence);
				((IList)this).Add(data);
				persistence.Next();
			}
		}

		public void FetchAll(Session session)
		{
			Persistence persistence = Persistence.GetInstance(session);
			persistence.ExecuteQuery("SELECT * FROM " + StoredDataType.Name);
			Set(session, persistence);
			persistence.Close();
			persistence = null;
		}

		public void AddRange(StoredDataList list)
		{
			foreach (StoredData item in list)
			{
				((IList)this).Add(item);
			}
		}

		#endregion
	}
}
