using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;
using System.Windows.Controls;
using System.Windows.Media.Animation;
using System.Windows;
using System.Windows.Media;

namespace TumblerTimePicker
{
    public class TumblerDataToOffsetConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            // TumblerDataToOffsetConverter is just a shell for getting SelectedValueChanged event and using it to adjust the items grid
            // canvas offset.  Reason we don't just bind the Canvas.Top property directly is because that binding will
            // get lost when we manually set Canvas.Top via drag support
            Grid tumbler = values[0] as Grid;
            TumblerData td = tumbler.Tag as TumblerData;
            if (td != null)
            {
                double itemHeight = tumbler.ActualHeight / (td.Values.Count);
                double offset = td.SelectedValueIndex > 0 ? -(td.SelectedValueIndex - 2) * itemHeight : itemHeight * 2;
                // animate from current position to value offset
                DoubleAnimation da = new DoubleAnimation();
                da.From = Canvas.GetTop(tumbler);
                da.To = offset;
                da.Duration = new Duration(TimeSpan.FromMilliseconds(100));
                tumbler.BeginAnimation(Canvas.TopProperty, da);
            }
            return values[2];
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    //public class OpenToBrushConverter : IValueConverter
    //{
    //    public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
    //    {
    //        bool isOpen = (bool)value;
    //        return isOpen ? Brushes.Black : Brushes.Transparent;
    //    }

    //    public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
    //    {
    //        throw new NotImplementedException();
    //    }
    //}

    public class BooleanToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return ((bool)value && parameter == null) || (!(bool)value && parameter != null) ? Visibility.Visible : Visibility.Hidden;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class OpenOrSelectedToVisibilityConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            TumblerData td = values[0] as TumblerData;
            bool isOpen = (bool)values[1];
            int currentIndex = (int)values[2];
            return isOpen || (td != null && currentIndex == td.SelectedValueIndex) ? Visibility.Visible : Visibility.Hidden;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

}
