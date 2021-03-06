﻿using System;
using System.Reflection;
using log4net;
using MiskoPersist.Attributes;
using MiskoPersist.Core;
using MiskoPersist.Data.Viewed;
using MiskoPersist.Message.Requests;
using MiskoPersist.Message.Responses;

namespace MiskoPersist.Message
{
	public class CoreMessage : ViewedData
	{
		private static ILog Log = LogManager.GetLogger(typeof(CoreMessage));

		#region Fields

		

		#endregion
		
		#region Viewed Properties
		
		[Viewed]
		public Guid? SessionToken
		{
			get;
			set;
		}
		
		[Viewed]
		public ErrorMessages Confirmations
		{
			get;
			set;
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
				String msgName = GetType().Name.Substring(0, GetType().Name.Length - 2);
				if (this is RequestMessage)
				{
					String msgPath = GetType().FullName.Replace("Requests." + msgName + "RQ", "");
					return Type.GetType(Assembly.CreateQualifiedName(GetType().Assembly.FullName, msgPath + msgName));
				}
				if (this is ResponseMessage)
				{
					String msgPath = GetType().FullName.Replace("Responses." + msgName + "RS", "");
					return Type.GetType(Assembly.CreateQualifiedName(GetType().Assembly.FullName, msgPath + msgName));
				}
				return GetType();
			}
		}

		#endregion
		
		#region Constructors
		
		public CoreMessage()
		{
			Confirmations = new ErrorMessages();
		}
		
		#endregion
	}
}

