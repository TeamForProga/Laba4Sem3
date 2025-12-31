// SelectedToBorderBrushConverter.cs
using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace RFOnline_CCG
{
    public class SelectedToBorderBrushConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            // Если карта выбрана - подсвечиваем ее
            if (value is bool isSelected && isSelected)
            {
                return new SolidColorBrush(Colors.Yellow);
            }

            // Если это существо игрока - синий
            if (parameter?.ToString() == "PlayerCreature")
            {
                return new SolidColorBrush(Colors.Cyan);
            }

            // Если это существо противника - красный
            if (parameter?.ToString() == "OpponentCreature")
            {
                return new SolidColorBrush(Colors.Red);
            }

            // По умолчанию - обычный цвет
            return new SolidColorBrush(Color.FromRgb(0, 242, 255)); // NeonCyan
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}