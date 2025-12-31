using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RFCardGame.Core
{
    public static class CardUtils
    {
        /// <summary>
        /// Перемешать колоду.
        /// </summary>
        public static void ShuffleDeck(List<ICard> deck)
        {
            if (deck == null) throw new ArgumentNullException(nameof(deck));

            var random = new Random();
            int n = deck.Count;

            while (n > 1)
            {
                n--;
                int k = random.Next(n + 1);
                (deck[k], deck[n]) = (deck[n], deck[k]);
            }
        }

        /// <summary>
        /// Взять верхнюю карту из колоды.
        /// </summary>
        public static ICard DrawCard(List<ICard> deck, List<ICard> hand)
        {
            if (deck == null || hand == null)
                throw new ArgumentNullException();

            if (deck.Count == 0)
                return null;

            var card = deck[0];
            deck.RemoveAt(0);
            hand.Add(card);

            return card;
        }

        /// <summary>
        /// Найти карту по имени в коллекции.
        /// </summary>
        public static ICard FindCardByName(List<ICard> cards, string name)
        {
            return cards.FirstOrDefault(c =>
                c.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
        }

        /// <summary>
        /// Отфильтровать карты по стоимости.
        /// </summary>
        public static List<ICard> FilterByCost(List<ICard> cards, int maxCost)
        {
            return cards.Where(c => c.Cost <= maxCost).ToList();
        }

        /// <summary>
        /// Получить статистику по коллекции карт.
        /// </summary>
        public static void PrintDeckStatistics(List<ICard> deck, string deckName = "Колода")
        {
            Console.WriteLine($"=== Статистика: {deckName} ===");
            Console.WriteLine($"Всего карт: {deck.Count}");

            var byFaction = deck.GroupBy(c => c.Faction)
                .ToDictionary(g => g.Key, g => g.Count());

            foreach (var kvp in byFaction)
            {
                Console.WriteLine($"{kvp.Key}: {kvp.Value} карт");
            }

            var byType = deck.GroupBy(c => c.Type)
                .ToDictionary(g => g.Key, g => g.Count());

            foreach (var kvp in byType)
            {
                Console.WriteLine($"{kvp.Key}: {kvp.Value} карт");
            }

            var averageCost = deck.Average(c => c.Cost);
            Console.WriteLine($"Средняя стоимость: {averageCost:F1}");
            Console.WriteLine();
        }

        /// <summary>
        /// Клонировать коллекцию карт (глубокое копирование).
        /// </summary>
        public static List<ICard> CloneCardList(List<ICard> cards)
        {
            return cards.Select(c => c.Clone() as ICard).ToList();
        }

        /// <summary>
        /// Найти все существа на поле, которые могут атаковать.
        /// </summary>
        public static List<ICreatureCard> GetAttackReadyCreatures(List<ICreatureCard> field)
        {
            return field.Where(c => c.IsAlive && c.State == CreatureState.Active).ToList();
        }

        /// <summary>
        /// Найти все живые существа на поле.
        /// </summary>
        public static List<ICreatureCard> GetAliveCreatures(List<ICreatureCard> field)
        {
            return field.Where(c => c.IsAlive).ToList();
        }

        /// <summary>
        /// Проверить, может ли игрок разыграть карту (хватает ли энергии).
        /// </summary>
        public static bool CanPlayCard(ICard card, int currentEnergy)
        {
            return card.Cost <= currentEnergy;
        }
    }
}
