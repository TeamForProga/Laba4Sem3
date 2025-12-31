using Microsoft.VisualStudio.TestTools.UnitTesting;
using RFCardGame.Core;
using System;
using System.IO;
using System.Linq;

namespace TestProject1
{
    [TestClass]
    public class SaveTests
    {
        [TestMethod]
        public void GameState_Creation_Test()
        {
            // Arrange & Act
            var state = new GameState
            {
                Player1Name = "Тест1",
                Player2Name = "Тест2",
                Player1Faction = Faction.Accretia,
                Player2Faction = Faction.Bellato,
                CurrentTurn = 5,
                IsPlayer1Turn = true
            };

            // Assert
            Assert.AreEqual("Тест1", state.Player1Name);
            Assert.AreEqual("Тест2", state.Player2Name);
            Assert.AreEqual(Faction.Accretia, state.Player1Faction);
            Assert.AreEqual(Faction.Bellato, state.Player2Faction);
            Assert.AreEqual(5, state.CurrentTurn);
            Assert.IsTrue(state.IsPlayer1Turn);
        }

        [TestMethod]
        public void JsonGameStateService_SaveAndLoad_Test()
        {
            // Arrange
            var service = new JsonGameStateService();
            var state = new GameState
            {
                Player1Name = "Игрок1",
                Player2Name = "Игрок2",
                Player1Faction = Faction.Accretia,
                Player2Faction = Faction.Bellato,
                CurrentTurn = 3
            };

            string tempFile = Path.Combine(Path.GetTempPath(), $"test_save_{Guid.NewGuid()}.json");

            try
            {
                // Act - Save
                service.SaveGameState(tempFile, state);

                // Assert - File exists
                Assert.IsTrue(File.Exists(tempFile));

                // Act - Load
                var loadedState = service.LoadGameState(tempFile);

                // Assert
                Assert.IsNotNull(loadedState);
                Assert.AreEqual("Игрок1", loadedState.Player1Name);
                Assert.AreEqual("Игрок2", loadedState.Player2Name);
                Assert.AreEqual(Faction.Accretia, loadedState.Player1Faction);
                Assert.AreEqual(Faction.Bellato, loadedState.Player2Faction);
                Assert.AreEqual(3, loadedState.CurrentTurn);
            }
            finally
            {
                // Cleanup
                if (File.Exists(tempFile))
                    File.Delete(tempFile);
            }
        }
    }
}

namespace TestProject1
{
    [TestClass]
    public class CardTests
    {
        [TestMethod]
        public void CreatureCard_TakeDamage_Test()
        {
            var creature = new CreatureCard
            {
                Name = "Тестовое существо",
                MaxHealth = 10,
                CurrentHealth = 10
            };

            creature.TakeDamage(3);
            Assert.AreEqual(7, creature.CurrentHealth);
            Assert.IsTrue(creature.IsAlive);
        }


        [TestMethod]
        public void CreatureCard_Heal_Increases_Health_Test()
        {
            var creature = new CreatureCard
            {
                Name = "Тестовое существо",
                MaxHealth = 10,
                CurrentHealth = 3
            };

            creature.Heal(4);
            Assert.AreEqual(7, creature.CurrentHealth);
        }

        [TestMethod]
        public void CreatureCard_Heal_Not_Exceed_MaxHealth_Test()
        {
            var creature = new CreatureCard
            {
                Name = "Тестовое существо",
                MaxHealth = 10,
                CurrentHealth = 9
            };

            creature.Heal(5);
            Assert.AreEqual(10, creature.CurrentHealth);
        }
    }

    [TestClass]
    public class FactoryTests
    {
        [TestMethod]
        public void CardFactory_CreateCard_Returns_Valid_Card_Test()
        {
            var factory = new CardFactory();
            var card = factory.CreateCard("Штурмовой юнит Аккретии");

            Assert.IsNotNull(card);
            Assert.AreEqual("Штурмовой юнит Аккретии", card.Name);
            Assert.AreEqual(Faction.Accretia, card.Faction);
        }

        [TestMethod]
        [ExpectedException(typeof(System.ArgumentException))]
        public void CardFactory_CreateCard_Throws_For_Invalid_Name_Test()
        {
            var factory = new CardFactory();
            factory.CreateCard("Несуществующая карта");
        }

        [TestMethod]
        public void CardFactory_GetAllCards_Returns_Cards_Test()
        {
            var factory = new CardFactory();
            var cards = factory.GetAllCards();

            Assert.IsNotNull(cards);
            Assert.IsTrue(cards.Count > 0);
            Assert.IsTrue(cards.Any(c => c.Faction == Faction.Accretia));
        }
    }

    [TestClass]
    public class PlayerTests
    {
        [TestMethod]
        public void Player_Initialization_Sets_Correct_Values_Test()
        {
            var player = new Player("Тестовый игрок", Faction.Accretia);

            Assert.AreEqual("Тестовый игрок", player.Name);
            Assert.AreEqual(Faction.Accretia, player.Faction);
            Assert.AreEqual(30, player.Health);
            Assert.AreEqual(0, player.Energy);
        }

        [TestMethod]
        public void Player_DrawCard_Moves_Card_From_Deck_To_Hand_Test()
        {
            var player = new Player("Тест", Faction.Accretia);
            var card = new CreatureCard { Name = "Тестовая карта" };
            player.Deck.Add(card);

            var drawnCard = player.DrawCard();
            Assert.AreEqual(card, drawnCard);
            Assert.IsTrue(player.Hand.Contains(card));
            Assert.IsFalse(player.Deck.Contains(card));
        }

        [TestMethod]
        public void Player_PlayCreatureCard_Success_When_Has_Enough_Energy_Test()
        {
            var player = new Player("Тест", Faction.Accretia);
            player.Energy = 5;
            var creature = new CreatureCard { Name = "Существо", Cost = 3 };
            player.Hand.Add(creature);

            bool success = player.PlayCreatureCard(creature);
            Assert.IsTrue(success);
            Assert.IsTrue(player.Field.Contains(creature));
            Assert.AreEqual(2, player.Energy);
        }
    }
}

namespace TestProject1
{
    [TestClass]
    public class GameEngineTests
    {
        [TestMethod]
        public void GameEngine_Initialization_Creates_Players_Test()
        {
            var engine = new GameEngine("Игрок 1", Faction.Accretia, "Игрок 2", Faction.Bellato);

            Assert.IsNotNull(engine.Player1);
            Assert.IsNotNull(engine.Player2);
            Assert.AreEqual("Игрок 1", engine.Player1.Name);
            Assert.AreEqual("Игрок 2", engine.Player2.Name);
        }

        [TestMethod]
        public void GameEngine_EndTurn_Swaps_Players_Test()
        {
            var engine = new GameEngine("Игрок 1", Faction.Accretia, "Игрок 2", Faction.Bellato);
            engine.StartGame();
            var firstPlayer = engine.CurrentPlayer;

            engine.EndTurn();
            Assert.AreNotEqual(firstPlayer, engine.CurrentPlayer);
        }
    }
}