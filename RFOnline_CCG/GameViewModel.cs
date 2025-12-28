using RFCardGame.Core;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;

namespace RFOnline_CCG
{
    public class GameViewModel : INotifyPropertyChanged
    {
        private GameEngine _gameEngine;

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

        public GameViewModel() { }

        public void StartNewGame(string player1Name, Faction player1Faction,
                               string player2Name, Faction player2Faction)
        {
            GameEngine = new GameEngine(player1Name, player1Faction, player2Name, player2Faction);
            GameEngine.StartGame();
            UpdateAll();
        }

        public void LoadGame(string saveFile)
        {
            try
            {
                var service = new JsonGameStateService();
                var state = service.LoadGameState(saveFile);

                GameEngine = new GameEngine(state.Player1Name, state.Player1Faction,
                                          state.Player2Name, state.Player2Faction);
                GameEngine.LoadFromState(state, service);
                UpdateAll();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки: {ex.Message}");
            }
        }

        public void SaveGame(string fileName)
        {
            try
            {
                var state = GameEngine.CreateGameState();
                var service = new JsonGameStateService();
                service.SaveGameState($"Saves/{fileName}.json", state);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка сохранения: {ex.Message}");
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

        public void AttackPlayerDirectly()
        {
            if (SelectedPlayerCreature != null)
            {
                bool success = GameEngine.AttackPlayerDirectly(SelectedPlayerCreature);
                if (success) UpdateAll();
            }
        }

        public void EndTurn()
        {
            GameEngine.EndTurn();
            UpdateAll();
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
            UpdateCollection(GameLog, GameEngine.GameLog);

            // Сбрасываем выбранные карты
            SelectedHandCard = null;
            SelectedPlayerCreature = null;
            SelectedOpponentCreature = null;

            OnPropertyChanged(null);
        }

        private void UpdateCollection<T>(ObservableCollection<T> target, IEnumerable<T> source)
        {
            target.Clear();
            if (source != null)
            {
                foreach (var item in source)
                {
                    target.Add(item);
                }
            }
        }

        public void OnPropertyChanged(string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}