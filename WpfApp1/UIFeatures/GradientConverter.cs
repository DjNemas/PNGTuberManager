using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;

namespace PNGTuberManager.UIFeatures
{
    public class GradientConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values is not null && values.Length > 0 && values[0] is double position)
            {
                var calculatedValue = position / 0.75D;

                if (calculatedValue < 0 || calculatedValue > 1)
                    return Brushes.Transparent;

                // Farbverlauf erstellen
                GradientStopCollection gradientStops = new GradientStopCollection
                {
                    new GradientStop(Colors.Cyan, 0),
                    new GradientStop(Colors.Cyan, calculatedValue),
                    new GradientStop(Colors.Red, calculatedValue),
                    new GradientStop(Colors.Red, 1)
                };

                return new LinearGradientBrush(gradientStops, new Point(0, 0), new Point(1, 0));
            }

            return Brushes.Transparent;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}