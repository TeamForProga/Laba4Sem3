using RFCardGame.Core;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Collections.Generic;

namespace RFOnline_CCG.ViewModels
{
    public class GameViewModel : BaseViewModel
    {
        private EnhancedGameController _gameController;
        private GameEngine _gameEngine;
        private System.Windows.Threading.DispatcherTimer _gameTimer;
        private DateTime _gameStartTime;

        // Состояния видимости
        private Visibility _mainMenuVisibility = Visibility.Visible;
        private Visibility _gameBoardVisibility = Visibility.Collapsed;
        private Visibility _newGameScreenVisibility = Visibility.Collapsed;
        private Visibility _loadGameScreenVisibility = Visibility.Collapsed;
        private Visibility _pauseMenuVisibility = Visibility.Collapsed;

        // Выбранные фракции для новой игры
        private Faction _player1Faction = Faction.Accretia;
        private Faction _player2Faction = Faction.Bellato;
        private string _player1Name = "Игрок 1";
        private string _player2Name = "Игрок 2";

        // Коллекции
        public ObservableCollection<CardViewModel> PlayerHand { get; } = new ObservableCollection<CardViewModel>();
        public ObservableCollection<CreatureViewModel> PlayerField { get; } = new ObservableCollection<CreatureViewModel>();
        public ObservableCollection<CreatureViewModel> EnemyField { get; } = new ObservableCollection<CreatureViewModel>();
        public ObservableCollection<SaveGameViewModel> SaveGames { get; } = new ObservableCollection<SaveGameViewModel>();

        // Свойства для отображения
        private string _playerHealthText = "30 HP";
        private string _enemyHealthText = "30 HP";
        private string _energyText = "0/0";
        private string _gameTimeText = "00:00";
        private string _deckCountText = "30";
        private string _turnText = "Ход: 1";
        private string _currentPlayerText = "Текущий игрок:";
        private string _gameLogText = "";
        private CardViewModel _selectedCard;
        private CreatureViewModel _selectedCreature;
        private SaveGameViewModel _selectedSaveGame;

        public Visibility MainMenuVisibility
        {
            get => _mainMenuVisibility;
            set => SetField(ref _mainMenuVisibility, value);
        }

        public Visibility GameBoardVisibility
        {
            get => _gameBoardVisibility;
            set => SetField(ref _gameBoardVisibility, value);
        }

        public Visibility NewGameScreenVisibility
        {
            get => _newGameScreenVisibility;
            set => SetField(ref _newGameScreenVisibility, value);
        }

        public Visibility LoadGameScreenVisibility
        {
            get => _loadGameScreenVisibility;
            set => SetField(ref _loadGameScreenVisibility, value);
        }

        public Visibility PauseMenuVisibility
        {
            get => _pauseMenuVisibility;
            set => SetField(ref _pauseMenuVisibility, value);
        }

        public Faction Player1Faction
        {
            get => _player1Faction;
            set => SetField(ref _player1Faction, value);
        }

        public Faction Player2Faction
        {
            get => _player2Faction;
            set => SetField(ref _player2Faction, value);
        }

        public string Player1Name
        {
            get => _player1Name;
            set => SetField(ref _player1Name, value);
        }

        public string Player2Name
        {
            get => _player2Name;
            set => SetField(ref _player2Name, value);
        }

        public string PlayerHealthText
        {
            get => _playerHealthText;
            set => SetField(ref _playerHealthText, value);
        }

        public string EnemyHealthText
        {
            get => _enemyHealthText;
            set => SetField(ref _enemyHealthText, value);
        }

        public string EnergyText
        {
            get => _energyText;
            set => SetField(ref _energyText, value);
        }

        public string GameTimeText
        {
            get => _gameTimeText;
            set => SetField(ref _gameTimeText, value);
        }

        public string DeckCountText
        {
            get => _deckCountText;
            set => SetField(ref _deckCountText, value);
        }

        public string TurnText
        {
            get => _turnText;
            set => SetField(ref _turnText, value);
        }

        public string CurrentPlayerText
        {
            get => _currentPlayerText;
            set => SetField(ref _currentPlayerText, value);
        }

        public string GameLogText
        {
            get => _gameLogText;
            set => SetField(ref _gameLogText, value);
        }

