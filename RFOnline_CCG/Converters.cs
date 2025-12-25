using RFCardGame.Core;
using RFOnline_CCG.Resources;
using RFOnline_CCG.ViewModels;
using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace RFOnline_CCG.Converters
{
    #region Видимость
    /// <summary>
    /// Конвертер bool в Visibility
    /// </summary>
    public class BoolToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool isVisible = false;

            if (value is bool b)
            {
                isVisible = b;
            }
            else if (value is int i)
            {
                isVisible = i > 0;
            }
            else if (value is string s)
            {
                isVisible = !string.IsNullOrEmpty(s);
            }
            else if (value != null)
            {
                isVisible = true;
            }

            // Параметр может инвертировать логику
            if (parameter is string param && param.ToLower() == "inverse")
            {
                isVisible = !isVisible;
            }

            return isVisible ? Visibility.Visible : Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is Visibility visibility)
            {
                return visibility == Visibility.Visible;
            }
            return false;
        }
    }

    /// <summary>
    /// Конвертер для отображения Visibility.Hidden вместо Collapsed
    /// </summary>
    public class BoolToHiddenVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool isVisible = false;

            if (value is bool b)
            {
                isVisible = b;
            }
            else if (value != null)
            {
                isVisible = true;
            }

            // Параметр может инвертировать логику
            if (parameter is string param && param.ToLower() == "inverse")
            {
                isVisible = !isVisible;
            }

            return isVisible ? Visibility.Visible : Visibility.Hidden;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is Visibility visibility)
            {
                return visibility == Visibility.Visible;
            }
            return false;
        }
    }

    /// <summary>
    /// Конвертер сравнения значений
    /// </summary>
    public class EqualsToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value?.ToString() == parameter?.ToString()
                ? Visibility.Visible
                : Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
    public class CardToImageConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            try
            {
                if (value is CardViewModel cardVM)
                {
                    var card = cardVM.GetCard();
                    string imageName = card.Name.Replace(" ", "_") + ".png";

                    // Определяем тип карты
                    string folder = card.Type switch
                    {
                        CardType.Creature => "Creatures/",
                        CardType.Spell => "Spells/",
                        CardType.Artifact => "Artifacts/",
                        _ => ""
                    };

                    string factionFolder = card.Faction switch
                    {
                        Faction.Accretia => "Accretia/",
                        Faction.Bellato => "Bellato/",
                        Faction.Cora => "Cora/",
                        _ => "Neutral/"
                    };

                    string path = $"/Images/Cards/{folder}{factionFolder}{imageName}";
                    return new BitmapImage(new Uri(path, UriKind.RelativeOrAbsolute));
                }
            }
            catch
            {
                // Возвращаем изображение по умолчанию
                return new BitmapImage(new Uri("/Images/Cards/DefaultCard.png", UriKind.RelativeOrAbsolute));
            }

            return new BitmapImage();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class FactionToImageConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string factionName = value?.ToString() ?? "Neutral";
            string path = $"/Images/Factions/{factionName}.png";
            return new BitmapImage(new Uri(path, UriKind.RelativeOrAbsolute));
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}

    public class FactionToImageConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is Faction faction)
            {
                return ImageManager.GetFactionImage(faction);
            }

            return ImageManager.GetFactionImage(Faction.Neutral);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
    #endregion

    #region Цвета
    /// <summary>
    /// Конвертер здоровья в цвет
    /// </summary>
    /// <summary>
    /// Конвертер здоровья в цвет
    /// </summary>
    public class HealthToColorConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values.Length >= 2 && values[0] is int health && values[1] is int maxHealth)
            {
                if (health <= 0)
                    return new SolidColorBrush(Colors.Gray);

                double percentage = (double)health / maxHealth;

                if (percentage > 0.7)
                    return new SolidColorBrush(Color.FromRgb(0, 200, 0)); // Зеленый
                if (percentage > 0.3)
                    return new SolidColorBrush(Color.FromRgb(255, 200, 0)); // Желтый
                return new SolidColorBrush(Color.FromRgb(220, 0, 0)); // Красный
            }

            // Если пришло одно значение вместо двух
            if (values.Length >= 1 && values[0] is double singlePercentage)
            {
                if (singlePercentage <= 0)
                    return new SolidColorBrush(Colors.Gray);

                if (singlePercentage > 0.7)
                    return new SolidColorBrush(Color.FromRgb(0, 200, 0));
                if (singlePercentage > 0.3)
                    return new SolidColorBrush(Color.FromRgb(255, 200, 0));
                return new SolidColorBrush(Color.FromRgb(220, 0, 0));
            }

            // Безопасное значение по умолчанию
            return new SolidColorBrush(Color.FromRgb(100, 100, 100));
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
    /// <summary>
    /// Конвертер фракции в цвет
    /// </summary>
    public class FactionToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is Faction faction)
            {
                return faction switch
                {
                    Faction.Accretia => new SolidColorBrush(Color.FromRgb(255, 50, 50)), // Красный
                    Faction.Bellato => new SolidColorBrush(Color.FromRgb(50, 150, 255)), // Синий
                    Faction.Cora => new SolidColorBrush(Color.FromRgb(180, 50, 255)),    // Фиолетовый
                    Faction.Neutral => new SolidColorBrush(Color.FromRgb(150, 150, 150)), // Серый
                    _ => new SolidColorBrush(Color.FromRgb(255, 255, 255))               // Белый
                };
            }

            if (value is string factionName)
            {
                return factionName.ToLower() switch
                {
                    "accretia" => new SolidColorBrush(Color.FromRgb(255, 50, 50)),
                    "bellato" => new SolidColorBrush(Color.FromRgb(50, 150, 255)),
                    "cora" => new SolidColorBrush(Color.FromRgb(180, 50, 255)),
                    "neutral" => new SolidColorBrush(Color.FromRgb(150, 150, 150)),
                    _ => new SolidColorBrush(Color.FromRgb(255, 255, 255))
                };
            }

            return new SolidColorBrush(Color.FromRgb(255, 255, 255));
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// Конвертер состояния существа в цвет
    /// </summary>
    public class CreatureStateToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is CreatureState state)
            {
                return state switch
                {
                    CreatureState.Active => new SolidColorBrush(Color.FromRgb(0, 200, 0)),   // Зеленый
                    CreatureState.Asleep => new SolidColorBrush(Color.FromRgb(100, 100, 200)), // Синий
                    CreatureState.Exhausted => new SolidColorBrush(Color.FromRgb(200, 100, 0)), // Оранжевый
                    _ => new SolidColorBrush(Color.FromRgb(150, 150, 150))
                };
            }

            return new SolidColorBrush(Color.FromRgb(150, 150, 150));
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// Конвертер энергии в цвет
    /// </summary>
    public class EnergyToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is int energy)
            {
                // Цвет меняется от красного к зеленому в зависимости от количества энергии
                double factor = Math.Min(energy / 10.0, 1.0);
                byte red = (byte)(255 * (1.0 - factor));
                byte green = (byte)(255 * factor);

                return new SolidColorBrush(Color.FromRgb(red, green, 0));
            }

            return new SolidColorBrush(Color.FromRgb(255, 150, 0)); // Оранжевый по умолчанию
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
    #endregion

    #region Текст и форматирование
    /// <summary>
    /// Конвертер для отображения здоровья существа
    /// </summary>
    public class HealthToTextConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values.Length >= 2 && values[0] is int health && values[1] is int maxHealth)
            {
                return $"{health}/{maxHealth}";
            }

            return "0/0";
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// Конвертер для форматирования атаки/здоровья
    /// </summary>
    public class AttackHealthConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values.Length >= 2 && values[0] is int attack && values[1] is int health)
            {
                return $"{attack}/{health}";
            }

            return "0/0";
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// Конвертер для отображения энергии
    /// </summary>
    public class EnergyTextConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values.Length >= 2 && values[0] is int current && values[1] is int max)
            {
                return $"{current}/{max}";
            }

            if (values.Length >= 1 && values[0] is int energy)
            {
                return energy.ToString();
            }

            return "0/0";
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// Конвертер для отображения стоимости карты с цветом
    /// </summary>
    public class CardCostConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is int cost)
            {
                // Параметр может содержать текущую энергию игрока
                if (parameter is int currentEnergy)
                {
                    // Если не хватает энергии, выделяем красным
                    return cost > currentEnergy ? $"❗ {cost}" : cost.ToString();
                }
                return cost.ToString();
            }

            return "0";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
    #endregion

    #region Фракции и сравнения
    /// <summary>
    /// Конвертер фракции в bool (для RadioButton)
    /// </summary>
    public class FactionToBoolConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null || parameter == null)
                return false;

            string valueStr = value.ToString();
            string paramStr = parameter.ToString();

            return valueStr.Equals(paramStr, StringComparison.OrdinalIgnoreCase);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool isChecked && isChecked)
            {
                if (Enum.TryParse<Faction>(parameter?.ToString(), true, out var faction))
                {
                    return faction;
                }
            }

            return Binding.DoNothing;
        }
    }

    /// <summary>
    /// Конвертер для сравнения значений
    /// </summary>
    public class EqualsToBoolConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value?.ToString() == parameter?.ToString();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// Конвертер для проверки "больше чем"
    /// </summary>
    public class GreaterThanConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null || parameter == null)
                return false;

            if (double.TryParse(value.ToString(), out double val) &&
                double.TryParse(parameter.ToString(), out double param))
            {
                return val > param;
            }

            return false;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// Конвертер для проверки "меньше чем"
    /// </summary>
    public class LessThanConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null || parameter == null)
                return false;

            if (double.TryParse(value.ToString(), out double val) &&
                double.TryParse(parameter.ToString(), out double param))
            {
                return val < param;
            }

            return false;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
    #endregion

    #region Карты и игровые объекты
    /// <summary>
    /// Конвертер типа карты в текст
    /// </summary>
    public class CardTypeToTextConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is CardType cardType)
            {
                return cardType switch
                {
                    CardType.Creature => "Существо",
                    CardType.Spell => "Заклинание",
                    CardType.Artifact => "Артефакт",
                    _ => "Неизвестно"
                };
            }

            return "Неизвестно";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// Конвертер подтипа заклинания в текст
    /// </summary>
    public class SpellSubtypeToTextConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is SpellSubtype subtype)
            {
                return subtype switch
                {
                    SpellSubtype.Attack => "Атака",
                    SpellSubtype.Healing => "Лечение",
                    SpellSubtype.Buff => "Усиление",
                    SpellSubtype.Other => "Особое",
                    _ => "Заклинание"
                };
            }

            return "Заклинание";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// Конвертер для определения типа карты
    /// </summary>
    public class IsCreatureConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is CardType cardType)
            {
                return cardType == CardType.Creature;
            }

            if (value is ICard card)
            {
                return card.Type == CardType.Creature;
            }

            return false;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// Конвертер для определения типа заклинания
    /// </summary>
    public class IsSpellConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is CardType cardType)
            {
                return cardType == CardType.Spell;
            }

            if (value is ICard card)
            {
                return card.Type == CardType.Spell;
            }

            return false;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
    #endregion

    #region Проценты и математика
    /// <summary>
    /// Конвертер в проценты
    /// </summary>
    public class PercentageConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is double d)
            {
                return d * 100;
            }

            if (value is int i)
            {
                return i * 100;
            }

            if (value is float f)
            {
                return f * 100;
            }

            return 0;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is double d)
            {
                return d / 100;
            }

            if (value is int i)
            {
                return i / 100.0;
            }

            return 0.0;
        }
    }

    /// <summary>
    /// Конвертер для расчета значения из процентов
    /// </summary>
    public class ValueFromPercentageConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values.Length >= 2 && values[0] is double percentage && values[1] is double maxValue)
            {
                return percentage * maxValue;
            }

            return 0.0;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// Конвертер для сложения значений
    /// </summary>
    public class AddConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            double result = 0;

            foreach (var value in values)
            {
                if (value is double d)
                {
                    result += d;
                }
                else if (value is int i)
                {
                    result += i;
                }
                else if (value is float f)
                {
                    result += f;
                }
            }

            // Добавляем параметр если есть
            if (parameter != null && double.TryParse(parameter.ToString(), out double param))
            {
                result += param;
            }

            return result;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
    #endregion

    #region Оптимизация
    /// <summary>
    /// Конвертер для ограничения длины текста
    /// </summary>
    public class TextTruncateConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string text)
            {
                int maxLength = 20; // Значение по умолчанию

                if (parameter != null && int.TryParse(parameter.ToString(), out int paramLength))
                {
                    maxLength = paramLength;
                }

                if (text.Length > maxLength)
                {
                    return text.Substring(0, maxLength - 3) + "...";
                }

                return text;
            }

            return string.Empty;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// Конвертер для форматирования даты
    /// </summary>
    public class DateTimeConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is DateTime dateTime)
            {
                string format = parameter as string ?? "dd.MM.yyyy HH:mm";
                return dateTime.ToString(format);
            }

            return string.Empty;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
    #endregion