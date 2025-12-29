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
            if (value is string imagePath && !string.IsNullOrEmpty(imagePath))
            {
                    // Если путь относительный - делаем его абсолютным
                    if (!System.IO.Path.IsPathRooted(imagePath))
                    {
                        imagePath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, imagePath);
                    }

                    if (System.IO.File.Exists(imagePath))
                    {
                        return new BitmapImage(new Uri(imagePath, UriKind.Absolute));
                    }
                    else
                    {
                    }  
            }
            string image = value as string;
            return new BitmapImage(new Uri(image, UriKind.Absolute));
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}