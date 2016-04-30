using System;

namespace MiskoPersist.Enums
{
	public class SerializationType : MiskoEnum
    {
        #region Fields

        private static readonly SerializationType mNULL_ = new SerializationType(-1, "", "");
        private static readonly SerializationType mXml_ = new SerializationType(0, "", "Xml");
        private static readonly SerializationType mJson_ = new SerializationType(1, "", "Json");

        private static readonly SerializationType[] mElements_ = new[]
		{
		    mNULL_, mXml_, mJson_
		};

        #endregion

        #region Parameters

        public static SerializationType[] Elements { get { return mElements_; } }
        public static SerializationType NULL { get { return mNULL_; } }
        public static SerializationType Xml { get { return mXml_; } }
        public static SerializationType Json { get { return mJson_; } }

        #endregion

        #region Constructors

        public SerializationType()
        {
        }

        public SerializationType(Int64 value, String code, String description) : base(value, code, description)
        {
        }

        #endregion

        #region Helpers

        public static SerializationType GetElement(long index)
        {
            for (Int32 i = 0; Elements != null && i < Elements.Length; i++)
            {
                if(Elements[i].Value == index)
                {
                    return Elements[i];
                }
            }

            return null;
        }

        public static SerializationType GetElement(String descriptionCode)
        {
            for (Int32 i = 0; descriptionCode != null && Elements != null && i < Elements.Length; i++)
            {
                if(Elements[i].Description.ToLower().Equals(descriptionCode.ToLower()) || Elements[i].Code.ToLower().Equals(descriptionCode.ToLower()))
                {
                    return Elements[i];
                }
            }

            return null;
        }
        
        public String ToHttpContentType()
        {
        	return this.Equals(Json) ? "text/json" : "text/xml";
        }

        public static SerializationType FromHttpContentType(String contentType)
        {
        	return "text/json".Equals(contentType) ? SerializationType.Json : SerializationType.Xml;
        }
        
        #endregion
    }
}
