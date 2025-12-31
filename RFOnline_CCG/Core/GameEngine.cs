using System;
using System.Collections.Generic;
using System.Linq;

namespace RFCardGame.Core
{
    /// <summary>
    /// Основной игровой движок.
    /// </summary>
    public class GameEngine
    {
        public Player Player1 { get; private set; }
        public Player Player2 { get; private set; }
        public Player CurrentPlayer { get; private set; }
        public Player OpponentPlayer { get; private set; }
        public int CurrentTurn { get; private set; } = 1;
        public bool IsGameOver { get; private set; }
        public Player Winner { get; private set; }
        public List<string> GameLog { get; private set; } = new List<string>();
        public List<ICard> Graveyard { get; private set; } = new List<ICard>();

        public GameEngine(string player1Name, Faction player1Faction,
                         string player2Name, Faction player2Faction)
        {
            InitializePlayers(player1Name, player1Faction, player2Name, player2Faction);
        }

        private void InitializePlayers(string player1Name, Faction player1Faction,
                                      string player2Name, Faction player2Faction)
        {
            var factory = new CardFactory();
            Player1 = new Player(player1Name, player1Faction);
            Player2 = new Player(player2Name, player2Faction);

            Player1.Deck = factory.CreateStandardDeck(player1Faction);
            Player2.Deck = factory.CreateStandardDeck(player2Faction);

            Player1.ShuffleDeck();
            Player2.ShuffleDeck();

            CurrentPlayer = Player1;
            OpponentPlayer = Player2;
        }

        #region Методы сохранения и загрузки

        /// <summary>
        /// Создать состояние игры для сохранения.
        /// </summary>
        public GameState CreateGameState()
        {
            var state = new GameState
            {
                Player1Name = Player1.Name,
                Player2Name = Player2.Name,
                Player1Faction = Player1.Faction,
                Player2Faction = Player2.Faction,
                CurrentTurn = CurrentTurn,
                IsPlayer1Turn = CurrentPlayer == Player1,
                SaveDate = DateTime.Now
            };

            // Сохраняем данные игрока 1
            state.Player1Data = SavePlayerData(Player1);

            // Сохраняем данные игрока 2
            state.Player2Data = SavePlayerData(Player2);

            // Сохраняем кладбище (только имена карт)
            state.Graveyard = Graveyard.Select(c => c.Name).ToList();

            return state;
        }

        /// <summary>
        /// Загрузить игру из сохраненного состояния.
        /// </summary>
        public void LoadFromState(GameState state, JsonGameStateService service)
        {
            if (state == null) throw new ArgumentNullException(nameof(state));
            if (service == null) throw new ArgumentNullException(nameof(service));

            // Сбрасываем текущее состояние
            IsGameOver = false;
            Winner = null;
            GameLog.Clear();
            Graveyard.Clear();

            // Восстанавливаем основные параметры
            CurrentTurn = state.CurrentTurn;

            // Восстанавливаем игроков
            Player1 = LoadPlayerData(state.Player1Data, service);
            Player2 = LoadPlayerData(state.Player2Data, service);

            // Устанавливаем текущего игрока
            CurrentPlayer = state.IsPlayer1Turn ? Player1 : Player2;
            OpponentPlayer = state.IsPlayer1Turn ? Player2 : Player1;

            // Восстанавливаем кладбище
            foreach (var cardName in state.Graveyard)
            {
                Graveyard.Add(service.CreateCardFromName(cardName));
            }

            AddToGameLog($"=== ИГРА ЗАГРУЖЕНА ===");
            AddToGameLog($"Ход: {CurrentTurn}, Текущий игрок: {CurrentPlayer.Name}");
            AddToGameLog($"Дата сохранения: {state.SaveDate}");
        }

        private PlayerData SavePlayerData(Player player)
        {
            return new PlayerData
            {
                Name = player.Name,
                Faction = player.Faction,
                Health = player.Health,
                Energy = player.Energy,
                MaxEnergy = player.MaxEnergy,
                Deck = player.Deck.Select(c => c.Name).ToList(),
                Hand = player.Hand.Select(c => c.Name).ToList(),
                Field = player.Field.Select(c => new CreatureData
                {
                    Name = c.Name,
                    Attack = c.Attack,
                    CurrentHealth = c.CurrentHealth,
                    MaxHealth = c.MaxHealth,
                    State = c.State
                }).ToList(),
                Artifacts = player.Artifacts.Select(c => c.Name).ToList()
            };
        }