        public CardViewModel SelectedCard
        {
            get => _selectedCard;
            set => SetField(ref _selectedCard, value);
        }

        public CreatureViewModel SelectedCreature
        {
            get => _selectedCreature;
            set => SetField(ref _selectedCreature, value);
        }

        public SaveGameViewModel SelectedSaveGame
        {
            get => _selectedSaveGame;
            set => SetField(ref _selectedSaveGame, value);
        }

        // Команды
        public ICommand StartGameCommand { get; }
        public ICommand LoadGameMenuCommand { get; }
        public ICommand ExitCommand { get; }
        public ICommand PlayCardCommand { get; }
        public ICommand AttackCommand { get; }
        public ICommand EndTurnCommand { get; }
        public ICommand ShowMainMenuCommand { get; }
        public ICommand ShowNewGameScreenCommand { get; }
        public ICommand ShowLoadGameScreenCommand { get; }
        public ICommand ShowPauseMenuCommand { get; }
        public ICommand ResumeGameCommand { get; }
        public ICommand SaveGameCommand { get; }
        public ICommand LoadSelectedGameCommand { get; }
        public ICommand StartBoardGameCommand { get; }
        public ICommand CancelNewGameCommand { get; }
        public ICommand CancelLoadGameCommand { get; }
        public ICommand SelectFactionCommand { get; }

        public GameViewModel()
        {
            _gameController = new EnhancedGameController();

            // Инициализация команд
            StartGameCommand = new RelayCommand(ShowNewGameScreen);
            LoadGameMenuCommand = new RelayCommand(ShowLoadGameScreen);
            ExitCommand = new RelayCommand(() => Application.Current.Shutdown());
            PlayCardCommand = new RelayCommand(PlaySelectedCard);
            AttackCommand = new RelayCommand(AttackWithSelectedCreature);
            EndTurnCommand = new RelayCommand(EndTurn);
            ShowMainMenuCommand = new RelayCommand(ShowMainMenu);
            ShowNewGameScreenCommand = new RelayCommand(ShowNewGameScreen);
            ShowLoadGameScreenCommand = new RelayCommand(ShowLoadGameScreen);
            ShowPauseMenuCommand = new RelayCommand(ShowPauseMenu);
            ResumeGameCommand = new RelayCommand(ResumeGame);
            SaveGameCommand = new RelayCommand(SaveCurrentGame);
            LoadSelectedGameCommand = new RelayCommand(LoadSelectedGame);
            StartBoardGameCommand = new RelayCommand(StartBoardGame);
            CancelNewGameCommand = new RelayCommand(CancelNewGame);
            CancelLoadGameCommand = new RelayCommand(CancelLoadGame);
            SelectFactionCommand = new RelayCommand<string>(SelectFaction);

            // Инициализация таймера
            _gameTimer = new System.Windows.Threading.DispatcherTimer();
            _gameTimer.Interval = TimeSpan.FromSeconds(1);
            _gameTimer.Tick += GameTimer_Tick;
        }

        private void GameTimer_Tick(object sender, EventArgs e)
        {
            if (_gameStartTime != default)
            {
                var elapsed = DateTime.Now - _gameStartTime;
                GameTimeText = $"{elapsed.Hours:D2}:{elapsed.Minutes:D2}:{elapsed.Seconds:D2}";
            }
        }

        private void ShowMainMenu()
        {
            MainMenuVisibility = Visibility.Visible;
            GameBoardVisibility = Visibility.Collapsed;
            NewGameScreenVisibility = Visibility.Collapsed;
            LoadGameScreenVisibility = Visibility.Collapsed;
            PauseMenuVisibility = Visibility.Collapsed;

            _gameTimer.Stop();
        }

        private void ShowNewGameScreen()
        {
            MainMenuVisibility = Visibility.Collapsed;
            NewGameScreenVisibility = Visibility.Visible;
        }

        private void ShowLoadGameScreen()
        {
            MainMenuVisibility = Visibility.Collapsed;
            LoadGameScreenVisibility = Visibility.Visible;
            LoadSaveGames();
        }

        private void ShowPauseMenu()
        {
            PauseMenuVisibility = Visibility.Visible;
        }

        private void ResumeGame()
        {
            PauseMenuVisibility = Visibility.Collapsed;
        }

