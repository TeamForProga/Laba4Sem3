using RFCardGame.Core;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.IO;
using System.Windows.Media.Effects;

namespace RFOnline_CCG
{
    public partial class MainWindow : Window
    {
        private GameViewModel _viewModel;
        private bool _isDragging = false;
        private Point _clickPosition;

        public MainWindow()
        {
            InitializeComponent();
            _viewModel = new GameViewModel();
            DataContext = _viewModel;
        }

        // Переключение между экранами главного меню
        private void StartGame_Click(object sender, RoutedEventArgs e)
        {
            MainMenuUI.Visibility = Visibility.Collapsed;
            NewGameScreen.Visibility = Visibility.Visible;
        }

        // Возврат к главному меню с любого экрана
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

        // Выход из приложения
        private void Exit_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        // Создание новой игры с выбранными параметрами
        private void BtnStartBoardGame_Click(object sender, RoutedEventArgs e)
        {
            // Определение фракций для игроков
            Faction player1Faction = GetSelectedFactionFromRadioButtons(
                Player1Accretia, Player1Bellato, Player1Cora);

            Faction player2Faction = GetSelectedFactionFromRadioButtons(
                Player2Accretia, Player2Bellato, Player2Cora);

            // Получение имен игроков (используем значения по умолчанию, если поле пустое)
            string player1Name = Player1NameBox.Text.Trim();
            if (string.IsNullOrWhiteSpace(player1Name))
                player1Name = "Игрок 1";

            string player2Name = Player2NameBox.Text.Trim();
            if (string.IsNullOrWhiteSpace(player2Name))
                player2Name = "Игрок 2";

            // Запуск новой игры через ViewModel
            _viewModel.StartNewGame(player1Name, player1Faction, player2Name, player2Faction);

            // Переключение на игровое поле
            NewGameScreen.Visibility = Visibility.Collapsed;
            GameBoardUI.Visibility = Visibility.Visible;
        }

        // Определение выбранной фракции из RadioButton элементов
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

        // Открытие экрана загрузки игры
        private void BtnLoadGame_Click(object sender, RoutedEventArgs e)
        {
            MainMenuUI.Visibility = Visibility.Collapsed;
            LoadGameScreen.Visibility = Visibility.Visible;

            // Загружаем список доступных сохранений
            LoadSaveGamesList();
        }

        // Загрузка списка файлов сохранений в ListBox
        private void LoadSaveGamesList()
        {
            try
            {
                SaveGamesList.Items.Clear();
                var saveGames = _viewModel.GetSaveGames();

                if (!saveGames.Any())
                {
                    // Отображение сообщения, если сохранений нет
                    SaveGamesList.Items.Add(new
                    {
                        SaveName = "Сохранений не найдено",
                        Date = "",
                        Factions = "Создайте новую игру",
                        FilePath = ""
                    });
                    return;
                }

                foreach (var save in saveGames)
                {
                    SaveGamesList.Items.Add(new
                    {
                        SaveName = save.SaveName,
                        Date = save.Date.ToString("dd.MM.yyyy HH:mm"),
                        Factions = save.Factions,
                        Players = save.Players,
                        Turn = $"Ход: {save.CurrentTurn}",
                        FilePath = save.FilePath
                    });
                }
            }
            catch (Exception ex)
            {
                ShowGameMessage($"Ошибка загрузки списка: {ex.Message}");
            }
        }

        // Обработка выбора сохранения из списка
        private void SaveGame_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (SaveGamesList.SelectedItem != null)
            {
                dynamic selectedItem = SaveGamesList.SelectedItem;
                if (selectedItem.FilePath is string filePath && !string.IsNullOrEmpty(filePath))
                {
                    // Проверка, что это не заглушка "Сохранений не найдено"
                    if (selectedItem.SaveName == "Сохранений не найдено")
                    {
                        SaveGamesList.SelectedItem = null;
                        return;
                    }

                    // Сохранение пути к выбранному файлу в Tag элемента
                    SaveGamesList.Tag = filePath;
                }
            }
        }

