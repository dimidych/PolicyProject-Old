using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows.Data;
using PolicyProjectManagementService;

namespace PolicyProjectManagementClient
{
    public class FromPlatformIdToPlatformNameConverter : IMultiValueConverter
    {
        public object Convert(object[] value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null || !value.Any() || value[0] == null || value[1] == null)
                return string.Empty;

            var platformId = (short) value[0];
            var platformCollection = (IEnumerable<PlatformDataContract>) value[1];
            var result = platformCollection.FirstOrDefault(x => x.PlatformId == platformId);
            return result == null ? string.Empty : result.PlatformName;
        }

        public object[] ConvertBack(object value, Type[] targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}