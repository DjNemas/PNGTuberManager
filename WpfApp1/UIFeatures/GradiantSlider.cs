using System.Windows.Controls;
using System.Windows;

namespace PNGTuberManager.UIFeatures
{
    public class GradientSlider : Slider
    {
        // DependencyProperty für GradientPosition
        public static readonly DependencyProperty GradientPositionProperty =
            DependencyProperty.RegisterAttached(
                "GradientPosition",
                typeof(double),
                typeof(GradientSlider),
                new FrameworkPropertyMetadata(0.0, FrameworkPropertyMetadataOptions.AffectsRender));

        public static double GetGradientPosition(DependencyObject obj)
        {
            return (double)obj.GetValue(GradientPositionProperty);
        }

        public static void SetGradientPosition(DependencyObject obj, double value)
        {
            obj.SetValue(GradientPositionProperty, value);
        }
    }
}