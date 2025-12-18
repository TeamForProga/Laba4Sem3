using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace RFOnline_CCG
{
    public partial class MainWindow : Window
    {
        private bool _isDragging = false;
        private Point _clickPosition;

        public MainWindow()
        {
            InitializeComponent();
        }

        // ПЕРЕКЛЮЧЕНИЕ ЭКРАНОВ
        private void StartGame_Click(object sender, RoutedEventArgs e)
        {
            MainMenuUI.Visibility = Visibility.Collapsed;
            NewGameScreen.Visibility = Visibility.Visible;
        }

        private void BackToMenu_Click(object sender, RoutedEventArgs e)
        {
            MainMenuUI.Visibility = Visibility.Collapsed;
            LoadGameScreen.Visibility = Visibility.Visible;
        }

        private void Exit_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        // ЛОГИКА DRAG & DROP
        private void Card_MouseDown(object sender, MouseButtonEventArgs e)
        {
            var card = sender as FrameworkElement;
            if (card == null) return;

            _isDragging = true;
            _clickPosition = e.GetPosition(this);
            card.CaptureMouse();

            if (!(card.RenderTransform is TranslateTransform))
                card.RenderTransform = new TranslateTransform();

            Panel.SetZIndex(card, 1000); // Чтобы карта была выше всех
        }

        private void Card_MouseMove(object sender, MouseEventArgs e)
        {
            if (!_isDragging) return;

            var card = sender as FrameworkElement;
            var currentPosition = e.GetPosition(this);
            var transform = card.RenderTransform as TranslateTransform;

            if (transform != null)
            {
                transform.X = currentPosition.X - _clickPosition.X;
                transform.Y = currentPosition.Y - _clickPosition.Y;
            }

            // Визуальная подсветка зоны при наведении
            Point dropPoint = e.GetPosition(DropZone);
            bool isOver = dropPoint.X >= 0 && dropPoint.X <= DropZone.ActualWidth &&
                          dropPoint.Y >= 0 && dropPoint.Y <= DropZone.ActualHeight;

            DropZone.Opacity = isOver ? 0.6 : 0.2;
        }

        private void Card_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (!_isDragging) return;

            var card = sender as FrameworkElement;
            _isDragging = false;
            card.ReleaseMouseCapture();

            Point dropPoint = e.GetPosition(DropZone);
            bool isInside = dropPoint.X >= 0 && dropPoint.X <= DropZone.ActualWidth &&
                            dropPoint.Y >= 0 && dropPoint.Y <= DropZone.ActualHeight;

            if (isInside)
            {
                // Если бросили в зону - карта "исчезает" из руки (разыгрывается)
                card.Visibility = Visibility.Collapsed;
                DropZone.Opacity = 0.2;
                MessageBox.Show("ЮНИТ РАЗВЕРНУТ!");
            }
            else
            {
                // Возвращаем на место
                var transform = card.RenderTransform as TranslateTransform;
                transform.X = 0;
                transform.Y = 0;
            }
        }

        private void BtnStartBoardGame_Click(object sender, RoutedEventArgs e)
        {
            NewGameScreen.Visibility = Visibility.Collapsed;
            GameBoardUI.Visibility = Visibility.Visible;
        }

        private void BtnNewGameCancel_Click(object sender, RoutedEventArgs e)
        {
            NewGameScreen.Visibility = Visibility.Collapsed;
            MainMenuUI.Visibility = Visibility.Visible;
        }

        private void BtnLoadGameCancel_Click(object sender, RoutedEventArgs e)
        {
            LoadGameScreen.Visibility = Visibility.Collapsed;
            MainMenuUI.Visibility = Visibility.Visible;
        }

        private void BtnLoadGameStart_Click(object sender, RoutedEventArgs e)
        {
            LoadGameScreen.Visibility = Visibility.Collapsed;
            GameBoardUI.Visibility = Visibility.Visible;
        }
    }
}