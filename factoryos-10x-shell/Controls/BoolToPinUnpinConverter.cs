using System;
using Windows.UI.Xaml.Data;

namespace factoryos_10x_shell.Controls
{
    public class BoolToPinUnpinConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            bool isPinned = (bool)value;
            return isPinned ? "Desafixar" : "Fixar";
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            return value.ToString() == "Fixar";
        }
    }
}
