using Kang.Util;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace ExcelToDatabase.Views.Home.Converter
{
    public class ResultForegroundConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            String _value = value.ToString();
            if (StringUtil.isNotBlank(_value))
            {
                if ("导入成功".Equals(_value))
                {
                    return SolidColorBrushConverter.From16JinZhi("#FF0000");
                }
            }
            return SolidColorBrushConverter.From16JinZhi("#000000");
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
