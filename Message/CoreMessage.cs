using System;
using System.Reflection;
using log4net;
using MiskoPersist.Attributes;
using MiskoPersist.Data;
using MiskoPersist.Message.Request;
using MiskoPersist.Message.Response;

namespace Message
{
	public class CoreMessage : ViewedData
	{
		private static ILog Log = LogManager.GetLogger(typeof(CoreMessage));

		#region Fields

		private ErrorMessages mConfirmations_ = new ErrorMessages();

		#endregion
		
		#region Viewed Properties
		
		[Viewed]
		public ErrorMessages Confirmations
		{
			get
			{
				return mConfirmations_;
			}
			set
			{
				mConfirmations_ = value;
			}
		}
		
		#endregion

		#region Properties		
		
		public Boolean HasConfirmations 
		{ 
			get 
			{ 
				return Confirmations != null && Confirmations.Count > 0; 
			} 
		}

		public Boolean HasUnconfirmed 
		{
			get
			{
				Boolean hasUnconfirmed = false;
				if(HasConfirmations)
				{
					foreach (ErrorMessage confirmMessage in Confirmations) 
					{
						if(confirmMessage.Confirmed.HasValue && !confirmMessage.Confirmed.Value)
						{
							hasUnconfirmed = true;
							break;
						}
					}
				}
				return hasUnconfirmed;
			}
		}
		
		internal Type RequestClass
		{
			get
			{
				if(this is ResponseMessage)
				{
					String msgName = GetType().Name.Substring(0, GetType().Name.Length - 2);
					String msgPath = GetType().FullName.Replace("Responses." + msgName + "RS", "");
					return Type.GetType(Assembly.CreateQualifiedName(GetType().Assembly.FullName, msgPath + "Responses." + msgName + "RQ"));
				}
				return GetType();
			}
		}
		
		internal Type ResponseClass
		{
			get
			{
				if(this is RequestMessage)
				{
					String msgName = GetType().Name.Substring(0, GetType().Name.Length - 2);
					String msgPath = GetType().FullName.Replace("Requests." + msgName + "RQ", "");
					return Type.GetType(Assembly.CreateQualifiedName(GetType().Assembly.FullName, msgPath + "Responses." + msgName + "RS"));
				}
				return GetType();
			}
		}
		
		internal Type WrapperClass
		{
			get
			{
				if(this is RequestMessage)
				{
					String msgName = GetType().Name.Substring(0, GetType().Name.Length - 2);
					String msgPath = GetType().FullName.Replace("Requests." + msgName + "RQ", "");
					return Type.GetType(Assembly.CreateQualifiedName(GetType().Assembly.FullName, msgPath + msgName));
				}
				else
				{
					String msgName = GetType().Name.Substring(0, GetType().Name.Length - 2);
					String msgPath = GetType().FullName.Replace("Responses." + msgName + "RS", "");
					return Type.GetType(Assembly.CreateQualifiedName(GetType().Assembly.FullName, msgPath + msgName));
				}
			}
		}

		#endregion
	}
}