        private Player LoadPlayerData(PlayerData data, JsonGameStateService service)
        {
            var player = new Player(data.Name, data.Faction)
            {
                Health = data.Health,
                Energy = data.Energy,
                MaxEnergy = data.MaxEnergy
            };

            // Восстанавливаем колоду
            player.Deck = new List<ICard>();
            foreach (var cardName in data.Deck)
            {
                player.Deck.Add(service.CreateCardFromName(cardName));
            }

            // Восстанавливаем руку
            player.Hand = new List<ICard>();
            foreach (var cardName in data.Hand)
            {
                player.Hand.Add(service.CreateCardFromName(cardName));
            }

            // Восстанавливаем существ на поле
            player.Field = new List<ICreatureCard>();
            foreach (var creatureData in data.Field)
            {
                var creature = service.CreateCardFromName(creatureData.Name,
                    new Dictionary<string, object>
                    {
                        ["CurrentHealth"] = creatureData.CurrentHealth,
                        ["Attack"] = creatureData.Attack,
                        ["State"] = creatureData.State
                    }) as CreatureCard;

                if (creature != null)
                {
                    creature.MaxHealth = creatureData.MaxHealth;
                    player.Field.Add(creature);
                }
            }

            // Восстанавливаем артефакты
            player.Artifacts = new List<IArtifactCard>();
            foreach (var cardName in data.Artifacts)
            {
                var artifact = service.CreateCardFromName(cardName) as ArtifactCard;
                if (artifact != null)
                {
                    artifact.Activate();
                    player.Artifacts.Add(artifact);
                }
            }

            return player;
        }

        #endregion

        #region Основные методы игры

        public void StartGame()
        {
            IsGameOver = false;
            Winner = null;
            GameLog.Clear();
            Graveyard.Clear();

            AddToGameLog($"=== НАЧАЛО ИГРЫ ===");
            AddToGameLog($"{Player1.Name} ({Player1.Faction}) vs {Player2.Name} ({Player2.Faction})");

            Player1.DrawCards(2);
            Player2.DrawCards(3);

            AddToGameLog($"{Player1.Name} берёт 2 карты");
            AddToGameLog($"{Player2.Name} берёт 3 карты");

            StartTurn();
        }

        public void StartTurn()
        {
            if (IsGameOver) return;

            CurrentPlayer.MaxEnergy = Math.Min(CurrentPlayer.MaxEnergy + 1, 10);
            CurrentPlayer.Energy = CurrentPlayer.MaxEnergy;
            CurrentPlayer.ResetCreatureStates();
            CurrentPlayer.DrawCard();

            AddToGameLog($"=== ХОД {CurrentTurn} - {CurrentPlayer.Name} ===");
            AddToGameLog($"{CurrentPlayer.Name}: +1 энергия ({CurrentPlayer.Energy}/{CurrentPlayer.MaxEnergy})");
            AddToGameLog($"{CurrentPlayer.Name} берёт карту. В руке: {CurrentPlayer.Hand.Count}");
        }

        public void EndTurn()
        {
            if (IsGameOver) return;

            CurrentPlayer.CleanupDeadCreatures();
            OpponentPlayer.CleanupDeadCreatures();

            SwapPlayers();

            if (CurrentPlayer == Player1)
            {
                CurrentTurn++;
            }

            StartTurn();
        }

        private void SwapPlayers()
        {
            var temp = CurrentPlayer;
            CurrentPlayer = OpponentPlayer;
            OpponentPlayer = temp;
        }

        public void EndGame(Player winner)
        {
            IsGameOver = true;
            Winner = winner;
            AddToGameLog($"=== ИГРА ОКОНЧЕНА ===");
            AddToGameLog($"Победитель: {winner.Name}!");
        }

        #endregion

        #region Игровые действия

        public bool PlayCreatureCard(ICreatureCard creatureCard)
        {
            if (IsGameOver || creatureCard == null) return false;
            if (!CurrentPlayer.Hand.Contains(creatureCard)) return false;
            if (CurrentPlayer.Energy < creatureCard.Cost) return false;
            if (CurrentPlayer.Field.Count >= 7) return false;

            bool success = CurrentPlayer.PlayCreatureCard(creatureCard);
            if (success)
            {
                AddToGameLog($"{CurrentPlayer.Name} призывает {creatureCard.Name} ({creatureCard.Cost} энергии)");
                return true;
            }
            return false;
        }

