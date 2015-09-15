using System;
using System.Collections.Generic;
using System.ComponentModel;
using MiskoPersist.Core;

namespace MiskoPersist.Data
{
    public class AbstractViewedDataList<T> : BindingList<T> where T : AbstractViewedData, new()
    {
        #region Fields

        protected bool mIsSorted_;
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

			for (int i = 0; i < (pageNo - 1) * noRows && persistence.HasNext; i++)
            {
                persistence.Next();
            }

            for (int i = 0; (noRows == 0 || i < noRows) && persistence.HasNext; i++)
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
        	foreach (T item in list)
            {
                Items.Add(item);
            }
        }

        #endregion

        #region Private Methods



        #endregion

        #region Override Methods

        protected override bool SupportsSortingCore
        {
            get { return true; }
        }

        protected override bool IsSortedCore
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

        private int Compare(T lhs, T rhs)
        {
            var result = OnComparison(lhs, rhs);
            //invert if descending
            if (mSortDirection_ == ListSortDirection.Descending)
            {
                result = -result;
            }
                
            return result;
        }

        private int OnComparison(T lhs, T rhs)
        {
            object lhsValue = lhs == null ? null : mSortProperty_.GetValue(lhs);
            object rhsValue = rhs == null ? null : mSortProperty_.GetValue(rhs);
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
			return string.Compare(lhsValue.ToString(), rhsValue.ToString(), StringComparison.Ordinal);
        }

        #endregion
    }
}