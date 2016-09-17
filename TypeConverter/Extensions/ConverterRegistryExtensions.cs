namespace TypeConverter.Extensions
{
    public static class ConverterRegistryExtensions
    {
        public static void RegisterConverters(this IConverterRegistry converterRegistry, params IConvertable[] converters)
        {
            foreach (var converter in converters)
            {
                converterRegistry.RegisterConverter(converter);
            }
        }
    }
}
