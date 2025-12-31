// ImageManager.cs
using RFCardGame.Core;
using System;
using System.Collections.Generic;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace RFOnline_CCG.Resources
{
    public static class ImageManager
    {
        private static readonly Dictionary<string, ImageSource> _imageCache = new();

        // Базовые пути
        private const string BasePath = "pack://application:,,,/Images/";

        // Пути к основным папкам
        public static class Paths
        {
            public const string Factions = BasePath + "Factions/";
            public const string Cards = BasePath + "Cards/";
            public const string Creatures = Cards + "Creatures/";
            public const string Spells = Cards + "Spells/";
            public const string Artifacts = Cards + "Artifacts/";
            public const string UI = BasePath + "UI/";
            public const string Icons = BasePath + "Icons/";
        }

        // Метод для получения изображения с кэшированием
        public static ImageSource GetImage(string path)
        {
            if (_imageCache.TryGetValue(path, out var cachedImage))
                return cachedImage;

            try
            {
                var bitmap = new BitmapImage();
                bitmap.BeginInit();
                bitmap.UriSource = new Uri(path, UriKind.Absolute);
                bitmap.CacheOption = BitmapCacheOption.OnLoad;
                bitmap.EndInit();
                bitmap.Freeze(); // Важно для многопоточности

                _imageCache[path] = bitmap;
                return bitmap;
            }
            catch
            {
                // Возвращаем пустое изображение при ошибке
                return CreateEmptyImage();
            }
        }

        // Изображения фракций
        public static ImageSource GetFactionImage(Faction faction)
        {
            string fileName = faction switch
            {
                Faction.Accretia => "Accretia.png",
                Faction.Bellato => "Bellato.png",
                Faction.Cora => "Cora.png",
                _ => "Neutral.png"
            };

            return GetImage(Paths.Factions + fileName);
        }

        // Изображения карт
        public static ImageSource GetCardImage(string cardName, CardType cardType)
        {
            try
            {
                string fileName = cardName.Replace(" ", "_") + ".png";

                string path = cardType switch
                {
                    CardType.Creature => Paths.Creatures,
                    CardType.Spell => Paths.Spells,
                    CardType.Artifact => Paths.Artifacts,
                    _ => Paths.Cards
                };

                // Пытаемся найти по точному имени
                var image = GetImage(path + fileName);
                if (image != null && image is BitmapImage bitmap && !bitmap.IsDownloading)
                    return image;

                // Возвращаем изображение по умолчанию
                return GetDefaultCardImage(cardType);
            }
            catch
            {
                return GetDefaultCardImage(cardType);
            }
        }

        private static ImageSource GetDefaultCardImage(CardType cardType)
        {
            string defaultImage = cardType switch
            {
                CardType.Creature => "DefaultCreature.png",
                CardType.Spell => "DefaultSpell.png",
                CardType.Artifact => "DefaultArtifact.png",
                _ => "DefaultCard.png"
            };

            return GetImage(Paths.Cards + defaultImage);
        }

        // UI элементы
        public static ImageSource GetBackgroundImage()
        {
            return GetImage(Paths.UI + "Background.jpg");
        }

        public static ImageSource GetLogoImage()
        {
            return GetImage(Paths.UI + "Logo.png");
        }

        // Иконки
        public static ImageSource GetAttackIcon()
        {
            return GetImage(Paths.Icons + "Attack.png");
        }

        public static ImageSource GetHealthIcon()
        {
            return GetImage(Paths.Icons + "Health.png");
        }

        public static ImageSource GetEnergyIcon()
        {
            return GetImage(Paths.Icons + "Energy.png");
        }

        // Создание пустого изображения
        public static ImageSource CreateEmptyImage()
        {
            var drawingVisual = new DrawingVisual();
            using (var drawingContext = drawingVisual.RenderOpen())
            {
                drawingContext.DrawRectangle(
                    Brushes.Transparent,
                    null,
                    new System.Windows.Rect(0, 0, 1, 1));
            }

            var renderTarget = new RenderTargetBitmap(1, 1, 96, 96, PixelFormats.Pbgra32);
            renderTarget.Render(drawingVisual);
            renderTarget.Freeze();

            return renderTarget;
        }
    }
}