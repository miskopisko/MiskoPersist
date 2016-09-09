using System;
using log4net;
using MiskoPersist.Attributes;
using MiskoPersist.Data.Viewed;
using MiskoPersist.Enums;

namespace MiskoPersist.Message.Responses
{
	public class ResponseMessage : CoreMessage
	{
		private static ILog Log = LogManager.GetLogger(typeof(ResponseMessage));

		#region Fields

		

		#endregion

		#region Viewed Properties
		
		[Viewed]
		public ErrorLevel Status
		{
			get;
			set;
		}

		[Viewed]
		public ErrorMessages Errors
		{
			get;
			set;
		}

		[Viewed]
		public ErrorMessages Infos
		{
			get;
			set;
		}

		[Viewed]
		public ErrorMessages Warnings
		{
			get;
			set;
		}

		#endregion

		#region Properties

		public Boolean HasErrors
		{
			get
			{
				return Errors != null && Errors.Count > 0;
			}
		}

		public Boolean HasInfos
		{
			get
			{
				return Infos != null && Infos.Count > 0;
			}
		}
		
		public Boolean HasWarnings
		{
			get
			{
				return Warnings != null && Warnings.Count > 0;
			}
		}
		
		public ErrorMessages ErrorMessages
		{
			set
			{
				Errors = ((ErrorMessages)value).ListOf(ErrorLevel.Error);
				Confirmations = ((ErrorMessages)value).ListOf(ErrorLevel.Confirmation);
				Warnings = ((ErrorMessages)value).ListOf(ErrorLevel.Warning);
				Infos = ((ErrorMessages)value).ListOf(ErrorLevel.Information);
			}
		}

		#endregion
		
		#region Constructors
		
		public ResponseMessage()
		{
			Status = ErrorLevel.Success;
			Errors = new ErrorMessages();
			Infos = new ErrorMessages();
			Warnings = new ErrorMessages();
		}
		
		#endregion
		
		#region Public Static Methods
		
		public static String TimeSpanSerializer(Object value)
		{
			return ((TimeSpan?)value).GetValueOrDefault().ToString();
		}
		
		public static Object TimeSpanDeserializer(String value)
		{
			return !String.IsNullOrEmpty(value) ? TimeSpan.Parse(value) : (TimeSpan?)null;
		}
		
		#endregion
	}
}
