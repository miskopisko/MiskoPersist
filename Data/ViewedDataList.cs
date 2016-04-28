using System;
using System.Collections;
using System.ComponentModel;
using log4net;
using MiskoPersist.Core;

namespace MiskoPersist.Data
{
	public class ViewedDataList : CollectionBase, IBindingList, IComparer
	{
		private static ILog Log = LogManager.GetLogger(typeof(ViewedDataList));
		
		#region Fields
		
		private readonly Type mViewedDataType_;
		private Boolean mIsSorted_;
        private ListSortDirection mSortDirection_ = ListSortDirection.Ascending;
        private PropertyDescriptor mSortProperty_;
        private ListChangedEventHandler mOnListChanged_;
		
		#endregion
		
		#region Properties

        public Type ViewedDataType
        {
        	get
        	{
        		return mViewedDataType_;
        	}
        }
		
		#endregion
		
		#region Constructors
		
		public ViewedDataList(Type itemType)
        {
        	if(!typeof(ViewedData).IsAssignableFrom(itemType))
        	{
        		throw new MiskoException("Cannot add {0} to a ViewedDataList", itemType);
        	}
        	
        	mViewedDataType_ = itemType;
        }
		
		#endregion
		
		#region Public Methods
		
		public void Set(Session session, Persistence persistence)
        {
            Set(session, persistence, new Page());
        }

        public void Set(Session session, Persistence persistence, Page page)
        {
        	if(persistence.IsEof)
        	{
        		page.PageNo = 0;
        		return;
        	}
        	
            Int32 noRows = page.RowsPerPage;
            Int32 pageNo = page.PageNo;

			for (Int32 i = 0; i <(pageNo - 1) * noRows && !persistence.IsEof; i++)
            {
                persistence.Next();
            }

			for (Int32 i = 0; (noRows == 0 || i < noRows) && !persistence.IsEof; i++)
            {
				ViewedData data = (ViewedData)Activator.CreateInstance(ViewedDataType);
                data.Set(session, persistence);
                Add(data);
                persistence.Next();
            }

            if(page.IncludeRecordCount)
            {
                page.TotalRowCount = persistence.RecordCount;
            }
        }

        public void FetchAll(Session session)
        {
            Persistence persistence = Persistence.GetInstance(session);
            persistence.ExecuteQuery("SELECT * FROM " + ViewedDataType.Name);
            Set(session, persistence, new Page());
            persistence.Close();
            persistence = null;
        }
        
        public void Concatenate(ViewedDataList list)
        {
        	if(list != null)
        	{
        		foreach(ViewedData data in list) 
	        	{
	        		this.InnerList.Add(data);
	        	}
	        	FireListChanged(ListChangedType.Reset, -1);	
        	}
        }
        
        public ViewedDataList Clone()
        {
        	return (ViewedDataList)MemberwiseClone();
        }
		
		#endregion
		
		#region Override Methods
		
		public void Add(ViewedData viewedData)
		{
			((IList)this).Add(viewedData);
		}
		
		public void Remove(ViewedData viewedData)
		{
			((IList)this).Remove(viewedData);			
		}
		
		public Boolean Contains(ViewedData viewedData)
		{
			return ((IList)this).Contains(viewedData);
		}
		
		protected override void OnClearComplete()
		{
			base.OnClearComplete();
			FireListChanged(ListChangedType.Reset, -1);
		}
		
		protected override void OnInsertComplete(int index, object value)
		{
			base.OnInsertComplete(index, value);
			FireListChanged(ListChangedType.ItemAdded, index);
		}
		
		protected override void OnRemoveComplete(int index, object value)
		{
			base.OnRemoveComplete(index, value);
			FireListChanged(ListChangedType.ItemDeleted, index);
		}
		
		public override Boolean Equals(Object obj)
		{
			ViewedDataList other = obj as ViewedDataList;				
			return other != null && mViewedDataType_.Equals(other.ViewedDataType) && List.Equals(other.List);			
		}

		public override Int32 GetHashCode()
		{
			int hashCode = 0;
			unchecked
			{
				if(mViewedDataType_ != null)
				{
					hashCode += 1000000007 * mViewedDataType_.GetHashCode();
				}
				
				foreach (ViewedData data in this) 
				{
					hashCode += 1000000033 * data.GetHashCode();
				}
			}
			return hashCode;
		}

