using System;
using log4net;
using MiskoPersist.Attributes;

namespace MiskoPersist.Data.Viewed
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
				if (TotalRowCount > 0 && RowsPerPage == 0)
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
				return new Page(HasNext ? PageNo + 1 : 1, RowsPerPage, IncludeRecordCount, TotalRowCount);
			}
		}

		#endregion

		#region Constructors
		
		public Page()
			: this(1)
		{
		}
		
		public Page(Int32 page = 1, Int32 rowsPerPage = 0, Boolean includeRecordCount = false, Int32 totalRowCount = 0)
		{
			PageNo = page;
			RowsPerPage = rowsPerPage;
			IncludeRecordCount = includeRecordCount;
			TotalRowCount = totalRowCount;
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
