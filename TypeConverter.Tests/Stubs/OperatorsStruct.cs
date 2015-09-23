using System;

namespace TypeConverter.Tests.Stubs
{
    public struct OperatorsStruct
    {
        public static implicit operator string(OperatorsStruct o)
        {
            return "OperatorsStruct";
        }

        public static implicit operator int(OperatorsStruct o)
        {
            return 1;
        }

        public static explicit operator decimal?(OperatorsStruct o)
        {
            return 1.0m;
        }

        public static explicit operator StringSplitOptions(OperatorsStruct o)
        {
            return StringSplitOptions.RemoveEmptyEntries;
        }
    }
}