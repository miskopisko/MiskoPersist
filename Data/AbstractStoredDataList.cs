using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;
using MiskoPersist.Core;

namespace MiskoPersist.Data
{
    public class AbstractStoredDataList<AbstractStoredData> : BindingList<AbstractStoredData>
    {
        private static Logger Log = Logger.GetInstance(typeof(AbstractStoredDataList<AbstractStoredData>));

        #region Fields

        private bool mIsSorted_;
        private ListSortDirection mSortDirection_ = ListSortDirection.Ascending;
        private PropertyDescriptor mSortProperty_;

        #endregion

        #region Properties

        

        #endregion

        #region Constructors

        public AbstractStoredDataList()
        {
        }

        public AbstractStoredDataList(IList<AbstractStoredData> list) : base(list)
        {
        }

        #endregion

        #region Private Methods



        #endregion

        #region Public Methods

        public AbstractStoredDataList<AbstractStoredData> Save(Session session)
        {
            foreach (AbstractStoredData item in Items)
            {
                typeof(AbstractStoredData).InvokeMember("Save", BindingFlags.InvokeMethod, null, item, new Object[] { session });
            }            

            return this;
        }
        
        public void Set(Session session, Persistence persistence)
        {
            while (persistence.HasNext)
            {
                ConstructorInfo ctor = typeof(AbstractStoredData).GetConstructor(new[] { typeof(Session), typeof(Persistence) });
                AbstractStoredData data = (AbstractStoredData)ctor.Invoke(new object[] { session, persistence });
                Add(data);
            }
        }

        public void FetchAll(Session session)
        {
            Persistence persistence = Persistence.GetInstance(session);
            persistence.ExecuteQuery("SELECT * FROM " + typeof(AbstractStoredData).Name);
            Set(session, persistence);
            persistence.Close();
            persistence = null;
        }

        public void AddRange(AbstractStoredDataList<AbstractStoredData> list)
        {
            foreach (AbstractStoredData item in list)
            {
                Items.Add(item);
            }
        }

        #endregion

        #region Inherited Methods

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

            List<AbstractStoredData> list = Items as List<AbstractStoredData>;
            if (list == null) return;

            list.Sort(Compare);

            mIsSorted_ = true;
            //fire an event that the list has been changed.
            OnListChanged(new ListChangedEventArgs(ListChangedType.Reset, -1));
        }

        private int Compare(AbstractStoredData lhs, AbstractStoredData rhs)
        {
            var result = OnComparison(lhs, rhs);
            //invert if descending
            if (mSortDirection_ == ListSortDirection.Descending)
            {
                result = -result;
            }
            return result;
        }

        private int OnComparison(AbstractStoredData lhs, AbstractStoredData rhs)
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
            return lhsValue.ToString().CompareTo(rhsValue.ToString());
        }

        #endregion
    }
}
