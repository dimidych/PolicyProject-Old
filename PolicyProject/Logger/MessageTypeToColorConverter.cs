using System;
using System.Drawing;
using System.Globalization;
using System.Windows.Data;

namespace Logger
{
    public class MessageTypeToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var msgType = (MessageTypeEnum) parameter;
            var result = Color.Black;

            switch (msgType)
            {
                case MessageTypeEnum.Success:
                    result = Color.ForestGreen;
                    break;

                case MessageTypeEnum.Warning:
                    result = Color.DarkOrange;
                    break;

                case MessageTypeEnum.Error:
                    result = Color.DarkRed;
                    break;
            }

            return result;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}