using RFCardGame.Core;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Effects;

namespace RFOnline_CCG
{
    public partial class MainWindow : Window
    {
        private GameViewModel _viewModel;
        private bool _isDragging = false;
        private Point _clickPosition;
        private string _selectedSaveFile;

        public MainWindow()
        {
            InitializeComponent();
            _viewModel = new GameViewModel();
            DataContext = _viewModel;
        }

        // ПЕРЕКЛЮЧЕНИЕ ЭКРАНОВ
        private void StartGame_Click(object sender, RoutedEventArgs e)
        {
            MainMenuUI.Visibility = Visibility.Collapsed;
            NewGameScreen.Visibility = Visibility.Visible;
        }

        private void BackToMenu_Click(object sender, RoutedEventArgs e)
        {
            if (GameBoardUI.Visibility == Visibility.Visible)
            {
                GameBoardUI.Visibility = Visibility.Collapsed;
            }
            else if (LoadGameScreen.Visibility == Visibility.Visible)
            {
                LoadGameScreen.Visibility = Visibility.Collapsed;
            }
            else if (NewGameScreen.Visibility == Visibility.Visible)
            {
                NewGameScreen.Visibility = Visibility.Collapsed;
            }

            MainMenuUI.Visibility = Visibility.Visible;
        }

        private void Exit_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        // НАЧАТЬ НОВУЮ ИГРУ
        private void BtnStartBoardGame_Click(object sender, RoutedEventArgs e)
        {
            Faction player1Faction = GetSelectedFactionFromRadioButtons(
                Player1Accretia, Player1Bellato, Player1Cora);

            Faction player2Faction = GetSelectedFactionFromRadioButtons(
                Player2Accretia, Player2Bellato, Player2Cora);

            string player1Name = Player1NameBox.Text.Trim();
            if (string.IsNullOrWhiteSpace(player1Name))
                player1Name = "Игрок 1";

            string player2Name = Player2NameBox.Text.Trim();
            if (string.IsNullOrWhiteSpace(player2Name))
                player2Name = "Игрок 2";

            _viewModel.StartNewGame(player1Name, player1Faction, player2Name, player2Faction);

            NewGameScreen.Visibility = Visibility.Collapsed;
            GameBoardUI.Visibility = Visibility.Visible;
        }

        private Faction GetSelectedFactionFromRadioButtons(RadioButton accretia, RadioButton bellato, RadioButton cora)
        {
            if (accretia.IsChecked == true)
                return Faction.Accretia;
            else if (bellato.IsChecked == true)
                return Faction.Bellato;
            else if (cora.IsChecked == true)
                return Faction.Cora;

            return Faction.Neutral;
        }

        // ЗАГРУЗИТЬ ИГРУ
        private void SaveGame_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            _selectedSaveFile = "Saves/AUTO_SAVE_01.json";
        }

