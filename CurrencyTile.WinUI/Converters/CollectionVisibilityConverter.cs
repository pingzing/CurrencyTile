using System;
using System.Collections;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Data;

namespace CurrencyTile.WinUI.Converters;

class CollectionVisibilityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        bool isInverted = parameter is string str && str == "True";
        bool isEmpty = true;
        if (value is IEnumerable collection)
        {
            var enumerator = collection.GetEnumerator();
            isEmpty = !enumerator.MoveNext();
        }

        if (isInverted)
        {
            return isEmpty ? Visibility.Visible : Visibility.Collapsed;
        }
        else
        {
            return isEmpty ? Visibility.Collapsed : Visibility.Visible;
        }
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        throw new NotImplementedException();
    }
}
