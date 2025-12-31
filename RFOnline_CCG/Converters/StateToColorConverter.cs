using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;
using RFCardGame.Core;

namespace RFOnline_CCG
{
    public class StateToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            // Преобразует CreatureState в соответствующий цвет для отображения
            if (value is CreatureState state)
            {
                return state switch
                {
                    CreatureState.Active => new SolidColorBrush(Colors.Green),
                    CreatureState.Exhausted => new SolidColorBrush(Colors.Orange),
                    CreatureState.Asleep => new SolidColorBrush(Colors.Gray),
                    _ => new SolidColorBrush(Colors.White)
                };
            }
            return new SolidColorBrush(Colors.White);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}