using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace RFCardGame.Core
{
    /// <summary>
    /// Полное состояние игры для сериализации.
    /// </summary>
    public class GameState
    {
        public int Version { get; set; } = 1;
        public DateTime SaveDate { get; set; }
        public string Player1Name { get; set; }
        public string Player2Name { get; set; }
        public Faction Player1Faction { get; set; }
        public Faction Player2Faction { get; set; }
        public int CurrentTurn { get; set; }
        public bool IsPlayer1Turn { get; set; }

        // Данные игрока 1
        public PlayerData Player1Data { get; set; } = new PlayerData();

        // Данные игрока 2
        public PlayerData Player2Data { get; set; } = new PlayerData();

        // Кладбище (имена карт)
        public List<string> Graveyard { get; set; } = new();

        public GameState() => SaveDate = DateTime.Now;
    }

    /// <summary>
    /// Данные игрока для сохранения.
    /// </summary>
    public class PlayerData
    {
        public string Name { get; set; }
        public Faction Faction { get; set; }
        public int Health { get; set; }
        public int Energy { get; set; }
        public int MaxEnergy { get; set; }

        // Карты в колоде (только имена)
        public List<string> Deck { get; set; } = new();

        // Карты в руке (только имена)
        public List<string> Hand { get; set; } = new();

        // Существа на поле
        public List<CreatureData> Field { get; set; } = new();

        // Артефакты на поле
        public List<string> Artifacts { get; set; } = new();
    }

    /// <summary>
    /// Данные существа для сохранения.
    /// </summary>
    public class CreatureData
    {
        public string Name { get; set; }
        public int Attack { get; set; }
        public int CurrentHealth { get; set; }
        public int MaxHealth { get; set; }
        public CreatureState State { get; set; }
    }

    /// <summary>
    /// Сервис для сохранения и загрузки состояния игры.
    /// </summary>
    public interface IGameStateService
    {
        void SaveGameState(string filePath, GameState state);
        GameState LoadGameState(string filePath);
    }

    /// <summary>
    /// Основной сервис сохранения.
    /// </summary>
    public class JsonGameStateService : IGameStateService
    {
        private readonly CardFactory _cardFactory;
        private readonly JsonSerializerOptions _jsonOptions;

        public JsonGameStateService()
        {
            _cardFactory = new CardFactory();
            _jsonOptions = new JsonSerializerOptions
            {
                WriteIndented = true,
                Converters = { new JsonStringEnumConverter() }
            };
        }

        public void SaveGameState(string filePath, GameState state)
        {
            try
            {
                string json = JsonSerializer.Serialize(state, _jsonOptions);
                File.WriteAllText(filePath, json);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Ошибка сохранения: {ex.Message}", ex);
            }
        }

        public GameState LoadGameState(string filePath)
        {
            if (!File.Exists(filePath))
                throw new FileNotFoundException("Файл сохранения не найден", filePath);

            try
            {
                string json = File.ReadAllText(filePath);
                var state = JsonSerializer.Deserialize<GameState>(json, _jsonOptions)
                    ?? throw new InvalidDataException("Неверный формат файла");

                // Проверяем версию
                if (state.Version != 1)
                    throw new InvalidDataException($"Не поддерживаемая версия сохранения: {state.Version}");

                return state;
            }
            catch (JsonException ex)
            {
                throw new InvalidDataException($"Ошибка чтения файла: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Создать карту из данных сохранения.
        /// </summary>
        public ICard CreateCardFromName(string cardName, Dictionary<string, object> additionalData = null)
        {
            try
            {
                var card = _cardFactory.CreateCard(cardName);

                // Применяем дополнительные данные если есть
                if (additionalData != null && card is CreatureCard creature)
                {
                    if (additionalData.ContainsKey("CurrentHealth"))
                        creature.CurrentHealth = Convert.ToInt32(additionalData["CurrentHealth"]);
                    if (additionalData.ContainsKey("Attack"))
                        creature.Attack = Convert.ToInt32(additionalData["Attack"]);
                    if (additionalData.ContainsKey("State") &&
                        Enum.TryParse<CreatureState>(additionalData["State"].ToString(), out var state))
                        creature.State = state;
                }

                return card;
            }
            catch
            {
                // Создаем заглушку если карта не найдена
                return CreateStubCard(cardName);
            }
        }

        private ICard CreateStubCard(string cardName)
        {
            return new CreatureCard
            {
                Name = $"{cardName} (восстановлено)",
                Cost = 1,
                Attack = 1,
                MaxHealth = 1,
                CurrentHealth = 1,
                Faction = Faction.Neutral,
                EffectText = "Карта была восстановлена из сохранения",
                Lore = "Восстановленная карта"
            };
        }
    }
}