        // Загрузка выбранного сохранения
        private void BtnLoadGameStart_Click(object sender, RoutedEventArgs e)
        {
            if (SaveGamesList.Tag is string selectedFilePath && File.Exists(selectedFilePath))
            {
                bool success = _viewModel.LoadGame(selectedFilePath);
                if (success)
                {
                    LoadGameScreen.Visibility = Visibility.Collapsed;
                    GameBoardUI.Visibility = Visibility.Visible;
                    ShowGameMessage("Игра успешно загружена!");
                }
            }
            else
            {
                ShowGameMessage("Выберите сохранение для загрузки!");
            }
        }

        // Сохранение текущей игры
        private void BtnSaveGame_Click(object sender, RoutedEventArgs e)
        {
            if (_viewModel.GameEngine == null)
            {
                ShowGameMessage("Игра не запущена!");
                return;
            }

            var dialog = new SaveGameDialog();
            if (dialog.ShowDialog() == true && !string.IsNullOrWhiteSpace(dialog.SaveName))
            {
                _viewModel.SaveGame(dialog.SaveName);
                ShowGameMessage($"Игра сохранена: {dialog.SaveName}");
            }
        }

        // Отмена создания новой игры
        private void BtnNewGameCancel_Click(object sender, RoutedEventArgs e)
        {
            NewGameScreen.Visibility = Visibility.Collapsed;
            MainMenuUI.Visibility = Visibility.Visible;
        }

        // Отмена загрузки игры
        private void BtnLoadGameCancel_Click(object sender, RoutedEventArgs e)
        {
            LoadGameScreen.Visibility = Visibility.Collapsed;
            MainMenuUI.Visibility = Visibility.Visible;
        }

        // Отображение экрана завершения игры
        private void ShowGameOver()
        {
            if (_viewModel.GameEngine?.Winner == null) return;

            // Обновление информации о победителе
            GameOverTitle.Text = "ПОБЕДА!";
            WinnerText.Text = $"{_viewModel.GameEngine.Winner.Name} побеждает!";
            GameStats.Text = $"Ходов: {_viewModel.GameEngine.CurrentTurn}\n" +
                             $"Здоровье: {_viewModel.GameEngine.Winner.Health}";

            GameSummary.Text = $"Игра завершена. {_viewModel.GameEngine.Winner.Name} показал отличный результат!";

            // Переключение на экран завершения игры
            GameBoardUI.Visibility = Visibility.Collapsed;
            GameOverScreen.Visibility = Visibility.Visible;
        }

        // Начало новой игры после завершения предыдущей
        private void NewGameAfterEnd_Click(object sender, RoutedEventArgs e)
        {
            GameOverScreen.Visibility = Visibility.Collapsed;
            NewGameScreen.Visibility = Visibility.Visible;
        }

        // Завершение текущего хода с проверкой окончания игры
        private void BtnEndTurn_Click(object sender, RoutedEventArgs e)
        {
            _viewModel.EndTurn();
            ShowGameMessage("Ход завершен");

            // Проверка условий завершения игры
            if (_viewModel.GameEngine?.IsGameOver == true)
            {
                ShowGameOver();
            }
        }

        // Обработка клика по карте в руке для выбора и начала перетаскивания
        private void Card_MouseDown(object sender, MouseButtonEventArgs e)
        {
            var border = sender as Border;
            if (border?.Tag is ICard gameCard)
            {
                // Сброс подсветки всех карт
                ResetAllCardHighlights();

                // Выбор карты в ViewModel
                _viewModel.SelectedHandCard = gameCard;

                // Подсветка выбранной карты
                HighlightCard(border, true);

                // Принудительное обновление привязок
                _viewModel.OnPropertyChanged(nameof(_viewModel.SelectedHandCard));

                // Начало перетаскивания
                _isDragging = true;
                _clickPosition = e.GetPosition(this);
                border.CaptureMouse();

                // Создание визуального клона для перетаскивания
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

                // Добавление текста на клон
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

                // Добавление клона на Canvas
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

                // Сохранение ссылок на карту и overlay в Tag
                border.Tag = new object[] { gameCard, overlay };
            }
        }

        // Подсветка карты в зависимости от состояния выбора
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

        // Сброс подсветки всех карт в руке
        private void ResetAllCardHighlights()
        {
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
        }

        // Рекурсивный поиск дочернего элемента определенного типа
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

