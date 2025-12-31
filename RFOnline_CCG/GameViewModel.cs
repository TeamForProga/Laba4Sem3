using RFCardGame.Core;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Windows;

namespace RFOnline_CCG
{
    public class GameViewModel : INotifyPropertyChanged
    {
        private GameEngine _gameEngine;
        private JsonGameStateService _gameStateService;

        public event PropertyChangedEventHandler PropertyChanged;

        // Основной игровой движок
        public GameEngine GameEngine
        {
            get => _gameEngine;
            set
            {
                _gameEngine = value;
                OnPropertyChanged(nameof(GameEngine));
                UpdateAll();
            }
        }

        // Сервис для сохранения/загрузки состояния игры
        public JsonGameStateService GameStateService => _gameStateService ??= new JsonGameStateService();

        // Коллекции для привязки данных к UI
        public ObservableCollection<IArtifactCard> PlayerArtifacts { get; } = new ObservableCollection<IArtifactCard>();
        public ObservableCollection<ICard> PlayerHand { get; } = new ObservableCollection<ICard>();
        public ObservableCollection<ICreatureCard> PlayerField { get; } = new ObservableCollection<ICreatureCard>();
        public ObservableCollection<ICreatureCard> OpponentField { get; } = new ObservableCollection<ICreatureCard>();
        public ObservableCollection<string> GameLog { get; } = new ObservableCollection<string>();

        // Свойства текущего игрока для привязки
        public string PlayerName { get; set; }
        public int PlayerHealth { get; set; }
        public int PlayerEnergy { get; set; }
        public int PlayerMaxEnergy { get; set; }

        // Информация об атаке для отображения в UI
        public string AttackInfo
        {
            get
            {
                if (SelectedPlayerCreature == null)
                    return "Выберите существо для атаки";

                if (SelectedOpponentCreature == null)
                    return $"Готово: {SelectedPlayerCreature.Name} → выберите цель";

                return $"Атака: {SelectedPlayerCreature.Name} → {SelectedOpponentCreature.Name}";
            }
        }

        // Информация о возможности прямой атаки
        public string DirectAttackInfo
        {
            get
            {
                if (SelectedPlayerCreature == null || GameEngine == null)
                    return "";

                if (GameEngine.OpponentPlayer.GetAliveCreatureCount() == 0)
                    return $"✓ Можно атаковать игрока напрямую!";

                return $"Существ противника: {GameEngine.OpponentPlayer.GetAliveCreatureCount()}";
            }
        }

        // Количество карт в колоде игрока
        public int PlayerDeckCount { get; set; }

        // Свойства противника для привязки
        public string OpponentName { get; set; }
        public int OpponentHealth { get; set; }
        public int OpponentEnergy { get; set; }
        public int OpponentDeckCount { get; set; }

        // Информация о текущем ходе
        public int CurrentTurn { get; set; }
        public string TurnInfo => $"Ход {CurrentTurn} • {PlayerName}";

        // Выбранные карты для взаимодействия
        public ICard SelectedHandCard { get; set; }
        public ICreatureCard SelectedPlayerCreature { get; set; }
        public ICreatureCard SelectedOpponentCreature { get; set; }

        public GameViewModel()
        {
            // Создание папки для сохранений при инициализации ViewModel
            if (!Directory.Exists("Saves"))
            {
                Directory.CreateDirectory("Saves");
            }
        }

        // Создание новой игры с заданными параметрами
        public void StartNewGame(string player1Name, Faction player1Faction,
                               string player2Name, Faction player2Faction)
        {
            GameEngine = new GameEngine(player1Name, player1Faction, player2Name, player2Faction);
            GameEngine.StartGame();
            UpdateAll();
        }

