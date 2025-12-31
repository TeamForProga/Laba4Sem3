using System;
using System.Collections.Generic;
using System.Linq;

namespace RFCardGame.Core
{
    /// <summary>
    /// Полная фабрика карт с целевыми эффектами для всех фракций.
    /// </summary>
    public class CardFactory : ICardFactory
    {
        private readonly Dictionary<string, Func<ICard>> _cardCreators;

        public CardFactory()
        {
            _cardCreators = new Dictionary<string, Func<ICard>>(StringComparer.OrdinalIgnoreCase);
            InitializeAllCardCreators();
        }

        public ICard CreateCard(string cardName)
        {
            if (_cardCreators.TryGetValue(cardName, out var creator))
            {
                return creator();
            }
            throw new ArgumentException($"Карта с названием '{cardName}' не найдена.");
        }

        public List<ICard> GetAllCards()
        {
            return _cardCreators.Values.Select(creator => creator()).ToList();
        }

        public List<ICard> GetCardsByFaction(Faction faction)
        {
            return _cardCreators.Values
                .Select(creator => creator())
                .Where(card => card.Faction == faction)
                .ToList();
        }

        public List<ICard> GetCardsByType(CardType type)
        {
            return _cardCreators.Values
                .Select(creator => creator())
                .Where(card => card.Type == type)
                .ToList();
        }

        public List<ICard> CreateStandardDeck(Faction faction)
        {
            var deck = new List<ICard>();
            var factionCards = GetCardsByFaction(faction);
            var neutralCards = GetCardsByFaction(Faction.Neutral);

            foreach (var card in factionCards)
            {
                for (int i = 0; i < 2; i++)
                {
                    deck.Add(card.Clone() as ICard);
                }
            }

            foreach (var card in neutralCards)
            {
                deck.Add(card.Clone() as ICard);
            }

            return deck.Take(30).ToList();
        }

        private void InitializeAllCardCreators()
        {
            AddAccretiaCards();
            AddBellatoCards();
            AddCoraCards();
            AddSpellCards();
            AddArtifactCards();
        }

        #region 🔴 Accretia Empire Cards
        private void AddAccretiaCards()
        {
            _cardCreators["Штурмовой юнит Аккретии"] = () => new CreatureCard
            {
                Name = "Штурмовой юнит Аккретии",
                Cost = 2,
                Attack = 3,
                MaxHealth = 2,
                CurrentHealth = 2,
                Faction = Faction.Accretia,
                EffectText = "Базовое существо",
                Lore = "«Сталь не знает страха.»",
                ImagePath = "Images/Cards/Creatures/Accretia/Штурмовой_юнит_Аккретии.jpg"
            };

            _cardCreators["Боевой киборг подавления"] = () => new CreatureCard
            {
                Name = "Боевой киборг подавления",
                Cost = 3,
                Attack = 4,
                MaxHealth = 3,
                CurrentHealth = 3,
                Faction = Faction.Accretia,
                EffectText = "Базовое существо",
                Lore = "«Человек — слаб. Машина — эффективна.»",
                ImagePath = "Images/Cards/Creatures/Accretia/Боевой_киборг_подавления.jpg"
            };

            _cardCreators["Осадный киборг"] = () => new CreatureCard
            {
                Name = "Осадный киборг",
                Cost = 5,
                Attack = 6,
                MaxHealth = 4,
                CurrentHealth = 4,
                Faction = Faction.Accretia,
                EffectText = "Базовое существо",
                Lore = "«Он создан, чтобы разрушать.»",
                ImagePath = "Images/Cards/Creatures/Accretia/Осадный_киборг.jpg"
            };

            _cardCreators["Стальной авангард"] = () => new CreatureCard
            {
                Name = "Стальной авангард",
                Cost = 4,
                Attack = 2,
                MaxHealth = 6,
                CurrentHealth = 6,
                Faction = Faction.Accretia,
                EffectText = "Базовое существо",
                Lore = "«Пока он стоит — Империя не падёт.»",
                ImagePath = "Images/Cards/Creatures/Accretia/Стальной_авангард.jpg"
            };

            _cardCreators["Командный кибер-офицер"] = () => new CreatureCard
            {
                Name = "Командный кибер-офицер",
                Cost = 6,
                Attack = 5,
                MaxHealth = 5,
                CurrentHealth = 5,
                Faction = Faction.Accretia,
                EffectText = "Базовое существо",
                Lore = "«Приказ абсолютен.»",
                ImagePath = "Images/Cards/Creatures/Accretia/Командный_кибер-офицер.jpg"
            };
        }
        #endregion

