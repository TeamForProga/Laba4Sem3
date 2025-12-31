using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media.Imaging;

namespace RFOnline_CCG
{
    public class ImagePathConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            // Конвертер для преобразования строкового пути к изображению в BitmapImage
            if (value is string imagePath && !string.IsNullOrEmpty(imagePath))
            {
                // Преобразование относительного пути в абсолютный
                if (!System.IO.Path.IsPathRooted(imagePath))
                {
                    imagePath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, imagePath);
                }

                // Загрузка изображения, если файл существует
                if (System.IO.File.Exists(imagePath))
                {
                    return new BitmapImage(new Uri(imagePath, UriKind.Absolute));
                }
                else
                {
                    // Логирование отсутствующего файла изображения
                }
            }
            string image = value as string;
            // Возврат изображения по абсолютному пути (будет исключение при неверном пути)
            return new BitmapImage(new Uri(image, UriKind.Absolute));
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}