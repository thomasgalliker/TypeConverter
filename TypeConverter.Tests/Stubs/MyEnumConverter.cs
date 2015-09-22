using System;

namespace TypeConverter.Tests.Stubs
{
    public class MyEnumConverter : IConverter<string, MyEnum>, IConverter<MyEnum, string>
    {
        public MyEnum Convert(string value)
        {
            return (MyEnum)Enum.Parse(typeof(MyEnum), value);
        }

        public string Convert(MyEnum value)
        {
            return value.ToString();
        }
    }
}