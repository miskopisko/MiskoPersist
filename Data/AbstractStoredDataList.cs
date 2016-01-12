using System;
using System.Collections.Generic;
using System.ComponentModel;
using MiskoPersist.Core;

namespace MiskoPersist.Data
{
	public class AbstractStoredDataList<T> : BindingList<T> where T : AbstractStoredData, new()
    {
        #region Fields

        private Boolean mIsSorted_;
        private ListSortDirection mSortDirection_ = ListSortDirection.Ascending;
        private PropertyDescriptor mSortProperty_;

        #endregion

        #region Properties

        

        #endregion

        #region Constructors

        public AbstractStoredDataList()
        {
        }

        public AbstractStoredDataList(Session session, Persistence persistence)
        {
            Set(session, persistence);
        }

        #endregion

        #region Private Methods



        #endregion

        #region Public Methods

        public void Save(Session session)
        {
            foreach (T item in Items)
            {
                item.Save(session);
            }
        }
        
        public void Set(Session session, Persistence persistence)
        {
            while (persistence.HasNext)
            {
                T data = new T();
                data.Set(session, persistence);
                Add(data);
            }
        }

        public void FetchAll(Session session)
        {
            Persistence persistence = Persistence.GetInstance(session);
            persistence.ExecuteQuery("SELECT * FROM " + typeof(T).Name);
            Set(session, persistence);
            persistence.Close();
            persistence = null;
        }

        public void AddRange(AbstractStoredDataList<T> list)
        {
            foreach (T item in list)
            {
                Items.Add(item);
            }
        }

        #endregion

        #region Inherited Methods

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
            return lhsValue.ToString().CompareTo(rhsValue.ToString());
        }

        #endregion
    }
}
