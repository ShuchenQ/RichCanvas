﻿using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace RichCanvas.Converters
{
    public class BoolToGeometryConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var enableGrid = (bool)value;
            if (enableGrid)
            {
                return parameter;
            }
            return new GeometryDrawing(Brushes.AliceBlue, new Pen() { Brush = Brushes.AliceBlue }, new RectangleGeometry(new System.Windows.Rect(0, 0, 1, 1)));
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}