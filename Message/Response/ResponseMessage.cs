using System;
using log4net;
using Message;
using MiskoPersist.Attributes;
using MiskoPersist.Data;
using MiskoPersist.Enums;

namespace MiskoPersist.Message.Response
{
	public class ResponseMessage : CoreMessage
	{
		private static ILog Log = LogManager.GetLogger(typeof(ResponseMessage));

		#region Fields

		private ErrorLevel mStatus_ = ErrorLevel.Success;
		private ErrorMessages mErrors_ = new ErrorMessages();
		private ErrorMessages mInfos_ = new ErrorMessages();
		private ErrorMessages mWarnings_ = new ErrorMessages();

		#endregion

		#region Viewed Properties
		
		[Viewed]
		public ErrorLevel Status
		{
			get
			{
				return mStatus_;
			}
			set
			{
				mStatus_ = value;
			}
		}

		[Viewed]
		public ErrorMessages Errors
		{
			get
			{
				return mErrors_;
			}
			set
			{
				mErrors_ = value;
			}
		}

		[Viewed]
		public ErrorMessages Infos
		{
			get
			{
				return mInfos_;
			}
			set
			{
				mInfos_ = value;
			}
		}

		[Viewed]
		public ErrorMessages Warnings
		{
			get
			{
				return mWarnings_;
			}
			set
			{
				mWarnings_ = value;
			}
		}
		
		[Viewed(typeof(ResponseMessage), "TimeSpanSerializer", "TimeSpanDeserializer")]
		public TimeSpan? MessageExecutionTime
		{
			get;
			set;
		}

		[Viewed(typeof(ResponseMessage), "TimeSpanSerializer", "TimeSpanDeserializer")]
		public TimeSpan? SqlExecutionTime
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
			get
			{
				ErrorMessages result = new ErrorMessages();
				result.Concatenate(Errors);
				result.Concatenate(Confirmations);
				result.Concatenate(Warnings);
				result.Concatenate(Infos);
				return result;
			}
			set
			{
				Errors = ((ErrorMessages)value).ListOf(ErrorLevel.Error);
				Confirmations = ((ErrorMessages)value).ListOf(ErrorLevel.Confirmation);
				Warnings = ((ErrorMessages)value).ListOf(ErrorLevel.Warning);
				Infos = ((ErrorMessages)value).ListOf(ErrorLevel.Information);
			}
		}

		#endregion
		
		#region Public Static Methods
		
		public static String TimeSpanSerializer(Object value)
		{
			return ((TimeSpan?)value).GetValueOrDefault().ToString();
		}
		
		public static Object TimeSpanDeserializer(String value)
		{
			return TimeSpan.Parse(value);
		}
		
		#endregion
	}
}