        private void BtnLoadGameStart_Click(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(_selectedSaveFile))
            {
                _viewModel.LoadGame(_selectedSaveFile);
                LoadGameScreen.Visibility = Visibility.Collapsed;
                GameBoardUI.Visibility = Visibility.Visible;
            }
            else
            {
                ShowGameMessage("Выберите сохранение для загрузки");
            }
        }

        private void Card_MouseDown(object sender, MouseButtonEventArgs e)
        {
            var border = sender as Border;
            if (border?.Tag is ICard gameCard)
            {
                // Сброс подсветки всех карт
                ResetAllCardHighlights();

                // Выбираем карту
                _viewModel.SelectedHandCard = gameCard;

                // Подсвечиваем выбранную карту
                HighlightCard(border, true);

                // Принудительно обновляем привязки
                _viewModel.OnPropertyChanged(nameof(_viewModel.SelectedHandCard));

                // Продолжаем перетаскивание
                _isDragging = true;
                _clickPosition = e.GetPosition(this);
                border.CaptureMouse();

                // Создаем клон для перетаскивания
                var clone = new Border();
                clone.Width = border.Width * 1.2;
                clone.Height = border.Height * 1.2;
                clone.Opacity = 0.9;
                clone.Background = border.Background;
                clone.BorderBrush = Brushes.Yellow;
                clone.BorderThickness = new Thickness(3);
                clone.CornerRadius = border.CornerRadius;
                clone.Effect = new DropShadowEffect
                {
                    Color = Colors.Yellow,
                    BlurRadius = 20,
                    Opacity = 0.8
                };

                // Добавляем контент
                var content = new StackPanel();
                var text = new TextBlock();
                text.Text = gameCard.Name;
                text.Foreground = Brushes.White;
                text.FontWeight = FontWeights.Bold;
                text.FontSize = 14;
                text.HorizontalAlignment = HorizontalAlignment.Center;
                text.VerticalAlignment = VerticalAlignment.Center;
                text.Margin = new Thickness(10);
                content.Children.Add(text);
                clone.Child = content;

                // Добавляем на Canvas
                var canvas = new Canvas { ClipToBounds = false };
                canvas.Children.Add(clone);
                Canvas.SetLeft(clone, _clickPosition.X - clone.Width / 2);
                Canvas.SetTop(clone, _clickPosition.Y - clone.Height / 2);
                canvas.Tag = clone;

                var overlay = new Grid { ClipToBounds = false };
                overlay.Children.Add(canvas);
                overlay.Background = Brushes.Transparent;
                overlay.Tag = canvas;

                MainGrid.Children.Add(overlay);
                Panel.SetZIndex(overlay, 9999);

                border.Tag = new object[] { gameCard, overlay };
            }
        }

        // Метод для подсветки карты
        private void HighlightCard(Border cardBorder, bool isSelected)
        {
            if (isSelected)
            {
                cardBorder.BorderBrush = Brushes.Yellow;
                cardBorder.BorderThickness = new Thickness(3);
                var glow = cardBorder.Effect as DropShadowEffect;
                if (glow != null)
                {
                    glow.Color = Colors.Yellow;
                    glow.BlurRadius = 20;
                    glow.Opacity = 0.7;
                }
            }
            else
            {
                cardBorder.BorderBrush = (SolidColorBrush)FindResource("NeonCyan");
                cardBorder.BorderThickness = new Thickness(1.5);
                var glow = cardBorder.Effect as DropShadowEffect;
                if (glow != null)
                {
                    glow.Color = Color.FromRgb(0, 242, 255);
                    glow.BlurRadius = 10;
                    glow.Opacity = 0.3;
                }
            }
        }

        // Метод для сброса подсветки всех карт
        private void ResetAllCardHighlights()
        {
            // Сброс подсветки карт в руке
            if (PlayerHandItems != null)
            {
                foreach (var item in PlayerHandItems.Items)
                {
                    var container = PlayerHandItems.ItemContainerGenerator.ContainerFromItem(item) as ContentPresenter;
                    if (container != null)
                    {
                        var border = FindVisualChild<Border>(container);
                        if (border != null)
                        {
                            HighlightCard(border, false);
                        }
                    }
                }
            }

            // Сброс подсветки существ
            // (добавьте аналогичный код для полей существ)
        }

        // Вспомогательный метод для поиска дочерних элементов
        private T FindVisualChild<T>(DependencyObject parent) where T : DependencyObject
        {
            if (parent == null) return null;

            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++)
            {
                var child = VisualTreeHelper.GetChild(parent, i);
                if (child is T result)
                    return result;

                var descendant = FindVisualChild<T>(child);
                if (descendant != null)
                    return descendant;
            }

            return null;
        }
        // Метод для подсветки карты

// Метод для сброса подсветки всех карт

