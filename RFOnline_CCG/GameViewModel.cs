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

        public JsonGameStateService GameStateService => _gameStateService ??= new JsonGameStateService();

        // Коллекции для привязки
        public ObservableCollection<IArtifactCard> PlayerArtifacts { get; } = new ObservableCollection<IArtifactCard>();
        public ObservableCollection<ICard> PlayerHand { get; } = new ObservableCollection<ICard>();
        public ObservableCollection<ICreatureCard> PlayerField { get; } = new ObservableCollection<ICreatureCard>();
        public ObservableCollection<ICreatureCard> OpponentField { get; } = new ObservableCollection<ICreatureCard>();
        public ObservableCollection<string> GameLog { get; } = new ObservableCollection<string>();

        // Свойства для привязки
        public string PlayerName { get; set; }
        public int PlayerHealth { get; set; }
        public int PlayerEnergy { get; set; }
        public int PlayerMaxEnergy { get; set; }
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
        public int PlayerDeckCount { get; set; }

        public string OpponentName { get; set; }
        public int OpponentHealth { get; set; }
        public int OpponentEnergy { get; set; }
        public int OpponentDeckCount { get; set; }

        public int CurrentTurn { get; set; }
        public string TurnInfo => $"Ход {CurrentTurn} • {PlayerName}";

        // Выбранные карты
        public ICard SelectedHandCard { get; set; }
        public ICreatureCard SelectedPlayerCreature { get; set; }
        public ICreatureCard SelectedOpponentCreature { get; set; }

        public GameViewModel()
        {
            // Создаем папку для сохранений при запуске
            if (!Directory.Exists("Saves"))
            {
                Directory.CreateDirectory("Saves");
            }
        }

        public void StartNewGame(string player1Name, Faction player1Faction,
                               string player2Name, Faction player2Faction)
        {
            GameEngine = new GameEngine(player1Name, player1Faction, player2Name, player2Faction);
            GameEngine.StartGame();
            UpdateAll();
        }

        public bool LoadGame(string saveFilePath)
        {
            try
            {
                if (string.IsNullOrEmpty(saveFilePath) || !File.Exists(saveFilePath))
                {
                    MessageBox.Show($"Файл сохранения не найден: {saveFilePath}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    return false;
                }

                // Загружаем состояние
                var gameState = GameStateService.LoadGameState(saveFilePath);

                // Создаем новую игру на основе загруженного состояния
                GameEngine = new GameEngine(
                    gameState.Player1Name,
                    gameState.Player1Faction,
                    gameState.Player2Name,
                    gameState.Player2Faction);

                // Загружаем состояние в движок
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
                        // Пропускаем поврежденные файлы
                        Console.WriteLine($"Ошибка чтения файла {filePath}: {ex.Message}");
                    }
                }

                // Сортируем по дате (сначала новые)
                return saveGames.OrderByDescending(s => s.Date).ToList();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка получения списка сохранений:\n{ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return saveGames;
            }
        }

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

                // Убираем запрещенные символы
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

        // Игровые действия
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
                // TODO: Реализовать выбор цели
                MessageBox.Show("Выберите цель для заклинания");
            }
        }

        public bool AttackWithSelected()
        {
            if (SelectedPlayerCreature == null || SelectedOpponentCreature == null) return false;

            bool success = GameEngine.AttackWithCreature(SelectedPlayerCreature, SelectedOpponentCreature);
            if (success) UpdateAll();
            return success;
        }

        public void EndTurn()
        {
            if (GameEngine != null)
            {
                GameEngine.EndTurn();
                UpdateAll();
            }
        }

        public bool CanAttackPlayerDirectly()
        {
            if (GameEngine == null || SelectedPlayerCreature == null)
                return false;

            return !GameEngine.OpponentPlayer.Field.Any(c => c.IsAlive);
        }

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

        public void UpdateAll()
        {
            if (GameEngine == null) return;

            // Обновляем данные игроков
            PlayerName = GameEngine.CurrentPlayer.Name;
            PlayerHealth = GameEngine.CurrentPlayer.Health;
            PlayerEnergy = GameEngine.CurrentPlayer.Energy;
            PlayerMaxEnergy = GameEngine.CurrentPlayer.MaxEnergy;
            PlayerDeckCount = GameEngine.CurrentPlayer.Deck?.Count ?? 0;

            OpponentName = GameEngine.OpponentPlayer.Name;
            OpponentHealth = GameEngine.OpponentPlayer.Health;
            OpponentEnergy = GameEngine.OpponentPlayer.Energy;
            OpponentDeckCount = GameEngine.OpponentPlayer.Deck?.Count ?? 0;

            CurrentTurn = GameEngine.CurrentTurn;

            // Обновляем коллекции
            UpdateCollection(PlayerHand, GameEngine.CurrentPlayer.Hand);
            UpdateCollection(PlayerArtifacts, GameEngine.CurrentPlayer.Artifacts);
            UpdateCollection(PlayerField, GameEngine.CurrentPlayer.Field);
            UpdateCollection(OpponentField, GameEngine.OpponentPlayer.Field);

            // Лог - только последние 50 сообщений
            var recentLogs = GameEngine.GameLog.TakeLast(50).ToList();
            UpdateCollection(GameLog, recentLogs);

            // Сбрасываем выбранные карты
            SelectedHandCard = null;
            SelectedPlayerCreature = null;
            SelectedOpponentCreature = null;

            OnPropertyChanged(null);
        }

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

        public void OnPropertyChanged(string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

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

        public string Factions => $"{Player1Faction} vs {Player2Faction}";
        public string Players => $"{Player1Name} vs {Player2Name}";
    }
}