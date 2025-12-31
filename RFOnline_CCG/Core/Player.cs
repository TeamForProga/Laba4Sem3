using System;
using System.Collections.Generic;
using System.Linq;

namespace RFCardGame.Core
{
    public interface IPlayer
    {
        string Name { get; }
        int Health { get; set; }
        int Energy { get; set; }
        int MaxEnergy { get; set; }
        List<ICard> Hand { get; }
        List<ICreatureCard> Field { get; }
        List<IArtifactCard> Artifacts { get; }
    }
    /// <summary>
    /// Класс, представляющий игрока.
    /// </summary>
    [Serializable]
    public class Player : IPlayer
    {
        public string Name { get; set; }
        public int Health { get; set; }
        public int Energy { get; set; }
        public int MaxEnergy { get; set; }
        public List<ICard> Deck { get; set; } = new List<ICard>();
        public List<ICard> Hand { get; set; } = new List<ICard>();
        public List<ICreatureCard> Field { get; set; } = new List<ICreatureCard>();
        public List<IArtifactCard> Artifacts { get; set; } = new List<IArtifactCard>();
        public Faction Faction { get; set; }

        public Player(string name, Faction faction, int startingHealth = 30)
        {
            Name = name;
            Faction = faction;
            Health = startingHealth;
            Energy = 0;
            MaxEnergy = 0;
        }

        /// <summary>
        /// Взять карту из колоды.
        /// </summary>
        public ICard DrawCard()
        {
            if (Deck.Count == 0)
            {
                return null; // Усталость - игрок теряет здоровье
            }

            var card = Deck[0];
            Deck.RemoveAt(0);
            Hand.Add(card);
            return card;
        }

        /// <summary>
        /// Взять несколько карт.
        /// </summary>
        public List<ICard> DrawCards(int count)
        {
            var drawnCards = new List<ICard>();
            for (int i = 0; i < count && Deck.Count > 0; i++)
            {
                drawnCards.Add(DrawCard());
            }
            return drawnCards;
        }

        /// <summary>
        /// Перемешать колоду.
        /// </summary>
        public void ShuffleDeck()
        {
            CardUtils.ShuffleDeck(Deck);
        }

        /// <summary>
        /// Положить карту из руки на поле.
        /// </summary>
        public bool PlayCreatureCard(ICreatureCard creatureCard)
        {
            if (creatureCard == null || !Hand.Contains(creatureCard))
                return false;

            if (Energy < creatureCard.Cost)
                return false;

            // Оплачиваем стоимость
            Energy -= creatureCard.Cost;

            // Убираем карту из руки
            Hand.Remove(creatureCard);

            // Добавляем на поле
            Field.Add(creatureCard);
            creatureCard.State = CreatureState.Asleep; // Не может атаковать в первый ход

            return true;
        }

        /// <summary>
        /// Сыграть заклинание.
        /// </summary>
        public bool PlaySpellCard(ISpellCard spellCard, List<object> targets = null)
        {
            if (spellCard == null || !Hand.Contains(spellCard))
                return false;

            if (Energy < spellCard.Cost)
                return false;

            // Оплачиваем стоимость
            Energy -= spellCard.Cost;

            // Убираем карту из руки
            Hand.Remove(spellCard);

            return true;
        }

        /// <summary>
        /// Сыграть артефакт.
        /// </summary>
        // В классе Player дополним метод PlayArtifactCard
        public bool PlayArtifactCard(IArtifactCard artifactCard)
        {
            if (artifactCard == null || !Hand.Contains(artifactCard))
                return false;

            if (Energy < artifactCard.Cost)
                return false;

            // Оплачиваем стоимость
            Energy -= artifactCard.Cost;

            // Убираем карту из руки
            Hand.Remove(artifactCard);

            // Активируем артефакт
            artifactCard.Activate();
            Artifacts.Add(artifactCard);

            // Сразу применяем эффект (одноразовый)
            if (artifactCard.Duration == 0) // 0 означает одноразовый эффект
            {
                // Эффект применится в GameEngine
                Artifacts.Remove(artifactCard); // Убираем из активных, так как одноразовый
            }

            return true;
        }
        private void ApplyArtifactEffect(IArtifactCard artifact)
        {
            // Здесь можно добавить немедленные эффекты артефактов
            // или они могут применяться каждый ход
        }
        /// <summary>
        /// Получить общую атаку всех существ на поле.
        /// </summary>
        public int GetTotalAttack()
        {
            return Field.Where(c => c.IsAlive && c.State == CreatureState.Active)
                       .Sum(c => c.Attack);
        }

        /// <summary>
        /// Получить количество живых существ.
        /// </summary>
        public int GetAliveCreatureCount()
        {
            return Field.Count(c => c.IsAlive);
        }

        /// <summary>
        /// Очистить поле от мёртвых существ.
        /// </summary>
        public void CleanupDeadCreatures()
        {
            Field.RemoveAll(c => !c.IsAlive);
        }

        /// <summary>
        /// Сбросить состояние существ (подготовить к новому ходу).
        /// </summary>
        public void ResetCreatureStates()
        {
            foreach (var creature in Field.Where(c => c.IsAlive))
            {
                if (creature.State == CreatureState.Asleep)
                {
                    creature.State = CreatureState.Active; // Просыпается на второй ход
                }
                else if (creature.State == CreatureState.Exhausted)
                {
                    creature.State = CreatureState.Active; // Восстанавливается
                }
            }
        }

        /// <summary>
        /// Проверить, проиграл ли игрок.
        /// </summary>
        public bool HasLost()
        {
            return Health <= 0;
        }

        public override string ToString()
        {
            return $"{Name} ({Faction}) - Здоровье: {Health}, Энергия: {Energy}/{MaxEnergy}";
        }
    }
}