        // Обработка клика по существу для выбора цели
        private void Creature_MouseDown(object sender, MouseButtonEventArgs e)
        {
            var border = sender as Border;
            if (border?.Tag is ICreatureCard gameCreature)
            {
                // Сброс подсветки всех существ
                ResetAllCreatureHighlights();

                // Определение принадлежности существа
                bool isPlayerCreature = _viewModel.PlayerField.Contains(gameCreature);
                bool isOpponentCreature = _viewModel.OpponentField.Contains(gameCreature);

                if (isPlayerCreature)
                {
                    _viewModel.SelectedPlayerCreature = gameCreature;
                    HighlightCreature(border, "Cyan");

                    // Проверка возможности прямой атаки игрока
                    if (_viewModel.GameEngine.OpponentPlayer.GetAliveCreatureCount() == 0)
                    {
                        ShowGameMessage($"Выбрано: {gameCreature.Name} ✓ Можно атаковать игрока!");
                    }
                    else
                    {
                        ShowGameMessage($"Выбрано: {gameCreature.Name} - выберите цель для атаки");
                    }
                }
                else if (isOpponentCreature)
                {
                    _viewModel.SelectedOpponentCreature = gameCreature;
                    HighlightCreature(border, "Red");
                    ShowGameMessage($"Цель: {gameCreature.Name}");
                }

                // Обновление привязок
                _viewModel.OnPropertyChanged(nameof(_viewModel.SelectedPlayerCreature));
                _viewModel.OnPropertyChanged(nameof(_viewModel.SelectedOpponentCreature));
            }
        }

        // Подсветка существа в зависимости от принадлежности
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

        // Сброс подсветки всех существ (заглушка для реализации)
        private void ResetAllCreatureHighlights()
        {
            // TODO: Реализовать сброс подсветки существ на полях
        }

        // Очистка всех выбранных элементов в ViewModel
        public void ClearSelection()
        {
            _viewModel.SelectedHandCard = null;
            _viewModel.SelectedPlayerCreature = null;
            _viewModel.SelectedOpponentCreature = null;

            _viewModel.OnPropertyChanged(nameof(_viewModel.SelectedHandCard));
            _viewModel.OnPropertyChanged(nameof(_viewModel.SelectedPlayerCreature));
            _viewModel.OnPropertyChanged(nameof(_viewModel.SelectedOpponentCreature));
        }

