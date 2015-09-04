# TypeConverter 
 TypeConverter is a lightweight, portable class library which allows to convert between objects of different types. This library is shipped with some basic sample converters, however, you are free to write your own type converters and register them in the IConverterRegistry.

### Setup
* This library is available on NuGet: https://www.nuget.org/packages/TypeConverter/
* Install into your PCL project and Client projects.

### API Usage
#### Create your own type converter
* If you want to implement a type converter, you simple implement the IConverter<TFrom, TTo> interface where TFrom is the generic type from which you want to convert and TTo is the type to which you want to convert to.
* Following sample code illustrates a converter which converts between string and System.Uri.
```
    public class StringToUriConverter : IConverter<string, Uri>, IConverter<Uri, string>
    {
        public Uri Convert(string value)
        {
            return new Uri(value);
        }

        public string Convert(Uri value)
        {
            return value.AbsoluteUri;
        }
    }
```

#### Register a converter
* To be documented
```
IConverterRegistry.RegisterConverter
```

#### Convert between types
* To be documented
```
IConverterRegistry.Convert
```
