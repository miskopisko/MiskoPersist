﻿using System;
using System.Reflection;
using log4net;
using MiskoPersist.Attributes;
using MiskoPersist.Core;
using MiskoPersist.Data;
using MiskoPersist.Message.Request;
using MiskoPersist.Message.Response;
using MiskoPersist.Enums;

namespace Message
{
	public class CoreMessage : ViewedData
	{
		private static ILog Log = LogManager.GetLogger(typeof(CoreMessage));

		#region Fields

		private ErrorMessages mConfirmations_ = new ErrorMessages();
        private SerializationType mSerializationType_;

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

        public SerializationType SerializationType
        {
            get
            {
                if(mSerializationType_ == null)
                {
                    throw new ArgumentNullException("mSerializationType_");
                }
                return mSerializationType_;
            }
            set
            {
                mSerializationType_ = value;
            }
        }
		
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
				if (HasConfirmations)
				{
					foreach (ErrorMessage confirmMessage in Confirmations)
					{
						if (confirmMessage.Confirmed.HasValue && !confirmMessage.Confirmed.Value)
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
				if (this is ResponseMessage)
				{
					String msgName = GetType().Name.Substring(0, GetType().Name.Length - 2);
					String msgPath = GetType().FullName.Replace("Responses." + msgName + "RS", "");
					Type requestClass = Type.GetType(Assembly.CreateQualifiedName(GetType().Assembly.FullName, msgPath + "Responses." + msgName + "RQ"));
					if (requestClass == null)
					{
						throw new MiskoException("Could not find wrapper class {0}", msgPath + msgName);
					}
					return requestClass;
				}
				return GetType();
			}
		}
		
		internal Type ResponseClass
		{
			get
			{
				if (this is RequestMessage)
				{
					String msgName = GetType().Name.Substring(0, GetType().Name.Length - 2);
					String msgPath = GetType().FullName.Replace("Requests." + msgName + "RQ", "");
					Type responseClass = Type.GetType(Assembly.CreateQualifiedName(GetType().Assembly.FullName, msgPath + "Responses." + msgName + "RS"));
					if (responseClass == null)
					{
						throw new MiskoException("Could not find wrapper class {0}", msgPath + msgName);
					}
					return responseClass;
				}
				return GetType();
			}
		}
		
		internal Type WrapperClass
		{
			get
			{
				Type wrapperClass;
				String msgName;
				String msgPath;
				if (this is RequestMessage)
				{
					msgName = GetType().Name.Substring(0, GetType().Name.Length - 2);
					msgPath = GetType().FullName.Replace("Requests." + msgName + "RQ", "");
					wrapperClass = Type.GetType(Assembly.CreateQualifiedName(GetType().Assembly.FullName, msgPath + msgName));
				}
				else
				{
					msgName = GetType().Name.Substring(0, GetType().Name.Length - 2);
					msgPath = GetType().FullName.Replace("Responses." + msgName + "RS", "");
					wrapperClass = Type.GetType(Assembly.CreateQualifiedName(GetType().Assembly.FullName, msgPath + msgName));
				}
				if (wrapperClass == null)
				{
					throw new MiskoException("Could not find wrapper class {0}", msgPath + msgName);
				}
				return wrapperClass;
			}
		}

		#endregion
	}
}

