using System;
using System.Collections.Generic;
using System.ComponentModel;
using log4net;
using MiskoPersist.Core;

namespace MiskoPersist.Data.Viewed
{
	public class ViewedDataList<T> : BindingList<T>, IComparer<T> where T : ViewedData, new()
	{
		private static ILog Log = LogManager.GetLogger(typeof(ViewedDataList<T>));
		
		#region Fields
		
		private ListSortDirection mSortDirection_ = ListSortDirection.Ascending;
        private PropertyDescriptor mSortProperty_;
		private Boolean mIsSorted_ = false;
		
		#endregion
		
		#region Properties
		
		protected override Boolean SupportsSortingCore
		{
			get
			{
				return true;
			}
		}
		
		protected override ListSortDirection SortDirectionCore
		{
			get
			{
				return mSortDirection_;
			}
		}
		
		protected override PropertyDescriptor SortPropertyCore
		{
			get
			{
				return mSortProperty_;
			}
		}
		
		protected override Boolean IsSortedCore
		{
			get
			{
				return mIsSorted_;
			}
		}
		
		#endregion
		
		#region Constructors
		
		public ViewedDataList()
		{
		}
		
		public ViewedDataList(Session session, Persistence persistence)
		{
			Set(session, persistence);
		}
		
		
		#endregion
		
		#region Override Methods
		
		protected override void ApplySortCore(PropertyDescriptor prop, ListSortDirection direction)
		{
			mSortProperty_ = prop;
            mSortDirection_ = direction;
            
			List<T> list = Items as List<T>;
            if (list == null)
            {
            	return;
            }

            list.Sort(this);
            
			mIsSorted_ = true;
            
			OnListChanged(new ListChangedEventArgs(ListChangedType.Reset, -1));
		}
		
		protected override void RemoveSortCore()
		{
			mSortDirection_ = ListSortDirection.Ascending;
            mSortProperty_ = null;
            mIsSorted_ = false;
		}
		
		#endregion
		
		#region Public Methods
		
		public void Set(Session session, Persistence persistence)
        {
            Set(session, persistence, new Page());
        }

        public void Set(Session session, Persistence persistence, Page page)
        {
            if (persistence.IsEof)
            {
                page.PageNo = 0;
                return;
            }
            
            Int32 noRows = page.RowsPerPage;
            Int32 pageNo = page.PageNo;

            for (Int32 i = 0; i < (pageNo - 1) * noRows && !persistence.IsEof; i++)
            {
                persistence.Next();
            }

            for (Int32 i = 0; (noRows == 0 || i < noRows) && !persistence.IsEof; i++)
            {
                T data = new T();
                data.Set(session, persistence);
                Add(data);
                persistence.Next();
            }

            if (page.IncludeRecordCount)
            {
                page.TotalRowCount = persistence.RecordCount;
            }
        }
		
		public void Add(ViewedDataList<T> list)
        {
            if (list != null)
            {
                foreach (T data in list)
                {
                    Add(data);
                }
                OnListChanged(new ListChangedEventArgs(ListChangedType.Reset, -1));
            }
        }
		
		public ViewedDataList<T> Clone()
        {
            return (ViewedDataList<T>)MemberwiseClone();
        }
		
		#endregion
		
		#region Private Methods
		
		private Int32 OnComparison(T lhs, T rhs)
        {
            Object lhsValue = lhs == null ? null : SortPropertyCore.GetValue(lhs);
            Object rhsValue = rhs == null ? null : SortPropertyCore.GetValue(rhs);
            
            if (lhsValue == null)
            {
                return (rhsValue == null) ? 0 : -1; // nulls are equal
            }
            
            if (rhsValue == null)
            {
                return 1; // first has value, second doesn't
            }
            
            if (lhsValue is IComparable)
            {
                return ((IComparable)lhsValue).CompareTo(rhsValue);
            }
            
            return lhsValue.Equals(rhsValue) ? 0 : String.Compare(lhsValue.ToString(), rhsValue.ToString(), StringComparison.Ordinal);
        }
		
		#endregion
		
		#region IComparer Implementation

		public Int32 Compare(T x, T y)
		{
			Int32 result = OnComparison((T)x, (T)y);

            if (SortDirectionCore == ListSortDirection.Descending)
            {
                result = -result;
            }
            
            return result;
		}
	
		#endregion
	}
}
