using System;
using log4net;
using MiskoPersist.Attributes;
using MiskoPersist.Core;

namespace MiskoPersist.Data
{
	public class Page
    {
        private static ILog Log = LogManager.GetLogger(typeof(Page));

        #region Fields

        

        #endregion

        #region Properties

        [Viewed]
		public Int32 PageNo 
		{
			get;
			set;
		}

		[Viewed]
		public Int32 RowsPerPage 
        { 
        	get;
        	set;
        }

		[Viewed]
		public Boolean IncludeRecordCount 
        { 
        	get;
        	set;
        }

		[Viewed]
		public Int32 TotalRowCount 
		{
			get;
			set;
		}
        
		public Int32 TotalPageCount 
		{
			get
			{
				if(TotalRowCount > 0 && RowsPerPage == 0)
				{
					return 1;
				}
				return TotalRowCount > 0 ? (Int32)Math.Ceiling((Decimal)TotalRowCount / (Decimal)RowsPerPage) : 0;
			}
		}		
        
        public Boolean HasNext 
        { 
        	get 
        	{ 
        		return PageNo < TotalPageCount; 
        	}
        }
        
        public Page Next 
        { 
        	get 
        	{ 
        		return new Page(HasNext ? PageNo + 1 : 1) { RowsPerPage = RowsPerPage, IncludeRecordCount = IncludeRecordCount };
        	}
        }

        #endregion

        #region Constructors
        
        public Page() : this(1)
        {        	
        }

        public Page(Int32 page)
        {
            PageNo = page;
            RowsPerPage = 0;
            IncludeRecordCount = false;
            TotalRowCount = 0;
        }

        #endregion

        #region Private Methods



        #endregion

        #region Public Methods

        

        #endregion

        #region Override Methods

        

        #endregion
    }
}
