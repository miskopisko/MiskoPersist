using System;

namespace MiskoPersist.Enums
{
	public class MiskoEnum : IComparable
	{
		#region Properties
		
		public Int64 Value
		{
			get;
			set;
		}

		public String Code
		{
			get;
			set;
		}

		public String Description
		{
			get;
			set;
		}

		public Boolean IsSet
		{
			get
			{
				return Value != -1;
			}
		}

		#endregion

		#region Constructors

		public MiskoEnum()
		{
			Value = -1;
		}

		public MiskoEnum(Int64 v, String code, String description)
		{
			Value = v;
			Code = code;
			Description = description;
		}
		
		#endregion
		
		#region Public Static Methods
		
		public Boolean InArray(MiskoEnum[] list)
		{
			foreach (MiskoEnum element in list) 
			{
				if (element.Equals(this))
				{
					return true;
				}
			}
			
			return false;
		}
		
        public static T Parse<T>(Int64 value) where T : MiskoEnum
		{			
			T result = null;
			if (!TryParse<T>(value, out result))
			{
				throw new FormatException(String.Format("Invalid {0} : {1}", typeof(T).Name, value));
			}
			return result;
		}
		
		public static T Parse<T>(String value) where T : MiskoEnum
		{			
			T result = null;
			if (!TryParse<T>(value, out result))
			{
				throw new FormatException(String.Format("Invalid {0} : {1}", typeof(T).Name, value));
			}
			return result;
		}
		
		public static Boolean TryParse<T>(Int64 value, out T result) where T : MiskoEnum
		{
            result = default(T);
		
            T[] elements = (T[])typeof(T).GetProperty("Elements").GetValue(null);
		
            for (Int32 i = 0; elements != null && i < elements.Length; i++)
            {
                if (elements[i].Value == value)
                {
                    result = elements[i];
                }
            }
			
			return result != null && result.IsSet;
		}
		
		public static Boolean TryParse<T>(String value, out T result) where T : MiskoEnum
		{
            result = default(T);

            T[] elements = (T[])typeof(T).GetProperty("Elements").GetValue(null);

            for (Int32 i = 0; value != null && elements != null && i < elements.Length; i++)
            {
                if (elements[i].Description.ToLower().Equals(value.ToLower()) || elements[i].Code.ToLower().Equals(value.ToLower()))
                {
                    result = elements[i];
                }
            }
			
			return result != null && result.IsSet;
		}
		
		#endregion

		#region Override Methods
		
		public override String ToString()
		{
			return Description;
		}
		
		public override Int32 GetHashCode()
		{
			return base.GetHashCode();
		}

		public override Boolean Equals(Object obj)
		{
			MiskoEnum o = obj as MiskoEnum;

			return o != null && o.GetType() == GetType() && o.Value == Value;
		}
		
		#endregion
		
		#region IComparable Implementation
		
		public virtual Int32 CompareTo(Object obj)
		{
			MiskoEnum o = obj as MiskoEnum;
			if (o != null)
			{
				if (obj == null || o.Code == null || o.Code.Trim().Length == 0)
				{
					return 1;
				}
				return String.Compare(Code, o.Code, StringComparison.Ordinal);
			}
			return -1;
		}
		
		#endregion
	}
}
