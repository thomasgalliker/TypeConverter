# TypeConverter 
 TypeConverter is a lightweight, portable class library which allows to convert between objects of different types. This library is shipped with some basic sample converters, however, you are free to write your own type converters and register them in the IConverterRegistry.

### Setup
* This library is available on NuGet: https://www.nuget.org/packages/TypeConverter/
* You can use this library in any .Net project which is compatible to PCL (e.g. Xamarin Android, iOS, Windows Phone, Windows Store, Universal Apps, etc.)

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
* Create (or retrieve via dependency injection) an instance of ConverterRegistry and register those converters you like to use later on. Beware that you will have to register a converter for each direction you want to convert (if you support two-way conversion). Following example shows how to register the StringToUriConverter to convert between string and Uri and vice versa.
```
IConverterRegistry converterRegistry = new ConverterRegistry();
converterRegistry.RegisterConverter<string, Uri>(() => new StringToUriConverter());
converterRegistry.RegisterConverter<Uri, string>(() => new StringToUriConverter());
```
