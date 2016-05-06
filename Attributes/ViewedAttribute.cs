using System;
using log4net;

namespace MiskoPersist.Attributes
{
	public delegate String ViewedSerializerHandler(Object o);
	public delegate Object ViewedDeserializerHandler(String s);
	
	public class ViewedAttribute : Attribute
	{
		private static ILog Log = LogManager.GetLogger(typeof(ViewedAttribute));

		#region Fields

		

		#endregion

		#region Properties

		public String ColumnName
		{
			get;
			set;
		}
		
		public ViewedSerializerHandler ViewedSerializer
		{
			get;
			set;
		}
		
		public ViewedDeserializerHandler ViewedDeserializer
		{
			get;
			set;
		}

		#endregion

		#region Constructors
		
		public ViewedAttribute()
		{
		}

		public ViewedAttribute(Type target, String serializer, String deserializer)
		{
			if (target == null)
			{
				throw new ArgumentNullException("target");
			}
			
			if (String.IsNullOrEmpty(serializer))
			{
				throw new ArgumentNullException("serializer");
			}
			
			if (String.IsNullOrEmpty(deserializer))
			{
				throw new ArgumentNullException("deserializer");
			}
			
			ViewedSerializer = (ViewedSerializerHandler)Delegate.CreateDelegate(typeof(ViewedSerializerHandler), target, serializer);
			ViewedDeserializer = (ViewedDeserializerHandler)Delegate.CreateDelegate(typeof(ViewedDeserializerHandler), target, deserializer);
		}

		#endregion

		#region Private Methods



		#endregion

		#region Public Methods



		#endregion
	}
}
