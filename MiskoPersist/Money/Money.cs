﻿using System;
using System.Globalization;
using log4net;

namespace MiskoPersist.MoneyType
{
	public struct Money : IComparable<Money>, IComparable, IEquatable<Money>, IFormattable, IConvertible
	{
		private static ILog Log = LogManager.GetLogger(typeof(Money));

		#region Constants

		public static readonly Money ZERO = new Money(0);
		public static readonly Money ONE = new Money(1);
		public static readonly Money HUNDRED = new Money(100);

		private const Decimal FractionScale = 1E9M;

		#endregion

		#region Fields

		private readonly Int64 mUnits_;
		private readonly Int32 mDecimalFraction_;

		#endregion

		#region Properties

		public Decimal Value
		{
			get
			{
				return computeValue();
			}
		}

		#endregion

		#region Constructors
		
		public Money(String s)
			: this(Decimal.Parse(s))
		{
		}

		public Money(Decimal value) 
			: this()
		{
			checkValue(value);

			mUnits_ = (Int64)value;
			mDecimalFraction_ = (Int32)Decimal.Round((value - mUnits_) * FractionScale);

			if (mDecimalFraction_ >= FractionScale)
			{
				mUnits_ += 1;
				mDecimalFraction_ = mDecimalFraction_ - (Int32)FractionScale;
			}
		}

		private Money(Int64 units, Int32 fraction)
		{
			mUnits_ = units;
			mDecimalFraction_ = fraction;
		}
		
		#endregion

		#region Operators

		public static implicit operator Money(Byte value)
		{
			return new Money(new Decimal(value));
		}

		public static implicit operator Money(SByte value)
		{
			return new Money(new Decimal(value));
		}

		public static implicit operator Money(Single value)
		{
			return new Money((Decimal)value);
		}

		public static implicit operator Money(Double value)
		{
			return new Money((Decimal)value);
		}

		public static implicit operator Money(Decimal value)
		{
			return new Money(value);
		}

		public static implicit operator Money(Int16 value)
		{
			return new Money(new Decimal(value));
		}

		public static implicit operator Money(Int32 value)
		{
			return new Money(new Decimal(value));
		}

		public static implicit operator Money(Int64 value)
		{
			return new Money(new Decimal(value));
		}

		public static implicit operator Money(UInt16 value)
		{
			return new Money(new Decimal(value));
		}

		public static implicit operator Money(UInt32 value)
		{
			return new Money(new Decimal(value));
		}

		public static implicit operator Money(UInt64 value)
		{
			return new Money(new Decimal(value));
		}

		public static Money operator -(Money value)
		{
			return new Money(-value.mUnits_, -value.mDecimalFraction_);
		}

		public static Money operator +(Money value)
		{
			return value;
		}

		public static Money operator +(Money left, Money right)
		{
			var fractionSum = left.mDecimalFraction_ + right.mDecimalFraction_;

			var overflow = 0L;
			var fractionSign = Math.Sign(fractionSum);
			var absFractionSum = Math.Abs(fractionSum);

			if (absFractionSum >= FractionScale)
			{
				overflow = fractionSign;
				absFractionSum -= (Int32)FractionScale;
				fractionSum = fractionSign * absFractionSum;
			}

			var newUnits = left.mUnits_ + right.mUnits_ + overflow;

			if (fractionSign < 0 && Math.Sign(newUnits) > 0)
			{
				newUnits -= 1;
				fractionSum = (Int32)FractionScale - absFractionSum;
			}

			return new Money(newUnits, fractionSum);
		}

		public static Money operator -(Money left, Money right)
		{
			return left + -right;
		}

		public static Money operator *(Money left, Money right)
		{
			return left * right;
		}

		public static Money operator /(Money left, Money right)
		{
			return left / right;
		}

		public static Boolean operator ==(Money left, Money right)
		{
			return left.Equals(right);
		}

		public static Boolean operator !=(Money left, Money right)
		{
			return !(left == right);
		}