        public bool PlaySpellCard(ISpellCard spellCard, ICreatureCard target = null)
        {
            if (IsGameOver || spellCard == null) return false;
            if (!CurrentPlayer.Hand.Contains(spellCard)) return false;
            if (CurrentPlayer.Energy < spellCard.Cost) return false;

            bool success = CurrentPlayer.PlaySpellCard(spellCard, null);
            if (success)
            {
                AddToGameLog($"{CurrentPlayer.Name} применяет {spellCard.Name} ({spellCard.Cost} энергии)");

                // Простая реализация эффектов заклинаний
                if (spellCard.TargetType == "SingleTarget" && target != null)
                {
                    if (spellCard.Subtype == SpellSubtype.Attack)
                    {
                        target.TakeDamage(spellCard.Power);
                        AddToGameLog($"{target.Name} получает {spellCard.Power} урона");
                    }
                    else if (spellCard.Subtype == SpellSubtype.Healing)
                    {
                        target.Heal(spellCard.Power);
                        AddToGameLog($"{target.Name} восстанавливает {spellCard.Power} здоровья");
                    }
                    else if (spellCard.Subtype == SpellSubtype.Buff)
                    {
                        target.Attack += spellCard.Power;
                        AddToGameLog($"{target.Name} получает +{spellCard.Power} к атаке");
                    }
                }
                else if (spellCard.TargetType == "AllEnemyCreatures")
                {
                    foreach (var creature in OpponentPlayer.Field)
                    {
                        creature.TakeDamage(spellCard.Power);
                        AddToGameLog($"{creature.Name} получает {spellCard.Power} урона");
                    }
                }
                else if (spellCard.TargetType == "AllAllyCreatures")
                {
                    foreach (var creature in CurrentPlayer.Field)
                    {
                        creature.Attack += spellCard.Power;
                        AddToGameLog($"{creature.Name} получает +{spellCard.Power} к атаке");
                    }
                }

                Graveyard.Add(spellCard);
                return true;
            }
            return false;
        }

        public bool PlayArtifactCard(IArtifactCard artifactCard)
        {
            if (IsGameOver || artifactCard == null) return false;
            if (!CurrentPlayer.Hand.Contains(artifactCard)) return false;
            if (CurrentPlayer.Energy < artifactCard.Cost) return false;

            bool success = CurrentPlayer.PlayArtifactCard(artifactCard);
            if (success)
            {
                AddToGameLog($"{CurrentPlayer.Name} активирует {artifactCard.Name} ({artifactCard.Cost} энергии)");

                // Применяем эффект артефакта
                ApplyArtifactEffect(artifactCard);

                // Перемещаем в кладбище (если артефакт разового использования)
                if (artifactCard.Duration == 0) // 0 = одноразовый
                {
                    Graveyard.Add(artifactCard);
                }

                return true;
            }
            return false;
        }

        // В GameEngine.cs дополним метод ApplyArtifactEffect
        private void ApplyArtifactEffect(IArtifactCard artifact)
        {
            string artifactName = artifact.Name.ToLower();

            // Простой хардкод эффектов
            switch (artifactName)
            {
                case "горнодобывающая установка":
                case "горнодобывающая":
                    // +1 макс энергии каждый ход
                    CurrentPlayer.MaxEnergy += 1;
                    AddToGameLog($"{CurrentPlayer.Name}: +1 к максимальной энергии от Горнодобывающей установки");
                    break;

                case "энергетический узел":
                case "энергетический":
                    // +2 энергии сразу
                    CurrentPlayer.Energy = Math.Min(CurrentPlayer.Energy + 2, CurrentPlayer.MaxEnergy);
                    AddToGameLog($"{CurrentPlayer.Name}: +2 энергии от Энергетического узла");
                    break;

                case "священная реликвия коры":
                case "священная реликвия":
                case "реликвия коры":
                    // +1 здоровье всем существам игрока
                    foreach (var creature in CurrentPlayer.Field)
                    {
                        creature.MaxHealth += 1;
                        creature.Heal(1);
                    }
                    AddToGameLog($"{CurrentPlayer.Name}: +1 здоровье всем существам от Реликвии");
                    break;
            }
        }
        public bool AttackWithCreature(ICreatureCard attacker, ICreatureCard defender)
        {
            if (IsGameOver || attacker == null || defender == null) return false;
            if (!CurrentPlayer.Field.Contains(attacker)) return false;
            if (!OpponentPlayer.Field.Contains(defender)) return false;
            if (!attacker.IsAlive || attacker.State != CreatureState.Active) return false;
            if (!defender.IsAlive) return false;

            AddToGameLog($"{attacker.Name} атакует {defender.Name}!");

            defender.TakeDamage(attacker.Attack);
            AddToGameLog($"{defender.Name} получает {attacker.Attack} урона. Здоровье: {defender.CurrentHealth}/{defender.MaxHealth}");

            if (defender.IsAlive)
            {
                attacker.TakeDamage(defender.Attack);
                AddToGameLog($"{attacker.Name} получает {defender.Attack} урона в ответ. Здоровье: {attacker.CurrentHealth}/{attacker.MaxHealth}");
            }

            attacker.State = CreatureState.Exhausted;
            return true;
        }