		public static bool operator ==(ViewedDataList lhs, ViewedDataList rhs)
		{
			if(ReferenceEquals(lhs, rhs))
			{
				return true;
			}
			if(ReferenceEquals(lhs, null) || ReferenceEquals(rhs, null))
			{
				return false;
			}
			return lhs.Equals(rhs);
		}

		public static bool operator !=(ViewedDataList lhs, ViewedDataList rhs)
		{
			return !(lhs == rhs);
		}

		#endregion
		
		#region Private Methods
		
		private void FireListChanged(ListChangedType type, Int32 index)
        {           
            if(mOnListChanged_ != null) 
            {
                mOnListChanged_(this, new ListChangedEventArgs(type, index));
            }
        }

        private Int32 OnComparison(ViewedData lhs, ViewedData rhs)
        {
            Object lhsValue = lhs == null ? null : mSortProperty_.GetValue(lhs);
            Object rhsValue = rhs == null ? null : mSortProperty_.GetValue(rhs);
            
            if(lhsValue == null)
            {
                return (rhsValue == null) ? 0 : -1; //nulls are equal
            }
            
            if(rhsValue == null)
            {
                return 1; //first has value, second doesn't
            }
            
            if(lhsValue is IComparable)
            {
                return ((IComparable)lhsValue).CompareTo(rhsValue);
            }
            
			return lhsValue.Equals(rhsValue) ? 0 : String.Compare(lhsValue.ToString(), rhsValue.ToString(), StringComparison.Ordinal);
        }

		#endregion
		
		#region IBindingList implementation
		
		public event ListChangedEventHandler ListChanged
		{
            add 
            {
                mOnListChanged_ = (ListChangedEventHandler)Delegate.Combine(mOnListChanged_, value);
            }
            remove 
            {
                mOnListChanged_ = (ListChangedEventHandler)Delegate.Remove(mOnListChanged_, value);
            }
        }        

		public Object AddNew()
		{
			ViewedData viewedData = (ViewedData)Activator.CreateInstance(mViewedDataType_);
			Add(viewedData);
			return viewedData;
		}

		public void AddIndex(PropertyDescriptor property)
		{
			throw new NotImplementedException();
		}

		public void ApplySort(PropertyDescriptor property, ListSortDirection direction)
		{
			mSortProperty_ = property;
            mSortDirection_ = direction;
            
            InnerList.Sort(this);
            
            mIsSorted_ = true;
            FireListChanged(ListChangedType.Reset, -1);
		}

		public Int32 Find(PropertyDescriptor property, Object key)
		{
			throw new NotImplementedException();
		}

		public void RemoveIndex(PropertyDescriptor property)
		{
			throw new NotImplementedException();
		}

		public void RemoveSort()
		{
			mSortDirection_ = ListSortDirection.Ascending;
            mSortProperty_ = null;
		}

		public Boolean AllowNew
		{
			get
			{
				return true;
			}
		}

		public Boolean AllowEdit
		{
			get
			{
				return true;
			}
		}

		public Boolean AllowRemove
		{
			get
			{
				return true;
			}
		}

		public Boolean SupportsChangeNotification
		{
			get
			{
				return true;
			}
		}

		public Boolean SupportsSearching
		{
			get
			{
				return false;
			}
		}

		public Boolean SupportsSorting
		{
			get
			{
				return true;
			}
		}

		public Boolean IsSorted
		{
			get
			{
				return mIsSorted_;
			}
		}

		public PropertyDescriptor SortProperty
		{
			get
			{
				return mSortProperty_;
			}
		}

		public ListSortDirection SortDirection
		{
			get
			{
				return mSortDirection_;
			}
		}
		
		#endregion

		#region IComparer implementation

		public Int32 Compare(Object x, Object y)
		{
			Int32 result = OnComparison((ViewedData)x, (ViewedData)y);

            if(mSortDirection_ == ListSortDirection.Descending)
            {
                result = -result;
            }
                
            return result;
		}

		#endregion
	}
}
