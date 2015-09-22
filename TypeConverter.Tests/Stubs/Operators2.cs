using System;

namespace TypeConverter.Tests
{
    public class Operators2
    {
        public static explicit operator bool(Operators2 o)
        {
            return false;
        }

        public static implicit operator Operators2(DerivedOperators o)
        {
            return null;
        }

        public static explicit operator Operators2(int i)
        {
            throw new NotImplementedException();
        }
    }
}