using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using MiskoPersist.Attributes;
using MiskoPersist.Core;

namespace MiskoPersist.Data
{
	[JsonConverter(typeof(PageSerializer))]
    public class Page
    {
        private static Logger Log = Logger.GetInstance(typeof(Page));

        #region Fields

        

        #endregion

        #region Properties

		public Int32 PageNo 
		{
			get;
			set;
		}
		
		public Int32 RowsPerPage 
        { 
        	get;
        	set;
        }
        
        public Boolean IncludeRecordCount 
        { 
        	get;
        	set;
        }
        
        public Int32 TotalRowCount 
		{
			get;
			set;
		}
        
		public Int32 TotalPageCount 
		{
			get
			{
				if(RowsPerPage == 0)
				{
					return 1;
				}
				return TotalRowCount > 0 ? (Int32)Math.Ceiling((decimal)TotalRowCount / (decimal)RowsPerPage) : 0;
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
        		return HasNext ? new Page(PageNo + 1) : new Page(1);
        	}
        }

        #endregion

        #region Constructors

        public Page()
        {
        }

        public Page(Int32 page)
        	: this()
        {
            PageNo = page;
        }

        #endregion

        #region Private Methods



        #endregion

        #region Public Methods

        

        #endregion

        #region Override Methods

        

        #endregion
    }
    
    internal class PageSerializer : JsonConverter
    {
		#region implemented abstract members of JsonConverter
		
		public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
		{
			Page page = value as Page;
			
			writer.WriteStartObject();
			writer.WritePropertyName("PageNo");
    		serializer.Serialize(writer, page.PageNo);
    		
    		if(page.RowsPerPage > 0)
    		{
    			writer.WritePropertyName("RowsPerPage");
    			serializer.Serialize(writer, page.RowsPerPage);
    		}
    		
    		writer.WritePropertyName("IncludeRecordCount");
    		serializer.Serialize(writer, page.IncludeRecordCount);
    		
    		if(page.IncludeRecordCount && page.RowsPerPage > 0)
    		{
    			writer.WritePropertyName("TotalRowCount");
    			serializer.Serialize(writer, page.TotalRowCount);
    		}
	    		
			writer.WriteEndObject();
		}
		
		public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
		{
			JObject jsonObject = JObject.Load(reader);
			
			return new Page{PageNo = (Int32)jsonObject["PageNo"],
							RowsPerPage = ((Int32?)jsonObject["RowsPerPage"]).HasValue ? ((Int32?)jsonObject["RowsPerPage"]).Value : 0,
							IncludeRecordCount = (Boolean)jsonObject["IncludeRecordCount"],
							TotalRowCount = ((Int32?)jsonObject["TotalRowCount"]).HasValue ? ((Int32?)jsonObject["TotalRowCount"]).Value : 0
						   };
		}
		
		public override bool CanConvert(Type objectType)
		{
			throw new NotImplementedException();
		}
		
		#endregion    	
    }
}
