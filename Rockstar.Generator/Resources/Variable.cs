using System;

namespace Rockstar
{
    public class Variable
    {
        private object _value;

        private bool _isString;
        private bool _isNumeric;
        private bool _isBool;
        private bool _isNull;
        private bool _isArray;
        private bool _isMysterious;

        public static Variable Mysterious => new Variable(0) { _isMysterious = true };
        public static Variable Null => new Variable(0) { _isNull = true };
        public static Variable Array => new Variable(new Variable[0]) { _isArray = true };

        public Variable(object value)
        {
            _value = value;
            _isNull = value is null;
            _isString = value is string;
            _isNumeric = value is uint ||
                         value is int ||
                         value is ushort ||
                         value is short ||
                         value is sbyte ||
                         value is byte ||
                         value is float ||
                         value is double ||
                         value is decimal ||
                         value is long ||
                         value is ulong;
            _isBool = value is bool;
            _isArray = value is Variable[];
        }

        public void Set(object newValue)
        {
            _value = newValue;
        }

        public dynamic Get() => _value;

        public void Set(Variable newValue, int index)
        {
            if (!_isArray) throw new InvalidOperationException("Trying to set a non-array as if it was an array");
            var arr = (Variable[])_value;
            if (arr.Length <= index)
            {
                System.Array.Resize(ref arr, index);
            }
            arr[index] = newValue;
        }

        public dynamic Get(int index)
        {
            if (!_isArray) throw new InvalidOperationException("Trying to get a non-array as if it was an array");
            var arr = (Variable[])_value;
            if (arr.Length < index)
            {
                return arr[index];
            }
            return Variable.Mysterious;
        }

        private double AsNumeric()
        {
            if (_isNull)
            {
                return 0;
            }
            else if (_isString)
            {
                return double.TryParse(_value as string, out var d) ? d : double.NaN;
            }
            else if (_isNumeric)
            {
                return Convert.ToDouble(_value);
            }
            else if (_isBool)
            {
                return (bool)_value ? 1 : 0;
            }
            else if (_isArray)
            {
                return ((Variable[])_value).Length;
            }
            throw new InvalidOperationException();
        }
        private bool AsBoolean()
        {
            if (_isString)
            {
                var str = (_value as string);
                return !string.IsNullOrEmpty(str);
            }
            else if (_isNumeric)
            {
                if (_value is byte b) return b != 0;
                if (_value is sbyte sb) return sb != 0;
                if (_value is short s) return s != 0;
                if (_value is ushort us) return us != 0;
                if (_value is int i) return i != 0;
                if (_value is long l) return l != 0;
                if (_value is uint ui) return ui != 0;
                if (_value is ulong ul) return ul != 0;
                if (_value is float f) return f != 0;
                return (double)_value != 0;
            }
            else if (_isBool)
            {
                return (bool)_value;
            }
            else if (_isNumeric) return false;
            else if (_isMysterious) return false;
            throw new InvalidOperationException();
        }

        #region ImplicitConversions

        //public static implicit operator bool(Variable a)
        //{
        //    if (a._isNull) return false;

        //    if(a._value is string str)
        //    {
        //        str = str.ToLowerInvariant();
        //        switch(str){
        //            case "true":
        //            case "yes":
        //            case "ok": return true;
        //            case "wrong":
        //            case "no":
        //            case "lies": return false;
        //            default: throw new Exception($"Invalid boolean conversion for value {a._value}");
        //    }
        //}

        public static implicit operator Variable(double a)
        {
            return new Variable(a);
        }

        public static implicit operator Variable(sbyte a)
        {
            return new Variable(a);
        }

        public static implicit operator Variable(byte a)
        {
            return new Variable(a);
        }

        public static implicit operator Variable(short a)
        {
            return new Variable(a);
        }

        public static implicit operator Variable(ushort a)
        {
            return new Variable(a);
        }

        public static implicit operator Variable(int a)
        {
            return new Variable(a);
        }

        public static implicit operator Variable(uint a)
        {
            return new Variable(a);
        }

        public static implicit operator Variable(long a)
        {
            return new Variable(a);
        }

        public static implicit operator Variable(ulong a)
        {
            return new Variable(a);
        }

        public static implicit operator Variable(decimal a)
        {
            return new Variable(a);
        }

        public static implicit operator Variable(string a)
        {
            return new Variable(a);
        }
        #endregion

        ////public static Variable operator <<(Variable a, int b)
        ////{
        ////    Variable c;
        ////    UVariable.LeftShift(out c._value, ref a._value, b);
        ////    return c;
        ////}

        ////public static Variable operator >>(Variable a, int b)
        ////{
        ////    Variable c;
        ////    UVariable.ArithmeticRightShift(out c._value, ref a._value, b);
        ////    return c;
        ////}