		public static Boolean operator >(Money left, Money right)
		{
			return left.CompareTo(right) > 0;
		}

		public static Boolean operator <(Money left, Money right)
		{
			return left.CompareTo(right) < 0;
		}

		public static Boolean operator >=(Money left, Money right)
		{
			return left.CompareTo(right) >= 0;
		}

		public static Boolean operator <=(Money left, Money right)
		{
			return left.CompareTo(right) <= 0;
		}

		#endregion

		#region Public Methods

		public static Boolean TryParse(String s, out Money money)
		{
			money = ZERO;

			if (s == null)
			{
				return false;
			}

			s = s.Trim();

			if (s == String.Empty)
			{
				return false;
			}

			Currency? currency = null;
			Currency currencyValue;

			if (!Currency.TryParse(s.Substring(0, 1), out currencyValue))
			{
				if (s.Length > 2 && Currency.TryParse(s.Substring(0, 3), out currencyValue))
				{
					s = s.Substring(3);
					currency = currencyValue;
				}
			}
			else
			{
				s = s.Substring(1);
				currency = currencyValue;
			}

			Decimal value;

			if (!Decimal.TryParse(s, out value))
			{
				return false;
			}

			money = new Money(value);

			return true;
		}

		public Money Round(RoundingPlaces places)
		{
			return Round(places, MidpointRoundingRule.ToEven);
		}

		public Money Round(RoundingPlaces places, MidpointRoundingRule rounding)
		{
			Money remainder;
			return Round(places, rounding, out remainder);
		}

		public Money Round(RoundingPlaces places, MidpointRoundingRule rounding, out Money remainder)
		{
			Int64 unit;

			var placesExponent = getExponentFromPlaces(places);
			var fraction = roundFraction(placesExponent, rounding, out unit);
			var units = mUnits_ + unit;
			remainder = new Money(0, mDecimalFraction_ - fraction);

			return new Money(units, fraction);
		}
		
		#endregion
		
		#region Private Methods

		private Int32 roundFraction(Int32 exponent, MidpointRoundingRule rounding, out Int64 unit)
		{
			var denominator = FractionScale / (Decimal)Math.Pow(10, exponent);
			var fraction = mDecimalFraction_ / denominator;

			switch (rounding)
			{
				case MidpointRoundingRule.ToEven:
					fraction = Math.Round(fraction, MidpointRounding.ToEven);
					break;
				case MidpointRoundingRule.AwayFromZero:
					{
						var sign = Math.Sign(fraction);
						fraction = Math.Abs(fraction);
						fraction = Math.Floor(fraction + 0.5M);
						fraction *= sign;
						break;
					}
				case MidpointRoundingRule.TowardZero:
					{
						var sign = Math.Sign(fraction);
						fraction = Math.Abs(fraction);
						fraction = Math.Floor(fraction + 0.5M);
						fraction *= sign;
						break;
					}
				case MidpointRoundingRule.Up:
					fraction = Math.Floor(fraction + 0.5M);
					break;
				case MidpointRoundingRule.Down:
					fraction = Math.Ceiling(fraction - 0.5M);
					break;
				default:
					throw new ArgumentOutOfRangeException("rounding");
			}

			fraction *= denominator;

			if (fraction >= FractionScale)
			{
				unit = 1;
				fraction = fraction - (Int32)FractionScale;
			}
			else
			{
				unit = 0;
			}

			return (Int32)fraction;
		}

		private Int32 getExponentFromPlaces(RoundingPlaces places)
		{
			switch (places)
			{
				case RoundingPlaces.Zero:
					return 0;
				case RoundingPlaces.One:
					return 1;
				case RoundingPlaces.Two:
					return 2;
				case RoundingPlaces.Three:
					return 3;
				case RoundingPlaces.Four:
					return 4;
				case RoundingPlaces.Five:
					return 5;
				case RoundingPlaces.Six:
					return 6;
				case RoundingPlaces.Seven:
					return 7;
				case RoundingPlaces.Eight:
					return 8;
				case RoundingPlaces.Nine:
					return 9;
				default:
					throw new ArgumentOutOfRangeException("places");
			}
		}