        private void StartBoardGame()
        {
            try
            {
                _gameEngine = new GameEngine(
                    Player1Name, Player1Faction,
                    Player2Name, Player2Faction);

                _gameEngine.StartGame();

                NewGameScreenVisibility = Visibility.Collapsed;
                GameBoardVisibility = Visibility.Visible;
                PauseMenuVisibility = Visibility.Collapsed;

                _gameStartTime = DateTime.Now;
                _gameTimer.Start();

                UpdateGameState();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка начала игры: {ex.Message}");
            }
        }

        private void PlaySelectedCard()
        {
            if (SelectedCard == null || _gameEngine == null) return;

            var card = SelectedCard.GetCard();

            if (card is ICreatureCard creatureCard)
            {
                if (_gameEngine.PlayCreatureCard(creatureCard))
                {
                    UpdateGameState();
                }
            }
            else if (card is ISpellCard spellCard)
            {
                // Для заклинаний нужно выбрать цель
                ShowSpellTargetSelection(spellCard);
            }

            SelectedCard = null;
        }

        private void ShowSpellTargetSelection(ISpellCard spellCard)
        {
            // TODO: Реализовать выбор цели для заклинания
            MessageBox.Show($"Используйте контекстное меню для выбора цели заклинания {spellCard.Name}");
        }

        private void AttackWithSelectedCreature()
        {
            if (SelectedCreature == null || _gameEngine == null) return;

            var creature = SelectedCreature.GetCreature();

            // Проверяем, может ли существо атаковать
            if (!creature.IsAlive || creature.State != CreatureState.Active)
            {
                MessageBox.Show("Это существо не может атаковать сейчас!");
                return;
            }

            // TODO: Реализовать выбор цели для атаки
            if (EnemyField.Any())
            {
                // Пока атакуем первого существа противника
                var target = EnemyField.First().GetCreature();
                _gameEngine.AttackWithCreature(creature, target);
                UpdateGameState();
            }
            else
            {
                // Прямая атака игрока
                _gameEngine.AttackPlayerDirectly(creature);
                UpdateGameState();
            }

            SelectedCreature = null;
        }

        private void EndTurn()
        {
            if (_gameEngine == null) return;

            _gameEngine.EndTurn();
            UpdateGameState();

            // Проверяем окончание игры
            if (_gameEngine.IsGameOver)
            {
                ShowGameOver();
            }
        }

        private void UpdateGameState()
        {
            if (_gameEngine == null) return;

            // Обновляем текстовые свойства
            PlayerHealthText = $"{_gameEngine.CurrentPlayer.Health} HP";
            EnemyHealthText = $"{_gameEngine.OpponentPlayer.Health} HP";
            EnergyText = $"{_gameEngine.CurrentPlayer.Energy}/{_gameEngine.CurrentPlayer.MaxEnergy}";
            DeckCountText = _gameEngine.CurrentPlayer.Deck.Count.ToString();
            TurnText = $"Ход: {_gameEngine.CurrentTurn}";
            CurrentPlayerText = $"Текущий игрок: {_gameEngine.CurrentPlayer.Name}";

            // Обновляем лог
            UpdateGameLog();

            // Обновляем коллекции
            UpdateCollections();
        }

        private void UpdateCollections()
        {
            // Обновляем руку
            PlayerHand.Clear();
            foreach (var card in _gameEngine.CurrentPlayer.Hand)
            {
                PlayerHand.Add(new CardViewModel(card));
            }

            // Обновляем поле игрока
            PlayerField.Clear();
            foreach (var creature in _gameEngine.CurrentPlayer.Field)
            {
                PlayerField.Add(new CreatureViewModel(creature));
            }

            // Обновляем поле противника
            EnemyField.Clear();
            foreach (var creature in _gameEngine.OpponentPlayer.Field)
            {
                EnemyField.Add(new CreatureViewModel(creature));
            }
        }

        private void UpdateGameLog()
        {
            if (_gameEngine == null) return;

            var recentLogs = _gameEngine.GameLog.TakeLast(10).ToList();
            GameLogText = string.Join("\n", recentLogs);
        }