        // Обработка движения мыши при перетаскивании карты
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
                    // Обновление позиции клона карты
                    Canvas.SetLeft(clone, currentPosition.X - 60);
                    Canvas.SetTop(clone, currentPosition.Y - 80);
                }
            }

            // Визуальная подсветка зоны сброса при наведении
            Point dropPoint = e.GetPosition(DropZone);
            bool isOver = dropPoint.X >= 0 && dropPoint.X <= DropZone.ActualWidth &&
                          dropPoint.Y >= 0 && dropPoint.Y <= DropZone.ActualHeight;

            DropZone.Opacity = isOver ? 0.6 : 0.2;
            DropZone.BorderBrush = isOver ? Brushes.Lime : Brushes.Cyan;
        }

        // Обработка атаки выбранного существа по цели
        private void BtnAttack_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Проверка инициализации ViewModel
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

                // Проверка состояния существ
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

                // Проверка возможности атаки
                if (_viewModel.SelectedPlayerCreature.State != CreatureState.Active)
                {
                    ShowGameMessage("Это существо не может атаковать сейчас!");
                    return;
                }

                // Выполнение атаки через игровой движок
                bool success = _viewModel.GameEngine.AttackWithCreature(
                    _viewModel.SelectedPlayerCreature,
                    _viewModel.SelectedOpponentCreature);

                if (success)
                {
                    ShowGameMessage($"{_viewModel.SelectedPlayerCreature.Name} атакует {_viewModel.SelectedOpponentCreature.Name}!");

                    // Обновление интерфейса
                    _viewModel.UpdateAll();

                    // Сброс выбора
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
            }
        }

        // Обработка кнопки розыгрыша выбранной карты
        private void BtnPlayCard_Click(object sender, RoutedEventArgs e)
        {
            if (_viewModel.SelectedHandCard == null)
            {
                ShowGameMessage("Выберите карту для игры");
                return;
            }

            // Определение типа карты и соответствующее действие
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

        // Розыгрыш карты артефакта
        private void PlayArtifactCard(IArtifactCard artifact)
        {
            try
            {
                // Проверка доступной энергии
                if (artifact.Cost > _viewModel.PlayerEnergy)
                {
                    ShowGameMessage($"Недостаточно энергии! Нужно: {artifact.Cost}");
                    return;
                }

                // Проверка лимита артефактов
                if (_viewModel.PlayerArtifacts.Count >= 5) // Максимум 5 артефактов
                {
                    ShowGameMessage("Слишком много артефактов! Максимум 5.");
                    return;
                }

                bool success = _viewModel.GameEngine.PlayArtifactCard(artifact);
                if (success)
                {
                    ShowGameMessage($"Артефакт {artifact.Name} активирован!");
                    _viewModel.UpdateAll();

                    // Сброс выделения карты
                    _viewModel.SelectedHandCard = null;
                    _viewModel.OnPropertyChanged(nameof(_viewModel.SelectedHandCard));
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

        // Обработка отпускания кнопки мыши после перетаскивания
        private void Card_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (!_isDragging) return;

            var border = sender as Border;
            _isDragging = false;
            border?.ReleaseMouseCapture();

            if (border?.Tag is object[] data)
            {
                var gameCard = data[0] as ICard;
                var overlay = data[1] as Grid;

                // Удаление overlay с клоном карты
                if (overlay != null && MainGrid.Children.Contains(overlay))
                {
                    MainGrid.Children.Remove(overlay);
                }

                // Проверка, находится ли точка сброса в зоне игры
                Point dropPoint = e.GetPosition(DropZone);
                bool isInside = dropPoint.X >= 0 && dropPoint.X <= DropZone.ActualWidth &&
                                dropPoint.Y >= 0 && dropPoint.Y <= DropZone.ActualHeight;

                // Обработка разных типов карт
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
                    // Обработка заклинаний
                    HandleSpellCard(spell);
                }
                else if (isInside && gameCard is IArtifactCard artifact)
                {
                    // Активация артефакта
                    PlayArtifactCard(artifact);
                    _viewModel.UpdateAll();
                }

                // Восстановление исходного Tag
                border.Tag = gameCard;
            }

            // Сброс визуального состояния зоны сброса
            DropZone.Opacity = 0.2;
            DropZone.BorderBrush = Brushes.Cyan;
        }

        // Розыгрыш карты заклинания
        private void PlaySpellCard(ISpellCard spell)
        {
            // Обработка целевых заклинаний
            if (spell.TargetType == "SingleTarget")
            {
                ICreatureCard target = null;

                // Определение цели в зависимости от типа заклинания
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

                // Применение заклинания
                bool success = _viewModel.GameEngine.PlaySpellCard(spell, target);
                if (success)
                {
                    ShowGameMessage($"Заклинание {spell.Name} применено к {target.Name}");
                    _viewModel.UpdateAll();

                    // Сброс выбора
                    _viewModel.SelectedPlayerCreature = null;
                    _viewModel.SelectedOpponentCreature = null;
                    _viewModel.SelectedHandCard = null;
                }
            }
            else
            {
                // Обработка массовых заклинаний
                bool success = _viewModel.GameEngine.PlaySpellCard(spell, null);
                if (success)
                {
                    ShowGameMessage($"Заклинание {spell.Name} применено!");
                    _viewModel.UpdateAll();
                    _viewModel.SelectedHandCard = null;
                }
            }
        }

        // Обработка прямой атаки игрока
        private void BtnDirectAttack_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (_viewModel == null || _viewModel.GameEngine == null)
                {
                    ShowGameMessage("Игра не запущена");
                    return;
                }

                if (_viewModel.SelectedPlayerCreature == null)
                {
                    ShowGameMessage("Выберите свое существо для атаки");
                    return;
                }

                // Проверка наличия существ у противника
                if (_viewModel.GameEngine.OpponentPlayer.GetAliveCreatureCount() > 0)
                {
                    ShowGameMessage("Сначала уничтожьте всех существ противника!");
                    return;
                }

                // Проверка состояния существа
                if (!_viewModel.SelectedPlayerCreature.IsAlive)
                {
                    ShowGameMessage("Выбранное существо мертво!");
                    _viewModel.SelectedPlayerCreature = null;
                    return;
                }

                if (_viewModel.SelectedPlayerCreature.State != CreatureState.Active)
                {
                    ShowGameMessage("Это существо не может атаковать сейчас!");
                    return;
                }

                // Выполнение прямой атаки
                bool success = _viewModel.GameEngine.AttackPlayerDirectly(
                    _viewModel.SelectedPlayerCreature);

                if (success)
                {
                    ShowGameMessage($"{_viewModel.SelectedPlayerCreature.Name} атакует {_viewModel.GameEngine.OpponentPlayer.Name}!");
                    _viewModel.UpdateAll();
                    _viewModel.SelectedPlayerCreature = null;
                }
                else
                {
                    ShowGameMessage("Прямая атака не удалась!");
                }
            }
            catch (Exception ex)
            {
                ShowGameMessage($"Ошибка: {ex.Message}");
            }
        }

        // Обработка карты заклинания при перетаскивании
        private void HandleSpellCard(ISpellCard spell)
        {
            // Обработка целевых заклинаний
            if (spell.TargetType == "SingleTarget")
            {
                if (spell.Subtype == SpellSubtype.Healing || spell.Subtype == SpellSubtype.Buff)
                {
                    // Заклинания лечения/усиления требуют выбора своего существа
                    if (_viewModel.SelectedPlayerCreature == null)
                    {
                        ShowGameMessage($"Выберите свое существо для {spell.Name}");
                        return;
                    }

                    bool success = _viewModel.GameEngine.PlaySpellCard(spell, _viewModel.SelectedPlayerCreature);
                    if (success)
                    {
                        ShowGameMessage($"{spell.Name} применено к {_viewModel.SelectedPlayerCreature.Name}");
                        _viewModel.UpdateAll();
                        _viewModel.SelectedPlayerCreature = null;
                    }
                }
                else if (spell.Subtype == SpellSubtype.Attack)
                {
                    // Атакующие заклинания требуют выбора существа противника
                    if (_viewModel.SelectedOpponentCreature == null)
                    {
                        ShowGameMessage($"Выберите существо противника для {spell.Name}");
                        return;
                    }

                    bool success = _viewModel.GameEngine.PlaySpellCard(spell, _viewModel.SelectedOpponentCreature);
                    if (success)
                    {
                        ShowGameMessage($"{spell.Name} наносит урон {_viewModel.SelectedOpponentCreature.Name}");
                        _viewModel.UpdateAll();
                        _viewModel.SelectedOpponentCreature = null;
                    }
                }
            }
            else
            {
                // Массовые заклинания применяются без цели
                bool success = _viewModel.GameEngine.PlaySpellCard(spell, null);
                if (success)
                {
                    ShowGameMessage($"{spell.Name} применено!");
                    _viewModel.UpdateAll();
                }
            }
        }

        // Отображение временного сообщения в центре экрана
        private void ShowGameMessage(string message)
        {
            // Удаление предыдущего сообщения, если оно есть
            foreach (var child in MainGrid.Children)
            {
                if (child is Grid grid && grid.Name == "MessageOverlay")
                {
                    MainGrid.Children.Remove(grid);
                    break;
                }
            }

            // Создание overlay для сообщения
            var overlay = new Grid
            {
                Name = "MessageOverlay",
                Background = new SolidColorBrush(Color.FromArgb(150, 0, 0, 0)),
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Stretch
            };

            // Создание стилизованного контейнера сообщения
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

            // Добавление сообщения на главную сетку
            MainGrid.Children.Add(overlay);
            Panel.SetZIndex(overlay, 10001);

            // Автоматическое скрытие сообщения через 0.5 секунды
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

        // Прокрутка руки влево
        private void ScrollLeft_Click(object sender, RoutedEventArgs e)
        {
            if (HandScrollViewer != null)
            {
                HandScrollViewer.ScrollToHorizontalOffset(HandScrollViewer.HorizontalOffset - 150);
            }
        }

        // Прокрутка руки вправо
        private void ScrollRight_Click(object sender, RoutedEventArgs e)
        {
            if (HandScrollViewer != null)
            {
                HandScrollViewer.ScrollToHorizontalOffset(HandScrollViewer.HorizontalOffset + 150);
            }
        }
    }
}