        #region 🔵 Bellato Union Cards
        private void AddBellatoCards()
        {
            _cardCreators["Разведчик Беллато"] = () => new CreatureCard
            {
                Name = "Разведчик Беллато",
                Cost = 2,
                Attack = 2,
                MaxHealth = 2,
                CurrentHealth = 2,
                Faction = Faction.Bellato,
                EffectText = "Базовое существо",
                Lore = "«Информация — половина победы.»",
                ImagePath = "Images/Cards/Creatures/Bellato/Разведчик_Беллато.jpg"
            };

            _cardCreators["Рейнджер Беллато"] = () => new CreatureCard
            {
                Name = "Рейнджер Беллато",
                Cost = 3,
                Attack = 3,
                MaxHealth = 2,
                CurrentHealth = 2,
                Faction = Faction.Bellato,
                EffectText = "Базовое существо",
                Lore = "«Точность важнее силы.»",
                ImagePath = "Images/Cards/Creatures/Bellato/Рейнджер_Беллато.jpg"
            };

            _cardCreators["Тактический инженер"] = () => new CreatureCard
            {
                Name = "Тактический инженер",
                Cost = 3,
                Attack = 2,
                MaxHealth = 4,
                CurrentHealth = 4,
                Faction = Faction.Bellato,
                EffectText = "Базовое существо",
                Lore = "«Каждый болт имеет значение.»",
                ImagePath = "Images/Cards/Creatures/Bellato/Тактический_инженер.jpg"
            };

            _cardCreators["Пилот MAU"] = () => new CreatureCard
            {
                Name = "Пилот MAU",
                Cost = 5,
                Attack = 4,
                MaxHealth = 4,
                CurrentHealth = 4,
                Faction = Faction.Bellato,
                EffectText = "Базовое существо",
                Lore = "«MAU — символ технологического превосходства.»",
                ImagePath = "Images/Cards/Creatures/Bellato/Пилот_MAU.jpg"
            };

            _cardCreators["Командир тактической группы"] = () => new CreatureCard
            {
                Name = "Командир тактической группы",
                Cost = 6,
                Attack = 5,
                MaxHealth = 5,
                CurrentHealth = 5,
                Faction = Faction.Bellato,
                EffectText = "Базовое существо",
                Lore = "«Победа — это правильный расчёт.»",
                ImagePath = "Images/Cards/Creatures/Bellato/Командир_тактической_группы.jpg"
            };
        }
        #endregion

        #region 🟣 Holy Alliance Cora Cards
        private void AddCoraCards()
        {
            _cardCreators["Послушник Коры"] = () => new CreatureCard
            {
                Name = "Послушник Коры",
                Cost = 2,
                Attack = 1,
                MaxHealth = 3,
                CurrentHealth = 3,
                Faction = Faction.Cora,
                EffectText = "Базовое существо",
                Lore = "«Каждый путь начинается с веры.»",
                ImagePath = "Images/Cards/Creatures/Cora/Послушник_Коры.jpg"
            };

            _cardCreators["Жрец Коры"] = () => new CreatureCard
            {
                Name = "Жрец Коры",
                Cost = 3,
                Attack = 1,
                MaxHealth = 4,
                CurrentHealth = 4,
                Faction = Faction.Cora,
                EffectText = "Базовое существо",
                Lore = "«Анимус слышит зов живых.»",
                ImagePath = "Images/Cards/Creatures/Cora/Жрец_Коры.jpg"
            };

            _cardCreators["Страж Коры"] = () => new CreatureCard
            {
                Name = "Страж Коры",
                Cost = 4,
                Attack = 3,
                MaxHealth = 5,
                CurrentHealth = 5,
                Faction = Faction.Cora,
                EffectText = "Базовое существо",
                Lore = "«Его сила растёт с каждой молитвой.»",
                ImagePath = "Images/Cards/Creatures/Cora/Страж_Коры.jpg"
            };

            _cardCreators["Призванный Анимус"] = () => new CreatureCard
            {
                Name = "Призванный Анимус",
                Cost = 6,
                Attack = 4,
                MaxHealth = 4,
                CurrentHealth = 4,
                Faction = Faction.Cora,
                EffectText = "Базовое существо",
                Lore = "«Он возвращается в поток энергии.»",
                ImagePath = "Images/Cards/Creatures/Cora/Призванный_Анимус.jpg"
            };

            _cardCreators["Верховный маг Коры"] = () => new CreatureCard
            {
                Name = "Верховный маг Коры",
                Cost = 7,
                Attack = 5,
                MaxHealth = 6,
                CurrentHealth = 6,
                Faction = Faction.Cora,
                EffectText = "Базовое существо",
                Lore = "«Воля Анимуса воплощена в нём.»",
                ImagePath = "Images/Cards/Creatures/Cora/Верховный_маг_Коры.jpg"
            };
        }
        #endregion

