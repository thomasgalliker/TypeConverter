
using System;

namespace TypeConverter.Tests.Stubs
{
    public class Operators2 : IEquatable<Operators2>
    {
        public int Value { get { return 999; } }

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
            return new Operators2();
        }

        public override bool Equals(object obj)
        {
            return this.Equals(obj as Operators2);
        }

        public override int GetHashCode()
        {
            return this.Value.GetHashCode();
        }

        public bool Equals(Operators2 other)
        {
            return other != null && this.Value == other.Value;
        }
    }
}