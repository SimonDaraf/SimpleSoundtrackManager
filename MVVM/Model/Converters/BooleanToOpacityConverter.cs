using System.Globalization;
using System.Windows.Data;

namespace SimpleSoundtrackManager.MVVM.Model.Converters
{
    public class BooleanToVisibilityConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            bool isMouseOver = (bool)values[0];
            bool isActive = (bool)values[1];

            return isActive ? 1.0 : isMouseOver ? 1.0 : 0.5;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
