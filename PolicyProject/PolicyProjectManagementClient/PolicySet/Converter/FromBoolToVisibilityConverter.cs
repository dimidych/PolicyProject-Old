﻿using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace PolicyProjectManagementClient
{
    public class FromBoolToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
                return Visibility.Collapsed;

            return (bool) value ? Visibility.Visible : Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (Visibility?) value == Visibility.Visible;
        }
    }
}