        public bool AttackPlayerDirectly(ICreatureCard attacker)
        {
            if (IsGameOver || attacker == null) return false;
            if (!CurrentPlayer.Field.Contains(attacker)) return false;
            if (!attacker.IsAlive || attacker.State != CreatureState.Active) return false;
            if (OpponentPlayer.GetAliveCreatureCount() > 0) return false;

            AddToGameLog($"{attacker.Name} атакует {OpponentPlayer.Name} напрямую!");
            OpponentPlayer.Health -= attacker.Attack;
            AddToGameLog($"{OpponentPlayer.Name} получает {attacker.Attack} урона. Здоровье: {OpponentPlayer.Health}");

            if (OpponentPlayer.Health <= 0)
            {
                OpponentPlayer.Health = 0;
                EndGame(CurrentPlayer);
            }

            attacker.State = CreatureState.Exhausted;
            return true;
        }

        #endregion

        #region Вспомогательные методы

        public void AddToGameLog(string message)
        {
            GameLog.Add($"[{DateTime.Now:HH:mm:ss}] {message}");
        }

        public string GetGameStateSummary()
        {
            return $"=== СОСТОЯНИЕ ИГРЫ ===\n" +
                   $"Ход: {CurrentTurn}\n" +
                   $"Текущий игрок: {CurrentPlayer.Name}\n" +
                   $"\n{CurrentPlayer.Name}:\n" +
                   $"  Здоровье: {CurrentPlayer.Health}\n" +
                   $"  Энергия: {CurrentPlayer.Energy}/{CurrentPlayer.MaxEnergy}\n" +
                   $"  Карты в руке: {CurrentPlayer.Hand.Count}\n" +
                   $"  Существ на поле: {CurrentPlayer.GetAliveCreatureCount()}\n" +
                   $"\n{OpponentPlayer.Name}:\n" +
                   $"  Здоровье: {OpponentPlayer.Health}\n" +
                   $"  Энергия: {OpponentPlayer.Energy}/{OpponentPlayer.MaxEnergy}\n" +
                   $"  Карты в руке: {OpponentPlayer.Hand.Count}\n" +
                   $"  Существ на поле: {OpponentPlayer.GetAliveCreatureCount()}";
        }

        public string GetBattlefieldInfo()
        {
            var info = $"=== ПОЛЕ БОЯ ===\n";

            info += $"\n{CurrentPlayer.Name} (Ваше поле):\n";
            if (CurrentPlayer.Field.Any())
            {
                foreach (var creature in CurrentPlayer.Field)
                {
                    info += $"  {creature.Name} ({creature.Attack}/{creature.CurrentHealth}) [{creature.State}]\n";
                }
            }
            else
            {
                info += "  Нет существ\n";
            }

            info += $"\n{OpponentPlayer.Name} (Поле противника):\n";
            if (OpponentPlayer.Field.Any())
            {
                foreach (var creature in OpponentPlayer.Field)
                {
                    info += $"  {creature.Name} ({creature.Attack}/{creature.CurrentHealth}) [{creature.State}]\n";
                }
            }
            else
            {
                info += "  Нет существ\n";
            }

            return info;
        }

        #endregion
    }
}