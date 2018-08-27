# TypeConverter 
<img src="https://raw.githubusercontent.com/thomasgalliker/TypeConverter/master/TypeConverter.NuGet/TypeConverterIcon.png" alt="TypeConverter" align="right">
TypeConverter is a lightweight, portable class library which allows to convert between objects of different types. The philosophy behind TypeConverter is that type conversion should no longer be a painfull topic to developers. As a developer, you simply specify source and target type and pass in your desired object which you want to convert.

This library is shipped with some basic sample conversion strategies, however, you are free to write your own type converters and register them in the IConverterRegistry. The most important type conversions provided by the .Net framework are integrated into TypeConverter. However, your own converters are always preferred over the .Net integrated default converstion/casting strategy. Following order of priority is respected:
- Attempt 1: Try to convert using registered, user-defined IConverters
- Attempt 2: Return source value if source and target type are the same
- Attempt 3: Try to cast implicitly to the target type
- Attempt 4: Try to cast explicitly to the target type
- Attempt 5: Try to convert between string and enum value if either source or target type is an enum resp. a string
- Attempt 6: Try to use String.Parse if either source or target type is a string

If all attempts fail, the Convert method throws a ConversionNotSupportedException with the specified reason. TryConvert does return null if no conversion could be done.

### Download and Install TypeConverter
This library is available on NuGet: https://www.nuget.org/packages/TypeConverter/
Use the following command to install TypeConverter using NuGet package manager console:

    PM> Install-Package TypeConverter

You can use this library in any .Net project which is compatible to PCL (e.g. Xamarin Android, iOS, Windows Phone, Windows Store, Universal Apps, etc.)

### API Usage
#### Create your own type converter
If you want to implement a type converter, you simply implement the IConverter<TFrom, TTo> interface where TFrom is the generic type from which you want to convert and TTo is the type to which you want to convert to.
Following sample code illustrates a converter which converts between string and System.Uri.
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
Create (or retrieve via dependency injection) an instance of ConverterRegistry and register those converters you like to use later on. Beware that you will have to register a converter for each direction you want to convert (if you support two-way conversion). Following example shows how to register the StringToUriConverter to convert between string and Uri and vice versa.
```
IConverterRegistry converterRegistry = new ConverterRegistry();
converterRegistry.RegisterConverter<string, Uri>(() => new StringToUriConverter());
converterRegistry.RegisterConverter<Uri, string>(() => new StringToUriConverter());
```

#### Convert between types
Now, after having set-up a basic converter, we can use IConverterRegistry to convert between object of different types.

Convert from string to System.Uri
```
var uri = converterRegistry.Convert<string, Uri>("http://github.com/");
```
Convert from System.Uri to string
```
var uriAsString = converterRegistry.Convert<Uri, string>(uri);
```

### License
TypeConverter is Copyright &copy; 2018 [Thomas Galliker](https://ch.linkedin.com/in/thomasgalliker). Free for non-commercial use. For commercial use please contact the author.
