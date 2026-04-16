using System.Globalization;
using System.Windows.Data;

namespace AppName.UI.Converters;

public sealed class NullableBoolEqualsConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (parameter is null)
        {
            return false;
        }

        if (!bool.TryParse(parameter.ToString(), out var expected))
        {
            return false;
        }

        return value is bool actual && actual == expected;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (parameter is null)
        {
            return Binding.DoNothing;
        }

        if (!bool.TryParse(parameter.ToString(), out var expected))
        {
            return Binding.DoNothing;
        }

        return value is true ? expected : Binding.DoNothing;
    }
}