        private void LoadSaveGames()
        {
            SaveGames.Clear();

            try
            {
                var savesDirectory = "Saves";
                if (!Directory.Exists(savesDirectory)) return;

                var files = Directory.GetFiles(savesDirectory, "*.json");
                foreach (var file in files.OrderByDescending(f => new FileInfo(f).LastWriteTime))
                {
                    try
                    {
                        var gameState = _gameController.LoadGameState(file);
                        var saveGame = new SaveGameViewModel
                        {
                            SaveName = Path.GetFileNameWithoutExtension(file),
                            Date = new FileInfo(file).LastWriteTime.ToString("dd.MM.yyyy HH:mm"),
                            Factions = $"{gameState.Player1Faction} vs {gameState.Player2Faction}",
                            FilePath = file
                        };

                        SaveGames.Add(saveGame);
                    }
                    catch
                    {
                        // Пропускаем поврежденные сохранения
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки списка сохранений: {ex.Message}");
            }
        }

        private void SaveCurrentGame()
        {
            if (_gameEngine == null) return;

            try
            {
                var saveName = $"save_{DateTime.Now:yyyyMMdd_HHmmss}";
                var fileName = Path.Combine("Saves", $"{saveName}.json");

                var gameState = _gameEngine.CreateGameState();
                _gameController.SaveGameState(fileName, gameState);

                MessageBox.Show($"Игра сохранена как: {saveName}");
                LoadSaveGames();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка сохранения: {ex.Message}");
            }
        }

        private void LoadSelectedGame()
        {
            if (SelectedSaveGame == null) return;

            try
            {
                var gameState = _gameController.LoadGameState(SelectedSaveGame.FilePath);

                _gameEngine = new GameEngine(
                    gameState.Player1Name,
                    gameState.Player1Faction,
                    gameState.Player2Name,
                    gameState.Player2Faction);

                _gameEngine.LoadFromState(gameState, _gameController.GetStateService());

                LoadGameScreenVisibility = Visibility.Collapsed;
                GameBoardVisibility = Visibility.Visible;

                _gameStartTime = DateTime.Now;
                _gameTimer.Start();

                UpdateGameState();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки игры: {ex.Message}");
            }
        }

        private void CancelNewGame()
        {
            NewGameScreenVisibility = Visibility.Collapsed;
            MainMenuVisibility = Visibility.Visible;
        }

        private void CancelLoadGame()
        {
            LoadGameScreenVisibility = Visibility.Collapsed;
            MainMenuVisibility = Visibility.Visible;
        }

        private void SelectFaction(string parameter)
        {
            if (string.IsNullOrEmpty(parameter)) return;

            var parts = parameter.Split(':');
            if (parts.Length != 2) return;

            var player = parts[0];
            var factionStr = parts[1];

            Faction faction = factionStr switch
            {
                "Accretia" => Faction.Accretia,
                "Bellato" => Faction.Bellato,
                "Cora" => Faction.Cora,
                _ => Faction.Accretia
            };

            if (player == "Player1")
                Player1Faction = faction;
            else if (player == "Player2")
                Player2Faction = faction;
        }

        private void ShowGameOver()
        {
            if (_gameEngine == null || !_gameEngine.IsGameOver) return;

            string message = $"=== ИГРА ОКОНЧЕНА ===\n";
            message += $"Победитель: {_gameEngine.Winner?.Name ?? "Неизвестно"}!\n";
            message += $"Всего ходов: {_gameEngine.CurrentTurn}\n\n";

            var recentLogs = _gameEngine.GameLog.TakeLast(10).ToList();
            message += "Лог последних событий:\n";
            message += string.Join("\n", recentLogs);

            MessageBox.Show(message, "Конец игры");

            ShowMainMenu();
        }
    }

    // Дополнительный класс для доступа к методам контроллера
    public static class GameControllerExtensions
    {
        public static GameState LoadGameState(this EnhancedGameController controller, string filePath)
        {
            var service = new JsonGameStateService();
            return service.LoadGameState(filePath);
        }

        public static void SaveGameState(this EnhancedGameController controller, string filePath, GameState state)
        {
            var service = new JsonGameStateService();
            service.SaveGameState(filePath, state);
        }

        public static JsonGameStateService GetStateService(this EnhancedGameController controller)
        {
            return new JsonGameStateService();
        }
    }
}