		private Decimal computeValue()
		{
			return mUnits_ + (mDecimalFraction_ / FractionScale);
		}

		private InvalidOperationException differentCurrencies()
		{
			return new InvalidOperationException("Money values are in different currencies. Convert to the same currency before performing operations on the values.");
		}

		private void checkValue(Decimal value)
		{
			if (value < Int64.MinValue || value > Int64.MaxValue)
			{
				throw new ArgumentOutOfRangeException("value", value, "Money value must be between " + Int64.MinValue + " and " + Int64.MaxValue);
			}
		}

		#endregion

		#region Override Methods

		public override Int32 GetHashCode()
		{
			unchecked
			{
				return 397 * mUnits_.GetHashCode();
			}
		}

		public override Boolean Equals(Object obj)
		{
			if (obj is Money)
			{
				return Equals((Money)obj);
			}
			return false;
		}

		public override String ToString()
		{
			return computeValue().ToString("C", NumberFormatInfo.CurrentInfo);
		}

		#endregion

		#region Implementation of IEquatable<Money>

		public Boolean Equals(Money other)
		{
			return mUnits_ == other.mUnits_ && mDecimalFraction_ == other.mDecimalFraction_;
		}

		#endregion

		#region Implementation of IComparable<Money>

		public Int32 CompareTo(Money other)
		{
			var unitCompare = mUnits_.CompareTo(other.mUnits_);

			return unitCompare == 0 ? mDecimalFraction_.CompareTo(other.mDecimalFraction_) : unitCompare;
		}

		#endregion

		#region Implementation of IComparable

		public Int32 CompareTo(Object other)
		{
			if (other is Money)
			{
				return CompareTo((Money)other);
			}
			throw new InvalidCastException();
		}

		#endregion

		#region Implementation of IFormattable

		public String ToString(String format, IFormatProvider formatProvider)
		{
			return computeValue().ToString(format, formatProvider);
		}

		#endregion

		#region Implementation of IConvertible

		public TypeCode GetTypeCode()
		{
			return TypeCode.Object;
		}

		public Boolean ToBoolean(IFormatProvider provider)
		{
			return mUnits_ == 0 && mDecimalFraction_ == 0;
		}

		public Char ToChar(IFormatProvider provider)
		{
			throw new NotSupportedException();
		}

		public SByte ToSByte(IFormatProvider provider)
		{
			return (SByte)computeValue();
		}

		public Byte ToByte(IFormatProvider provider)
		{
			return (Byte)computeValue();
		}

		public Int16 ToInt16(IFormatProvider provider)
		{
			return (Int16)computeValue();
		}

		public UInt16 ToUInt16(IFormatProvider provider)
		{
			return (UInt16)computeValue();
		}

		public Int32 ToInt32(IFormatProvider provider)
		{
			return (Int32)computeValue();
		}

		public UInt32 ToUInt32(IFormatProvider provider)
		{
			return (UInt32)computeValue();
		}

		public Int64 ToInt64(IFormatProvider provider)
		{
			return (Int64)computeValue();
		}

		public UInt64 ToUInt64(IFormatProvider provider)
		{
			return (UInt64)computeValue();
		}

		public Single ToSingle(IFormatProvider provider)
		{
			return (Single)computeValue();
		}

		public Double ToDouble(IFormatProvider provider)
		{
			return (Double)computeValue();
		}

		public Decimal ToDecimal(IFormatProvider provider)
		{
			return computeValue();
		}

		public DateTime ToDateTime(IFormatProvider provider)
		{
			throw new NotSupportedException();
		}

		public String ToString(IFormatProvider provider)
		{
			return computeValue().ToString(provider);
		}

		public Object ToType(Type conversionType, IFormatProvider provider)
		{
			return null;
		}

		#endregion
	}
}
