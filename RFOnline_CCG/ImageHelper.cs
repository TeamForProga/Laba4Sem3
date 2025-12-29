// ImageHelper.cs
using RFCardGame.Core;
using System;
using System.IO;
using System.Windows.Media.Imaging;

namespace RFOnline_CCG
{
    public static class ImageHelper
    {
        private static readonly string BasePath = "Images/Cards/";
        private static readonly BitmapImage DefaultCreatureImage;
        private static readonly BitmapImage DefaultSpellImage;
        private static readonly BitmapImage DefaultArtifactImage;

        static ImageHelper()
        {
            // Загружаем изображения по умолчанию
            DefaultCreatureImage = LoadImage("Default/Creature.jpg");
            DefaultSpellImage = LoadImage("Default/Spell.jpg");
            DefaultArtifactImage = LoadImage("Default/Artifact.jpg");
        }

        public static BitmapImage GetCardImage(string imagePath, CardType cardType)
        {
            try
            {
                if (File.Exists(imagePath))
                {
                    return new BitmapImage(new Uri(imagePath, UriKind.RelativeOrAbsolute));
                }

                // Если файл не найден, возвращаем изображение по умолчанию
                return cardType switch
                {
                    CardType.Creature => DefaultCreatureImage,
                    CardType.Spell => DefaultSpellImage,
                    CardType.Artifact => DefaultArtifactImage,
                    _ => DefaultCreatureImage
                };
            }
            catch
            {
                return DefaultCreatureImage;
            }
        }

        private static BitmapImage LoadImage(string path)
        {
            try
            {
                var uri = new Uri($"pack://application:,,,/{path}", UriKind.Absolute);
                return new BitmapImage(uri);
            }
            catch
            {
                // Создаем пустое изображение
                return new BitmapImage();
            }
        }
    }
}