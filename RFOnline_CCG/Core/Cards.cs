using System;
using System.Collections.Generic;

namespace RFCardGame.Core
{
    /// <summary>
    /// Базовый интерфейс для всех карт в игре.
    /// </summary>
    public interface ICard : ICloneable
    {
        Guid Id { get; }
        string Name { get; set; }
        int Cost { get; set; }
        Faction Faction { get; set; }
        CardType Type { get; set; }
        string EffectText { get; set; }
        string Lore { get; set; }

        string ImagePath { get; set; } // <- ДОБАВИМ
    }

    public interface ICreatureCard : ICard
    {
        int Attack { get; set; }
        int MaxHealth { get; set; }
        int CurrentHealth { get; set; }
        CreatureState State { get; set; }
        bool IsAlive { get; }
        void TakeDamage(int damage);
        void Heal(int amount);
    }

    public interface IArtifactCard : ICard
    {
        int Duration { get; set; }
        bool IsActive { get; set; }
        void Activate();
        void Deactivate();
    }

    public interface ISpellCard : ICard
    {
        SpellSubtype Subtype { get; set; }
        string TargetType { get; set; }
        int Power { get; set; }
    }

    [Serializable]
    public abstract class CardBase : ICard
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public int Cost { get; set; }
        public Faction Faction { get; set; }
        public CardType Type { get; set; }
        public string EffectText { get; set; }
        public string Lore { get; set; }

        public string ImagePath { get; set; } // <- ДОБАВИМ

        protected CardBase()
        {
            Id = Guid.NewGuid();
        }
        private string GenerateImagePath()
        {
            // Автоматически генерируем путь
            string cleanName = Name
                ?.Replace(" ", "_")
                .Replace("ё", "е")
                .Replace("Ё", "Е")
                .Replace(":", "")
                .Replace("«", "")
                .Replace("»", "")
                .Replace("\"", "")
                ?? "unknown";

            string typeFolder = Type switch
            {
                CardType.Creature => "Creatures",
                CardType.Spell => "Spells",
                CardType.Artifact => "Artifacts",
                _ => "Creatures"
            };

            string factionFolder = Faction.ToString();

            string fileName = Type switch
            {
                CardType.Spell => $"spell_{cleanName}",
                CardType.Artifact => $"artifact_{cleanName}",
                _ => cleanName
            };

            return $"Images/Cards/{typeFolder}/{factionFolder}/{fileName}.jpg";
        }
        public override string ToString()
        {
            return $"[{Faction}] {Name} ({Cost}) - {Type}";
        }

        public virtual object Clone()
        {
            return MemberwiseClone();
        }
    }

    /// <summary>
    /// Класс, представляющий карту существа.
    /// </summary>
    [Serializable]
    public class CreatureCard : CardBase, ICreatureCard
    {
        private int _currentHealth;

        public int Attack { get; set; }
        public int MaxHealth { get; set; }

        public int CurrentHealth
        {
            get => _currentHealth;
            set => _currentHealth = Math.Min(value, MaxHealth);
        }

        public CreatureState State { get; set; } = CreatureState.Asleep;
        public bool IsAlive => CurrentHealth > 0;

        public CreatureCard()
        {
            Type = CardType.Creature;
            CurrentHealth = MaxHealth;
        }

        public void TakeDamage(int damage)
        {
            if (damage < 0) return;
            CurrentHealth -= damage;
        }

        public void Heal(int amount)
        {
            if (amount < 0) return;
            CurrentHealth += amount;
        }

        public override string ToString()
        {
            return $"{base.ToString()} - {Attack}/{CurrentHealth}({MaxHealth}) [{State}]";
        }
    }

    // В файле Cards.cs дополним класс ArtifactCard
    [Serializable]
    public class ArtifactCard : CardBase, IArtifactCard
    {
        public int Duration { get; set; }
        public bool IsActive { get; set; }
        public string EffectType { get; set; } // Просто строка для типа эффекта

        public ArtifactCard()
        {
            Type = CardType.Artifact;
            Duration = 0; // 0 = одноразовый, >0 = количество ходов
        }

        public virtual void Activate()
        {
            IsActive = true;
        }

        public virtual void Deactivate()
        {
            IsActive = false;
        }

        public override string ToString()
        {
            return $"{base.ToString()} - Active: {IsActive}, Duration: {(Duration == 0 ? "∞" : Duration.ToString())}";
        }
    }
    /// <summary>
    /// Класс, представляющий карту заклинания.
    /// </summary>
    [Serializable]
    public class SpellCard : CardBase, ISpellCard
    {
        public SpellSubtype Subtype { get; set; }
        public string TargetType { get; set; } = "SingleTarget";
        public int Power { get; set; }

        public SpellCard()
        {
            Type = CardType.Spell;
        }

        public override string ToString()
        {
            return $"{base.ToString()} - {Subtype} ({Power}) -> {TargetType}";
        }
    }

    /// <summary>
    /// Фракции вселенной RF Online.
    /// </summary>
    public enum Faction
    {
        Accretia,
        Bellato,
        Cora,
        Neutral
    }

    /// <summary>
    /// Основные типы карт.
    /// </summary>
    public enum CardType
    {
        Creature,
        Spell,
        Artifact
    }

    /// <summary>
    /// Подтипы заклинаний.
    /// </summary>
    public enum SpellSubtype
    {
        Attack,
        Healing,
        Buff,
        Other
    }

    /// <summary>
    /// Состояние существа на поле.
    /// </summary>
    public enum CreatureState
    {
        Active,
        Exhausted,
        Asleep
    }
}