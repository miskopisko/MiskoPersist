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

		public Boolean IsNotSet
		{
			get
			{
				return !IsSet;
			}
		}

		#endregion

		#region Constructors

		protected MiskoEnum()
		{
			Value = -1;
		}

		protected MiskoEnum(Int64 v, String code, String description)
		{
			Value = v;
			Code = code;
			Description = description;
		}
		
		#endregion

		#region Override methods
		
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
		
		public virtual Int32 CompareTo(Object e)
		{
			if (e is MiskoEnum)
			{
				if (e == null || ((MiskoEnum)e).Code == null || ((MiskoEnum)e).Code.Trim().Length == 0)
				{
					return 1;
				}
				return String.Compare(Code, ((MiskoEnum)e).Code, StringComparison.Ordinal);
			}
			return -1;
		}
		
		#endregion
	}
}
