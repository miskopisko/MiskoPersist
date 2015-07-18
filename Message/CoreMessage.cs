using System;
using System.IO;
using System.Runtime.Serialization.Formatters;
using MiskoPersist.Core;
using MiskoPersist.Data;
using Newtonsoft.Json;

namespace Message
{
    public class CoreMessage
    {
        private static Logger Log = Logger.GetInstance(typeof(CoreMessage));

        #region Fields


        #endregion

        #region Properties

        public ErrorMessages Confirmations
        {
            get;
            set;
        }

        public Page Page 
        { 
            get;
            set;
        }

        #endregion

        public CoreMessage()
        {
            Confirmations = new ErrorMessages();
        }

        #region Serialization

        public String Serialize()
        {
            StringWriter stringWriter = new StringWriter();
            using(JsonTextWriter writer = new JsonTextWriter(stringWriter))
            {
                JsonSerializer serializer = new JsonSerializer();
                serializer.TypeNameAssemblyFormat = FormatterAssemblyStyle.Simple;
                serializer.TypeNameHandling = TypeNameHandling.Objects;
                serializer.NullValueHandling = NullValueHandling.Ignore;
                serializer.DateFormatString = "yyyy-MM-dd";
                serializer.Formatting = Formatting.Indented;
                serializer.Serialize(writer, this);
                stringWriter.Close();
            }

            return stringWriter.ToString();
        }

        #endregion
    }
}