        #region ✨ Spell Cards
        private void AddSpellCards()
        {
            _cardCreators["Тёмный взрыв"] = () => new SpellCard
            {
                Name = "Тёмный взрыв",
                Cost = 4,
                Faction = Faction.Cora,
                EffectText = "Наносит 5 урона выбранному существу.",
                Lore = "«Тьма поглощает слабых.»",
                Subtype = SpellSubtype.Attack,
                TargetType = "SingleTarget",
                Power = 5,
                ImagePath = "Images/Cards/Spells/Cora/Темный_взрыв.jpg"
            };

            _cardCreators["Святое восстановление"] = () => new SpellCard
            {
                Name = "Святое восстановление",
                Cost = 3,
                Faction = Faction.Cora,
                EffectText = "Восстанавливает 4 здоровья цели.",
                Lore = "«Сила Анимуса исцеляет раны.»",
                Subtype = SpellSubtype.Healing,
                TargetType = "SingleTarget",
                Power = 4,
                ImagePath = "Images/Cards/Spells/Cora/Святое_восстановление.jpg"
            };

            _cardCreators["Протокол перегрузки"] = () => new SpellCard
            {
                Name = "Протокол перегрузки",
                Cost = 2,
                Faction = Faction.Accretia,
                EffectText = "Даёт +4 к атаке существу.",
                Lore = "«Максимальная производительность.»",
                Subtype = SpellSubtype.Buff,
                TargetType = "SingleTarget",
                Power = 4,
                ImagePath = "Images/Cards/Spells/Accretia/Протокол_перегрузки.jpg"
            };

            _cardCreators["Орбитальный удар"] = () => new SpellCard
            {
                Name = "Орбитальный удар",
                Cost = 5,
                Faction = Faction.Bellato,
                EffectText = "Наносит 3 урона всем существам противника.",
                Lore = "«Удар с орбиты разрешён.»",
                Subtype = SpellSubtype.Attack,
                TargetType = "AllEnemyCreatures",
                Power = 3,
                ImagePath = "Images/Cards/Spells/Bellato/Орбитальный_удар.jpg"
            };

            _cardCreators["Тактический анализ"] = () => new SpellCard
            {
                Name = "Тактический анализ",
                Cost = 3,
                Faction = Faction.Bellato,
                EffectText = "Все ваши существа получают +1 к атаке.",
                Lore = "«Побеждает тот, кто контролирует поле боя.»",
                Subtype = SpellSubtype.Buff,
                TargetType = "AllAllyCreatures",
                Power = 1,
                ImagePath = "Images/Cards/Spells/Bellato/Тактический_анализ.jpg"
            };
        }
        #endregion

        #region 🧩 Artifact Cards
        private void AddArtifactCards()
        {
            _cardCreators["Горнодобывающая установка"] = () => new ArtifactCard
            {
                Name = "Горнодобывающая установка",
                Cost = 4,
                Faction = Faction.Neutral,
                EffectText = "Дает +1 к максимальной энергии",
                Lore = "«Ресурсы решают исход любой войны.»",
                Duration = 0,
                ImagePath = "Images/Cards/Artifacts/Neutral/Горнодобывающая_установка.jpg"
            };

            _cardCreators["Энергетический узел"] = () => new ArtifactCard
            {
                Name = "Энергетический узел",
                Cost = 3,
                Faction = Faction.Neutral,
                EffectText = "Дает +2 энергии",
                Lore = "«Стабильный поток энергии — залог превосходства.»",
                Duration = 0,
                ImagePath = "Images/Cards/Artifacts/Neutral/Энергетический_узел.jpg"
            };

            _cardCreators["Священная реликвия Коры"] = () => new ArtifactCard
            {
                Name = "Священная реликвия Коры",
                Cost = 4,
                Faction = Faction.Cora,
                EffectText = "Дает +1 здоровье всем вашим существам",
                Lore = "«Реликвия хранит отголоски древней силы Анимуса.»",
                Duration = 0,
                ImagePath = "Images/Cards/Artifacts/Cora/Священная_реликвия_Коры.jpg"
            };
        }
        #endregion
    }

        /// <summary>
        /// Статическая библиотека карт с полным набором всех фракций.
        /// </summary>
        public static class CompleteCardLibrary
    {
        private static readonly ICardFactory _factory = new CardFactory();

        public static ICardFactory Factory => _factory;
        public static List<ICard> AllCards => _factory.GetAllCards();

        public static ICard Create(string cardName) => _factory.CreateCard(cardName);
        public static List<ICard> CreateDeck(Faction faction) => _factory.CreateStandardDeck(faction);
    }

    /// <summary>
    /// Фабрика для создания карт.
    /// </summary>
    public interface ICardFactory
    {
        ICard CreateCard(string cardName);
        List<ICard> GetAllCards();
        List<ICard> GetCardsByFaction(Faction faction);
        List<ICard> GetCardsByType(CardType type);
        List<ICard> CreateStandardDeck(Faction faction);
    }
}