// Вспомогательный метод для поиска дочерних элементов
        private void Creature_MouseDown(object sender, MouseButtonEventArgs e)
        {
            var border = sender as Border;
            if (border?.Tag is ICreatureCard gameCreature)
            {
                // Сброс подсветки всех существ
                ResetAllCreatureHighlights();

                bool isPlayerCreature = _viewModel.PlayerField.Contains(gameCreature);
                bool isOpponentCreature = _viewModel.OpponentField.Contains(gameCreature);

                // Подсветка выбранного существа
                if (isPlayerCreature)
                {
                    _viewModel.SelectedPlayerCreature = gameCreature;
                    HighlightCreature(border, "Cyan");
                    ShowGameMessage($"Выбрано: {gameCreature.Name}");
                }
                else if (isOpponentCreature)
                {
                    _viewModel.SelectedOpponentCreature = gameCreature;
                    HighlightCreature(border, "Red");
                    ShowGameMessage($"Цель: {gameCreature.Name}");
                }

                _viewModel.OnPropertyChanged(nameof(_viewModel.SelectedPlayerCreature));
                _viewModel.OnPropertyChanged(nameof(_viewModel.SelectedOpponentCreature));
            }
        }

        // Метод для подсветки существа
        private void HighlightCreature(Border creatureBorder, string color)
        {
            creatureBorder.BorderThickness = new Thickness(3);
            var glow = creatureBorder.Effect as DropShadowEffect;

            if (color == "Cyan")
            {
                creatureBorder.BorderBrush = Brushes.Cyan;
                if (glow != null)
                {
                    glow.Color = Colors.Cyan;
                    glow.BlurRadius = 15;
                    glow.Opacity = 0.6;
                }
            }
            else if (color == "Red")
            {
                creatureBorder.BorderBrush = Brushes.Red;
                if (glow != null)
                {
                    glow.Color = Colors.Red;
                    glow.BlurRadius = 15;
                    glow.Opacity = 0.6;
                }
            }
        }

        // Метод для сброса подсветки существ
        private void ResetAllCreatureHighlights()
        {
            // Реализация аналогична ResetAllCardHighlights
            // Нужно пройти по всем существам на полях и сбросить их подсветку
        }
        // Добавьте этот метод для сброса выбора
        public void ClearSelection()
        {
            _viewModel.SelectedHandCard = null;
            _viewModel.SelectedPlayerCreature = null;
            _viewModel.SelectedOpponentCreature = null;

            _viewModel.OnPropertyChanged(nameof(_viewModel.SelectedHandCard));
            _viewModel.OnPropertyChanged(nameof(_viewModel.SelectedPlayerCreature));
            _viewModel.OnPropertyChanged(nameof(_viewModel.SelectedOpponentCreature));
        }
        private void Card_MouseMove(object sender, MouseEventArgs e)
        {
            if (!_isDragging) return;

            var border = sender as Border;
            var currentPosition = e.GetPosition(this);

            if (border?.Tag is object[] data && data[1] is Grid overlay)
            {
                var canvas = overlay.Tag as Canvas;
                var clone = canvas?.Tag as Border;

                if (clone != null)
                {
                    Canvas.SetLeft(clone, currentPosition.X - 60);
                    Canvas.SetTop(clone, currentPosition.Y - 80);
                }
            }

            // Визуальная подсветка зоны при наведении
            Point dropPoint = e.GetPosition(DropZone);
            bool isOver = dropPoint.X >= 0 && dropPoint.X <= DropZone.ActualWidth &&
                          dropPoint.Y >= 0 && dropPoint.Y <= DropZone.ActualHeight;

            DropZone.Opacity = isOver ? 0.6 : 0.2;
            DropZone.BorderBrush = isOver ? Brushes.Lime : Brushes.Cyan;
        }

        private void Card_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (!_isDragging) return;

            var border = sender as Border;
            _isDragging = false;
            border.ReleaseMouseCapture();

            if (border?.Tag is object[] data)
            {
                var gameCard = data[0] as ICard;
                var overlay = data[1] as Grid;

                // Удаляем overlay с клоном
                if (overlay != null && MainGrid.Children.Contains(overlay))
                {
                    MainGrid.Children.Remove(overlay);
                }

                Point dropPoint = e.GetPosition(DropZone);
                bool isInside = dropPoint.X >= 0 && dropPoint.X <= DropZone.ActualWidth &&
                                dropPoint.Y >= 0 && dropPoint.Y <= DropZone.ActualHeight;

                if (isInside && gameCard is ICreatureCard creature)
                {
                    bool success = _viewModel.GameEngine.PlayCreatureCard(creature);
                    if (success)
                    {
                        _viewModel.UpdateAll();
                        ShowGameMessage($"{creature.Name} развернут!");
                    }
                    else
                    {
                        ShowGameMessage("Нельзя разыграть эту карту!");
                    }
                }
                else if (isInside && gameCard is ISpellCard spell)
                {
                    ShowGameMessage("Выберите цель для заклинания");
                }

                // Восстанавливаем Tag
                border.Tag = gameCard;
            }

            DropZone.Opacity = 0.2;
            DropZone.BorderBrush = Brushes.Cyan;
        }

   
        private void BtnEndTurn_Click(object sender, RoutedEventArgs e)
        {
            _viewModel.EndTurn();
            ShowGameMessage("Ход завершен");
        }

        private void BtnAttack_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Проверяем все возможные null
                if (_viewModel == null)
                {
                    ShowGameMessage("Ошибка: ViewModel не инициализирована");
                    return;
                }

                if (_viewModel.GameEngine == null)
                {
                    ShowGameMessage("Ошибка: Игра не запущена");
                    return;
                }

                if (_viewModel.SelectedPlayerCreature == null)
                {
                    ShowGameMessage("Выберите свое существо для атаки (кликните на нем)");
                    return;
                }

                if (_viewModel.SelectedOpponentCreature == null)
                {
                    ShowGameMessage("Выберите цель для атаки (кликните на существе противника)");
                    return;
                }

                // Проверяем, живы ли существа
                if (!_viewModel.SelectedPlayerCreature.IsAlive)
                {
                    ShowGameMessage("Выбранное существо мертво!");
                    _viewModel.SelectedPlayerCreature = null;
                    return;
                }

                if (!_viewModel.SelectedOpponentCreature.IsAlive)
                {
                    ShowGameMessage("Цель уже мертва!");
                    _viewModel.SelectedOpponentCreature = null;
                    return;
                }

                // Проверяем, может ли существо атаковать
                if (_viewModel.SelectedPlayerCreature.State != CreatureState.Active)
                {
                    ShowGameMessage("Это существо не может атаковать сейчас!");
                    return;
                }

                // Выполняем атаку
                bool success = _viewModel.GameEngine.AttackWithCreature(
                    _viewModel.SelectedPlayerCreature,
                    _viewModel.SelectedOpponentCreature);

                if (success)
                {
                    ShowGameMessage($"{_viewModel.SelectedPlayerCreature.Name} атакует {_viewModel.SelectedOpponentCreature.Name}!");

                    // Обновляем интерфейс
                    _viewModel.UpdateAll();

                    // Сбрасываем выбор
                    _viewModel.SelectedPlayerCreature = null;
                    _viewModel.SelectedOpponentCreature = null;
                }
                else
                {
                    ShowGameMessage("Атака не удалась!");
                }
            }
            catch (Exception ex)
            {
                ShowGameMessage($"Ошибка атаки: {ex.Message}");
                // Для отладки можно добавить
                // MessageBox.Show(ex.ToString());
            }
        }
        private void BtnPlayCard_Click(object sender, RoutedEventArgs e)
        {
            if (_viewModel.SelectedHandCard == null)
            {
                ShowGameMessage("Выберите карту для игры");
                return;
            }

            // Проверяем тип карты
            if (_viewModel.SelectedHandCard is ICreatureCard creature)
            {
                ShowGameMessage("Перетащите карту существа на поле");
            }
            else if (_viewModel.SelectedHandCard is ISpellCard spell)
            {
                PlaySpellCard(spell);
            }
            else if (_viewModel.SelectedHandCard is IArtifactCard artifact)
            {
                PlayArtifactCard(artifact);
            }
        }

        private void PlayArtifactCard(IArtifactCard artifact)
        {
            try
            {
                bool success = _viewModel.GameEngine.PlayArtifactCard(artifact);
                if (success)
                {
                    ShowGameMessage($"Артефакт {artifact.Name} активирован!");
                    _viewModel.UpdateAll();
                    _viewModel.SelectedHandCard = null;
                }
                else
                {
                    ShowGameMessage("Не удалось активировать артефакт");
                }
            }
            catch (Exception ex)
            {
                ShowGameMessage($"Ошибка: {ex.Message}");
            }
        }

        private void PlaySpellCard(ISpellCard spell)
        {
            if (spell.TargetType == "SingleTarget")
            {
                // Проверяем, выбрано ли существо
                ICreatureCard target = null;

                if (spell.Subtype == SpellSubtype.Healing || spell.Subtype == SpellSubtype.Buff)
                {
                    target = _viewModel.SelectedPlayerCreature;
                    if (target == null)
                    {
                        ShowGameMessage($"Выберите свое существо для {spell.Name}");
                        return;
                    }
                }
                else if (spell.Subtype == SpellSubtype.Attack)
                {
                    target = _viewModel.SelectedOpponentCreature;
                    if (target == null)
                    {
                        ShowGameMessage($"Выберите существо противника для {spell.Name}");
                        return;
                    }
                }

                // Применяем заклинание
                bool success = _viewModel.GameEngine.PlaySpellCard(spell, target);
                if (success)
                {
                    ShowGameMessage($"Заклинание {spell.Name} применено к {target.Name}");
                    _viewModel.UpdateAll();

                    // Сбрасываем выбор
                    _viewModel.SelectedPlayerCreature = null;
                    _viewModel.SelectedOpponentCreature = null;
                    _viewModel.SelectedHandCard = null;
                }
            }
            else
            {
                // Массовые заклинания
                bool success = _viewModel.GameEngine.PlaySpellCard(spell, null);
                if (success)
                {
                    ShowGameMessage($"Заклинание {spell.Name} применено!");
                    _viewModel.UpdateAll();
                    _viewModel.SelectedHandCard = null;
                }
            }
        }
    
        // МЕТОДЫ ДЛЯ ОТОБРАЖЕНИЯ СООБЩЕНИЙ
        private void ShowGameMessage(string message)
        {
            // Проверяем, нет ли уже сообщения
            foreach (var child in MainGrid.Children)
            {
                if (child is Grid grid && grid.Name == "MessageOverlay")
                {
                    MainGrid.Children.Remove(grid);
                    break;
                }
            }

            // Создаем overlay
            var overlay = new Grid
            {
                Name = "MessageOverlay",
                Background = new SolidColorBrush(Color.FromArgb(150, 0, 0, 0)),
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Stretch
            };

            // Создаем сообщение
            var messageBox = new Border
            {
                Background = new SolidColorBrush(Color.FromArgb(230, 16, 26, 36)),
                BorderBrush = new SolidColorBrush(Colors.Cyan),
                BorderThickness = new Thickness(2),
                CornerRadius = new CornerRadius(10),
                Width = 400,
                Height = 150,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                Effect = new System.Windows.Media.Effects.DropShadowEffect
                {
                    Color = Colors.Cyan,
                    BlurRadius = 20,
                    Opacity = 0.7
                }
            };

            var stackPanel = new StackPanel
            {
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(20)
            };

            var textBlock = new TextBlock
            {
                Text = message,
                Foreground = Brushes.White,
                FontSize = 18,
                FontWeight = FontWeights.Bold,
                TextWrapping = TextWrapping.Wrap,
                HorizontalAlignment = HorizontalAlignment.Center,
                TextAlignment = TextAlignment.Center
            };

            stackPanel.Children.Add(textBlock);
            messageBox.Child = stackPanel;
            overlay.Children.Add(messageBox);

            // Добавляем на главную сетку
            MainGrid.Children.Add(overlay);
            Panel.SetZIndex(overlay, 10001);

            // Автоматическое скрытие через 3 секунды
            var timer = new System.Windows.Threading.DispatcherTimer();
            timer.Interval = TimeSpan.FromSeconds(0.5);
            timer.Tick += (s, args) =>
            {
                timer.Stop();
                if (MainGrid.Children.Contains(overlay))
                {
                    MainGrid.Children.Remove(overlay);
                }
            };
            timer.Start();
        }

        // ПРОКРУТКА РУКИ
        private void ScrollLeft_Click(object sender, RoutedEventArgs e)
        {
            if (HandScrollViewer != null)
            {
                HandScrollViewer.ScrollToHorizontalOffset(HandScrollViewer.HorizontalOffset - 150);
            }
        }

        private void BtnSaveGame_Click(object sender, RoutedEventArgs e)
        {
            
        }
        private void ScrollRight_Click(object sender, RoutedEventArgs e)
        {
            if (HandScrollViewer != null)
            {
                HandScrollViewer.ScrollToHorizontalOffset(HandScrollViewer.HorizontalOffset + 150);
            }
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
    }
}