using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Xml;
using MiskoPersist.Core;

namespace MiskoPersist.Data
{
	public class AbstractViewedDataList<T> : BindingList<T>, ICloneable where T : AbstractViewedData, new()
    {
        #region Fields

        protected Boolean mIsSorted_;
        protected ListSortDirection mSortDirection_ = ListSortDirection.Ascending;
        protected PropertyDescriptor mSortProperty_;

        #endregion

        #region Properties

        

        #endregion

        #region Constructors

        public AbstractViewedDataList()
        {
        }
        
        public AbstractViewedDataList(Session session, Persistence persistence)
        {
        	Set(session, persistence);
        }

        #endregion

        #region Public Methods

        public void Set(Session session, Persistence persistence)
        {
            Set(session, persistence, new Page());
        }

        public void Set(Session session, Persistence persistence, Page page)
        {
        	if(!persistence.HasNext)
        	{
        		page.PageNo = 0;
        		return;
        	}
        	
            Int32 noRows = page.RowsPerPage;
            Int32 pageNo = page.PageNo;

			for (Int32 i = 0; i < (pageNo - 1) * noRows && persistence.HasNext; i++)
            {
                persistence.Next();
            }

            for (Int32 i = 0; (noRows == 0 || i < noRows) && persistence.HasNext; i++)
            {
                T data = new T();
                data.Set(session, persistence);
                Add(data);
            }

            if(page.IncludeRecordCount)
            {
                page.TotalRowCount = persistence.RecordCount;
            }
        }

        public void FetchAll(Session session)
        {
            Persistence persistence = Persistence.GetInstance(session);
            persistence.ExecuteQuery("SELECT * FROM " + typeof(T).Name);
            Set(session, persistence, new Page());
            persistence.Close();
            persistence = null;
        }

        public void AddRange(AbstractViewedDataList<T> list)
        {
        	if(list != null)
        	{
	        	foreach (T item in list)
	            {
	                Items.Add(item);
	            }
        	}
        }

        #endregion

        #region Private Methods



        #endregion

        #region Override Methods

        protected override Boolean SupportsSortingCore
        {
            get { return true; }
        }

        protected override Boolean IsSortedCore
        {
            get { return mIsSorted_; }
        }

        protected override ListSortDirection SortDirectionCore
        {
            get { return mSortDirection_; }
        }

        protected override PropertyDescriptor SortPropertyCore
        {
            get { return mSortProperty_; }
        }

        protected override void RemoveSortCore()
        {
            mSortDirection_ = ListSortDirection.Ascending;
            mSortProperty_ = null;
        }

        protected override void ApplySortCore(PropertyDescriptor prop, ListSortDirection direction)
        {
            mSortProperty_ = prop;
            mSortDirection_ = direction;

            List<T> list = Items as List<T>;
            if (list == null) return;

            list.Sort(Compare);

            mIsSorted_ = true;
            //fire an event that the list has been changed.
            OnListChanged(new ListChangedEventArgs(ListChangedType.Reset, -1));
        }

        private Int32 Compare(T lhs, T rhs)
        {
            var result = OnComparison(lhs, rhs);
            //invert if descending
            if (mSortDirection_ == ListSortDirection.Descending)
            {
                result = -result;
            }
                
            return result;
        }

        private Int32 OnComparison(T lhs, T rhs)
        {
            Object lhsValue = lhs == null ? null : mSortProperty_.GetValue(lhs);
            Object rhsValue = rhs == null ? null : mSortProperty_.GetValue(rhs);
            if (lhsValue == null)
            {
                return (rhsValue == null) ? 0 : -1; //nulls are equal
            }
            if (rhsValue == null)
            {
                return 1; //first has value, second doesn't
            }
            if (lhsValue is IComparable)
            {
                return ((IComparable)lhsValue).CompareTo(rhsValue);
            }
            if (lhsValue.Equals(rhsValue))
            {
                return 0; //both are the same
            }
            //not comparable, compare ToString
			return String.Compare(lhsValue.ToString(), rhsValue.ToString(), StringComparison.Ordinal);
        }

        #endregion

		#region ICloneable implementation
		
		public Object Clone()
		{
			var result = Activator.CreateInstance(GetType());
			
			foreach(T data in this)
			{
				((AbstractViewedDataList<T>)result).Add((T)data.Clone());
			}
			
			return result;
		}

		#endregion

		#region Xml Serialization

		public static AbstractViewedDataList<T> ReadXml(XmlElement XML, String name, Type type)
		{
			AbstractViewedDataList<T> list = (AbstractViewedDataList<T>)Activator.CreateInstance(type);

			if (XML != null)
			{
				foreach (XmlNode n in XML.ChildNodes)
				{
					if (n.Name == name)
					{
						T value = new T();
						value.XML = (XmlElement)n;
						value.IsSet = true;
						value.ReadXml(null);
						list.Add(value);
					}
				}
			}

			return list;
		}

		public void WriteXml(XmlWriter writer, String name)
		{
			foreach (T element in this) 
			{
				writer.WriteStartElement(name);
				element.WriteXml(writer);
				writer.WriteEndElement();
			}
		}

		#endregion
    }
}