// TypeToVisibilityConverter.cs (обновленный)
using RFCardGame.Core;
using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace RFOnline_CCG
{
    public class TypeToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is CardType cardType)
            {
                // Если параметр "creature" - показываем только для существ
                if (parameter?.ToString() == "creature")
                {
                    return cardType == CardType.Creature ? Visibility.Visible : Visibility.Collapsed;
                }
                // Если параметр "spell" - показываем только для заклинаний
                else if (parameter?.ToString() == "spell")
                {
                    return cardType == CardType.Spell ? Visibility.Visible : Visibility.Collapsed;
                }
                // Если параметр "artifact" - показываем только для артефактов
                else if (parameter?.ToString() == "artifact")
                {
                    return cardType == CardType.Artifact ? Visibility.Visible : Visibility.Collapsed;
                }
                // Без параметра или "default" - показываем только для существ (для статистики)
                else
                {
                    return cardType == CardType.Creature ? Visibility.Visible : Visibility.Collapsed;
                }
            }
            return Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}