        // Загрузка игры из файла сохранения
        public bool LoadGame(string saveFilePath)
        {
            try
            {
                if (string.IsNullOrEmpty(saveFilePath) || !File.Exists(saveFilePath))
                {
                    MessageBox.Show($"Файл сохранения не найден: {saveFilePath}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    return false;
                }

                // Загрузка состояния игры из файла
                var gameState = GameStateService.LoadGameState(saveFilePath);

                // Создание нового игрового движка с параметрами из сохранения
                GameEngine = new GameEngine(
                    gameState.Player1Name,
                    gameState.Player1Faction,
                    gameState.Player2Name,
                    gameState.Player2Faction);

                // Загрузка полного состояния в движок
                GameEngine.LoadFromState(gameState, GameStateService);

                UpdateAll();
                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки игры:\n{ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
        }

        // Получение списка доступных сохранений
        public List<SaveGameInfo> GetSaveGames()
        {
            var saveGames = new List<SaveGameInfo>();

            try
            {
                if (!Directory.Exists("Saves"))
                    return saveGames;

                var saveFiles = Directory.GetFiles("Saves", "*.json");

                foreach (var filePath in saveFiles)
                {
                    try
                    {
                        var fileInfo = new FileInfo(filePath);
                        var gameState = GameStateService.LoadGameState(filePath);

                        saveGames.Add(new SaveGameInfo
                        {
                            FilePath = filePath,
                            SaveName = Path.GetFileNameWithoutExtension(filePath),
                            Date = fileInfo.LastWriteTime,
                            Player1Name = gameState.Player1Name,
                            Player2Name = gameState.Player2Name,
                            Player1Faction = gameState.Player1Faction,
                            Player2Faction = gameState.Player2Faction,
                            CurrentTurn = gameState.CurrentTurn,
                            SaveDate = gameState.SaveDate
                        });
                    }
                    catch (Exception ex)
                    {
                        // Пропуск поврежденных файлов сохранений
                        Console.WriteLine($"Ошибка чтения файла {filePath}: {ex.Message}");
                    }
                }

                // Сортировка по дате (сначала новые сохранения)
                return saveGames.OrderByDescending(s => s.Date).ToList();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка получения списка сохранений:\n{ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return saveGames;
            }
        }

        // Сохранение текущей игры
        public void SaveGame(string saveName)
        {
            try
            {
                if (GameEngine == null)
                {
                    MessageBox.Show("Игра не запущена!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                if (string.IsNullOrWhiteSpace(saveName))
                {
                    MessageBox.Show("Введите имя сохранения!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                // Удаление запрещенных символов из имени файла
                foreach (var c in Path.GetInvalidFileNameChars())
                {
                    saveName = saveName.Replace(c.ToString(), "");
                }

                string fileName = $"Saves/{saveName}.json";
                var gameState = GameEngine.CreateGameState();
                GameStateService.SaveGameState(fileName, gameState);

            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка сохранения:\n{ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // Розыгрыш выбранной карты
        public void PlaySelectedCard()
        {
            if (SelectedHandCard == null) return;

            if (SelectedHandCard is ICreatureCard creature)
            {
                bool success = GameEngine.PlayCreatureCard(creature);
                if (success) UpdateAll();
            }
            else if (SelectedHandCard is ISpellCard spell)
            {
                // TODO: Реализовать выбор цели для заклинаний
                MessageBox.Show("Выберите цель для заклинания");
            }
        }

        // Атака выбранным существом выбранной цели
        public bool AttackWithSelected()
        {
            if (SelectedPlayerCreature == null || SelectedOpponentCreature == null) return false;

            bool success = GameEngine.AttackWithCreature(SelectedPlayerCreature, SelectedOpponentCreature);
            if (success) UpdateAll();
            return success;
        }

        // Завершение текущего хода
        public void EndTurn()
        {
            if (GameEngine != null)
            {
                GameEngine.EndTurn();
                UpdateAll();
            }
        }

        // Проверка возможности прямой атаки игрока
        public bool CanAttackPlayerDirectly()
        {
            if (GameEngine == null || SelectedPlayerCreature == null)
                return false;

            return !GameEngine.OpponentPlayer.Field.Any(c => c.IsAlive);
        }

        // Выполнение прямой атаки игрока
        public void AttackPlayerDirectly()
        {
            if (!CanAttackPlayerDirectly() || SelectedPlayerCreature == null)
                return;

            bool success = GameEngine.AttackPlayerDirectly(SelectedPlayerCreature);
            if (success)
            {
                UpdateAll();
                SelectedPlayerCreature = null;
                SelectedOpponentCreature = null;
            }
        }

        // Полное обновление всех данных ViewModel
        public void UpdateAll()
        {
            if (GameEngine == null) return;

            // Обновление данных текущего игрока
            PlayerName = GameEngine.CurrentPlayer.Name;
            PlayerHealth = GameEngine.CurrentPlayer.Health;
            PlayerEnergy = GameEngine.CurrentPlayer.Energy;
            PlayerMaxEnergy = GameEngine.CurrentPlayer.MaxEnergy;
            PlayerDeckCount = GameEngine.CurrentPlayer.Deck?.Count ?? 0;

            // Обновление данных противника
            OpponentName = GameEngine.OpponentPlayer.Name;
            OpponentHealth = GameEngine.OpponentPlayer.Health;
            OpponentEnergy = GameEngine.OpponentPlayer.Energy;
            OpponentDeckCount = GameEngine.OpponentPlayer.Deck?.Count ?? 0;

            CurrentTurn = GameEngine.CurrentTurn;

            // Обновление коллекций для привязки
            UpdateCollection(PlayerHand, GameEngine.CurrentPlayer.Hand);
            UpdateCollection(PlayerArtifacts, GameEngine.CurrentPlayer.Artifacts);
            UpdateCollection(PlayerField, GameEngine.CurrentPlayer.Field);
            UpdateCollection(OpponentField, GameEngine.OpponentPlayer.Field);

            // Обновление лога игры (только последние 50 сообщений)
            var recentLogs = GameEngine.GameLog.TakeLast(50).ToList();
            UpdateCollection(GameLog, recentLogs);

            // Сброс выбранных карт после обновления
            SelectedHandCard = null;
            SelectedPlayerCreature = null;
            SelectedOpponentCreature = null;

            // Уведомление об изменении всех свойств
            OnPropertyChanged(null);
        }

        // Синхронизация ObservableCollection с исходными данными
        private void UpdateCollection<T>(ObservableCollection<T> target, IEnumerable<T> source)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                target.Clear();
                if (source != null)
                {
                    foreach (var item in source)
                    {
                        target.Add(item);
                    }
                }
            });
        }

        // Вызов события изменения свойства
        public void OnPropertyChanged(string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    // Класс для хранения информации о сохранении игры
    public class SaveGameInfo
    {
        public string FilePath { get; set; }
        public string SaveName { get; set; }
        public DateTime Date { get; set; }
        public string Player1Name { get; set; }
        public string Player2Name { get; set; }
        public Faction Player1Faction { get; set; }
        public Faction Player2Faction { get; set; }
        public int CurrentTurn { get; set; }
        public DateTime SaveDate { get; set; }

        // Форматированные свойства для отображения
        public string Factions => $"{Player1Faction} vs {Player2Faction}";
        public string Players => $"{Player1Name} vs {Player2Name}";
    }
}