        //public static Variable operator &(Variable a, Variable b)
        //{
        //    Variable c;
        //    UVariable.And(out c._value, ref a._value, ref b._value);
        //    return c;
        //}

        //public static Variable operator |(Variable a, Variable b)
        //{
        //    Variable c;
        //    UVariable.Or(out c._value, ref a._value, ref b._value);
        //    return c;
        //}

        //public static Variable operator ^(Variable a, Variable b)
        //{
        //    Variable c;
        //    UVariable.ExclusiveOr(out c._value, ref a._value, ref b._value);
        //    return c;
        //}

        //public static Variable operator ~(Variable a)
        //{
        //    Variable c;
        //    UVariable.Not(out c._value, ref a._value);
        //    return c;
        //}

        public static Variable operator +(Variable a, Variable b)
        {
            return new Variable(a.AsNumeric() + b.AsNumeric());
        }

        public static Variable operator -(Variable a, Variable b)
        {
            return new Variable(a.AsNumeric() - b.AsNumeric());
        }

        //public static Variable operator ++(Variable a)
        //{
        //    Variable c;
        //    UVariable.Add(out c._value, ref a._value, 1);
        //    return c;
        //}

        //public static Variable operator +(Variable a)
        //{
        //    return a;
        //}

        //public static Variable operator -(Variable a)
        //{
        //    Variable c;
        //    UVariable.Negate(out c._value, ref a._value);
        //    return c;
        //}

        //public static Variable operator --(Variable a)
        //{
        //    Variable c;
        //    UVariable.Subtract(out c._value, ref a._value, 1);
        //    return c;
        //}

        //public static Variable operator *(Variable a, int b)
        //{
        //    Variable c;
        //    Multiply(out c, ref a, b);
        //    return c;
        //}

        //public static Variable operator *(int a, Variable b)
        //{
        //    Variable c;
        //    Multiply(out c, ref b, a);
        //    return c;
        //}
        //public static Variable operator *(Variable a, Variable b)
        //{
        //    Variable c;
        //    Multiply(out c, ref a, ref b);
        //    return c;
        //}

        //public static Variable operator /(Variable a, Variable b)
        //{
        //    Variable c;
        //    Divide(out c, ref a, ref b);
        //    return c;
        //}

        //public static Variable operator %(Variable a, Variable b)
        //{
        //    Variable c;
        //    Remainder(out c, ref a, ref b);
        //    return c;
        //}

        public static bool operator <(Variable a, Variable b)
        {
            return LessThan(ref a, ref b);
        }

        public static bool operator <=(Variable a, Variable b)
        {
            return !LessThan(ref b, ref a);
        }

        public static bool operator >(Variable a, Variable b)
        {
            return LessThan(ref b, ref a);
        }

        public static bool operator >=(Variable a, Variable b)
        {
            return !LessThan(ref a, ref b);
        }

        public static bool operator ==(Variable a, Variable b)
        {
            return a.Equals(b);
        }

        public static bool operator !=(Variable a, Variable b)
        {
            return !a.Equals(b);
        }

        public int CompareTo(Variable other)
        {
            return AsNumeric().CompareTo(other.AsNumeric());
        }

        public int CompareTo(object obj)
        {
            if (obj == null)
                return 1;
            if (!(obj is Variable))
                throw new ArgumentException();
            return CompareTo((Variable)obj);
        }

        private static bool LessThan(ref Variable a, ref Variable b)
        {
            return a.AsNumeric() < b.AsNumeric();
        }

        public bool Equals(Variable other)
        {
            var a = this; var b = other;

            if (a._isMysterious && a._isMysterious) return true;
            if (a._isMysterious || b._isMysterious) return false;

            if (a._isString)
            {
                if (b._isNull)    return false;
                if (b._isBool)    return a.AsBoolean() == (bool)b._value;
                if (b._isNumeric) return a.AsNumeric() == b.AsNumeric();
                if (b._isString)  return (string)a._value == (string)b._value;
            }

            if (a._isNumeric)
            {
                if (b._isNull)    return a.AsNumeric() == 0;
                if (b._isBool)    return (bool)b._value == a.AsBoolean();
                if (b._isNumeric) return a.AsNumeric() == b.AsNumeric();
                if (b._isString)  return a.AsNumeric() == b.AsNumeric();
            }

            if (a._isBool)
            {
                if (b._isNull)    return (bool)a._value == false;
                if (b._isBool)    return (bool)a._value == (bool)b._value;
                if (b._isNumeric) return (bool)a._value == b.AsBoolean();
                if (b._isString)  return (bool)a._value == b.AsBoolean();
            }

            return _value.Equals(other._value);
        }

        public override bool Equals(object obj)
        {
            if (!(obj is Variable))
                return false;
            return Equals((Variable)obj);
        }

        public override int GetHashCode()
        {
            return AsNumeric().GetHashCode();
        }

        public override string ToString()
        {
            return _value.ToString();
        }
    }
}