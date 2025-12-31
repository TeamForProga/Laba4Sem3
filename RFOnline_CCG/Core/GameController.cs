using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace RFCardGame.Core
{
    /// <summary>
    /// Улучшенный контроллер игры с поддержкой сохранения.
    /// </summary>
    public class EnhancedGameController
    {
        private GameEngine _gameEngine;
        private JsonGameStateService _stateService;
        private string _savesDirectory = "Saves";

        public EnhancedGameController()
        {
            _stateService = new JsonGameStateService();

            // Создаем директорию для сохранений если ее нет
            if (!Directory.Exists(_savesDirectory))
            {
                Directory.CreateDirectory(_savesDirectory);
            }
        }

        /// <summary>
        /// Главное меню.
        /// </summary>
        public void ShowMainMenu()
        {
            while (true)
            {
                Console.Clear();
                Console.WriteLine("=== Card Game ===");
                Console.WriteLine("1. Новая игра");
                Console.WriteLine("2. Загрузить игру");
                Console.WriteLine("3. Удалить сохранение");
                Console.WriteLine("4. Список сохранений");
                Console.WriteLine("5. Выход");
                Console.Write("Выберите действие: ");

                switch (Console.ReadLine())
                {
                    case "1":
                        StartNewGame();
                        break;
                    case "2":
                        LoadGameMenu();
                        break;
                    case "3":
                        DeleteSaveMenu();
                        break;
                    case "4":
                        ListSaves();
                        break;
                    case "5":
                        Console.WriteLine("До свидания!");
                        return;
                    default:
                        Console.WriteLine("Неверный выбор!");
                        Console.ReadKey();
                        break;
                }
            }
        }

        /// <summary>
        /// Меню загрузки игры.
        /// </summary>
        private void LoadGameMenu()
        {
            Console.Clear();
            Console.WriteLine("=== ЗАГРУЗКА ИГРЫ ===");

            var saveFiles = GetSaveFiles();
            if (!saveFiles.Any())
            {
                Console.WriteLine("Сохранений не найдено.");
                Console.WriteLine("Нажмите любую клавишу...");
                Console.ReadKey();
                return;
            }

            Console.WriteLine("\nДоступные сохранения:");
            for (int i = 0; i < saveFiles.Count; i++)
            {
                var fileInfo = new FileInfo(saveFiles[i]);
                Console.WriteLine($"{i + 1}. {Path.GetFileNameWithoutExtension(saveFiles[i])}");
                Console.WriteLine($"   Дата: {fileInfo.LastWriteTime}, Размер: {fileInfo.Length / 1024} KB");
            }

            Console.Write("\nВыберите сохранение (номер) или 0 для отмены: ");
            if (int.TryParse(Console.ReadLine(), out int choice) && choice > 0 && choice <= saveFiles.Count)
            {
                LoadGame(saveFiles[choice - 1]);
            }
            else if (choice != 0)
            {
                Console.WriteLine("Неверный выбор!");
                Console.ReadKey();
            }
        }

        /// <summary>
        /// Загрузить игру из файла.
        /// </summary>
        private void LoadGame(string filePath)
        {
            try
            {
                Console.WriteLine($"\nЗагрузка игры из {Path.GetFileName(filePath)}...");

                // Загружаем состояние
                var gameState = _stateService.LoadGameState(filePath);

                // Создаем новую игру на основе загруженного состояния
                _gameEngine = new GameEngine(
                    gameState.Player1Name,
                    gameState.Player1Faction,
                    gameState.Player2Name,
                    gameState.Player2Faction);

                // Загружаем состояние в движок
                _gameEngine.LoadFromState(gameState, _stateService);

                Console.WriteLine("Игра успешно загружена!");
                Console.WriteLine($"Игроки: {gameState.Player1Name} vs {gameState.Player2Name}");
                Console.WriteLine($"Ход: {gameState.CurrentTurn}");
                Console.WriteLine("Нажмите любую клавишу для продолжения...");
                Console.ReadKey();

                // Запускаем игровой цикл
                GameLoop();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка загрузки: {ex.Message}");
                Console.WriteLine("Нажмите любую клавишу...");
                Console.ReadKey();
            }
        }

        /// <summary>
        /// Получить список файлов сохранений.
        /// </summary>
        private List<string> GetSaveFiles()
        {
            var saveFiles = new List<string>();

            try
            {
                // Ищем файлы .json в директории сохранений
                var files = Directory.GetFiles(_savesDirectory, "*.json");
                foreach (var file in files)
                {
                    saveFiles.Add(file);
                }

                // Сортируем по дате изменения (новые сначала)
                saveFiles = saveFiles.OrderByDescending(f => new FileInfo(f).LastWriteTime).ToList();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при поиске сохранений: {ex.Message}");
            }

            return saveFiles;
        }

        /// <summary>
        /// Начать новую игру.
        /// </summary>
        private void StartNewGame()
        {
            Console.Clear();
            Console.WriteLine("=== НОВАЯ ИГРА ===");

            // Ввод данных игроков
            Console.Write("Имя игрока 1: ");
            string player1Name = Console.ReadLine()?.Trim() ?? "Игрок 1";

            Console.WriteLine("Выберите фракцию для " + player1Name + ":");
            Console.WriteLine("1. Аккретия (🔴 Атака)");
            Console.WriteLine("2. Беллато (🔵 Технологии)");
            Console.WriteLine("3. Кора (🟣 Магия)");
            Console.Write("Ваш выбор (1-3): ");

            Faction player1Faction = GetFactionFromInput();

            Console.Write("\nИмя игрока 2: ");
            string player2Name = Console.ReadLine()?.Trim() ?? "Игрок 2";

            Console.WriteLine("Выберите фракцию для " + player2Name + ":");
            Console.WriteLine("1. Аккретия (🔴 Атака)");
            Console.WriteLine("2. Беллато (🔵 Технологии)");
            Console.WriteLine("3. Кора (🟣 Магия)");
            Console.Write("Ваш выбор (1-3): ");

            Faction player2Faction = GetFactionFromInput();

            // Создаем игру
            _gameEngine = new GameEngine(player1Name, player1Faction, player2Name, player2Faction);
            _gameEngine.StartGame();

            Console.WriteLine("\nИгра началась! Нажмите любую клавишу...");
            Console.ReadKey();

            GameLoop();
        }

        /// <summary>
        /// Основной игровой цикл.
        /// </summary>
        private void GameLoop()
        {
            while (!_gameEngine.IsGameOver)
            {
                Console.Clear();
                DisplayGameState();

                Console.WriteLine("=== ДОСТУПНЫЕ ДЕЙСТВИЯ ===");
                Console.WriteLine("1. Показать карты в руке");
                Console.WriteLine("2. Сыграть карту существа");
                Console.WriteLine("3. Сыграть заклинание");
                Console.WriteLine("4. Атаковать существом");
                Console.WriteLine("5. Атаковать игрока напрямую");
                Console.WriteLine("6. Завершить ход");
                Console.WriteLine("7. Показать лог игры");
                Console.WriteLine("8. Сохранить игру");
                Console.WriteLine("0. Выйти в главное меню");
                Console.Write("Выбор: ");

                switch (Console.ReadLine())
                {
                    case "1":
                        ShowHand();
                        break;
                    case "2":
                        PlayCreatureCard();
                        break;
                    case "3":
                        PlaySpellCard();
                        break;
                    case "4":
                        AttackWithCreature();
                        break;
                    case "5":
                        AttackPlayerDirectly();
                        break;
                    case "6":
                        _gameEngine.EndTurn();
                        break;
                    case "7":
                        ShowGameLog();
                        break;
                    case "8":
                        SaveGame();
                        break;
                    case "0":
                        if (ConfirmExit())
                            return;
                        break;
                    default:
                        Console.WriteLine("Неверный выбор!");
                        break;
                }

                if (!_gameEngine.IsGameOver)
                {
                    Console.WriteLine("\nНажмите любую клавишу...");
                    Console.ReadKey();
                }
            }

            // Игра окончена
            ShowGameOver();
        }

        /// <summary>
        /// Сохранить игру.
        /// </summary>
        private void SaveGame()
        {
            try
            {
                Console.Write("Введите имя для сохранения (без пробелов): ");
                string saveName = Console.ReadLine()?.Trim();

                if (string.IsNullOrWhiteSpace(saveName))
                {
                    saveName = $"save_{DateTime.Now:yyyyMMdd_HHmmss}";
                }

                string fileName = Path.Combine(_savesDirectory, $"{saveName}.json");

                // Создаем состояние игры
                var gameState = _gameEngine.CreateGameState();

                // Сохраняем
                _stateService.SaveGameState(fileName, gameState);

                Console.WriteLine($"\nИгра сохранена как: {fileName}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка сохранения: {ex.Message}");
            }

            Console.WriteLine("Нажмите любую клавишу...");
            Console.ReadKey();
        }

        /// <summary>
        /// Меню удаления сохранений.
        /// </summary>
        private void DeleteSaveMenu()
        {
            Console.Clear();
            Console.WriteLine("=== УДАЛЕНИЕ СОХРАНЕНИЙ ===");

            var saveFiles = GetSaveFiles();
            if (!saveFiles.Any())
            {
                Console.WriteLine("Сохранений не найдено.");
                Console.WriteLine("Нажмите любую клавишу...");
                Console.ReadKey();
                return;
            }

            Console.WriteLine("\nВыберите сохранение для удаления:");
            for (int i = 0; i < saveFiles.Count; i++)
            {
                Console.WriteLine($"{i + 1}. {Path.GetFileNameWithoutExtension(saveFiles[i])}");
            }
            Console.WriteLine("0. Отмена");

            Console.Write("\nВыбор: ");
            if (int.TryParse(Console.ReadLine(), out int choice) && choice > 0 && choice <= saveFiles.Count)
            {
                Console.Write($"\nУдалить {Path.GetFileNameWithoutExtension(saveFiles[choice - 1])}? (y/n): ");
                if (Console.ReadLine()?.ToLower() == "y")
                {
                    try
                    {
                        File.Delete(saveFiles[choice - 1]);
                        Console.WriteLine("Сохранение удалено!");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Ошибка удаления: {ex.Message}");
                    }
                }
            }

            Console.WriteLine("\nНажмите любую клавишу...");
            Console.ReadKey();
        }

        /// <summary>
        /// Показать список сохранений.
        /// </summary>
        private void ListSaves()
        {
            Console.Clear();
            Console.WriteLine("=== СПИСОК СОХРАНЕНИЙ ===");

            var saveFiles = GetSaveFiles();
            if (!saveFiles.Any())
            {
                Console.WriteLine("Сохранений не найдено.");
            }
            else
            {
                foreach (var file in saveFiles)
                {
                    var info = new FileInfo(file);
                    Console.WriteLine($"\n{Path.GetFileNameWithoutExtension(file)}");
                    Console.WriteLine($"  Размер: {info.Length} байт");
                    Console.WriteLine($"  Изменено: {info.LastWriteTime}");

                    // Пытаемся прочитать информацию о сохранении
                    try
                    {
                        var gameState = _stateService.LoadGameState(file);
                        Console.WriteLine($"  Игроки: {gameState.Player1Name} vs {gameState.Player2Name}");
                        Console.WriteLine($"  Ход: {gameState.CurrentTurn}");
                        Console.WriteLine($"  Дата сохранения: {gameState.SaveDate}");
                    }
                    catch
                    {
                        Console.WriteLine($"  (Не удалось прочитать детали)");
                    }
                }
            }

            Console.WriteLine("\nНажмите любую клавишу...");
            Console.ReadKey();
        }

        #region Вспомогательные методы

        private void DisplayGameState()
        {
            Console.WriteLine(_gameEngine.GetGameStateSummary());
            Console.WriteLine("\n" + _gameEngine.GetBattlefieldInfo());
        }

        private Faction GetFactionFromInput()
        {
            while (true)
            {
                if (int.TryParse(Console.ReadLine(), out int choice))
                {
                    switch (choice)
                    {
                        case 1: return Faction.Accretia;
                        case 2: return Faction.Bellato;
                        case 3: return Faction.Cora;
                    }
                }
                Console.Write("Неверный выбор. Введите 1, 2 или 3: ");
            }
        }

        private bool ConfirmExit()
        {
            Console.Write("\nВернуться в главное меню? Несохраненный прогресс будет утерян. (y/n): ");
            return Console.ReadLine()?.ToLower() == "y";
        }

        private void ShowGameOver()
        {
            Console.Clear();
            Console.WriteLine("=== ИГРА ОКОНЧЕНА ===");
            Console.WriteLine($"Победитель: {_gameEngine.Winner?.Name ?? "Неизвестно"}!");
            Console.WriteLine($"Всего ходов: {_gameEngine.CurrentTurn}");

            Console.WriteLine("\nЛог последних событий:");
            var recentLogs = _gameEngine.GameLog.TakeLast(10).ToList();
            foreach (var log in recentLogs)
            {
                Console.WriteLine($"  {log}");
            }

            Console.WriteLine("\nНажмите любую клавишу для возврата в меню...");
            Console.ReadKey();
        }

        private void ShowHand()
        {
            Console.Clear();
            var currentPlayer = _gameEngine.CurrentPlayer;

            Console.WriteLine($"=== КАРТЫ В РУКЕ ({currentPlayer.Name}) ===");
            Console.WriteLine();

            if (!currentPlayer.Hand.Any())
            {
                Console.WriteLine("В руке нет карт.");
                return;
            }

            for (int i = 0; i < currentPlayer.Hand.Count; i++)
            {
                var card = currentPlayer.Hand[i];
                string canPlay = card.Cost <= currentPlayer.Energy ? "(Можно сыграть)" : "(Недостаточно энергии)";

                if (card is CreatureCard creature)
                {
                    Console.WriteLine($"{i + 1}. {card.Name} [{card.Faction}] - {card.Cost} энергии");
                    Console.WriteLine($"   Существо: {creature.Attack}/{creature.MaxHealth}");
                    Console.WriteLine($"   {canPlay}");
                }
                else if (card is SpellCard spell)
                {
                    Console.WriteLine($"{i + 1}. {card.Name} [{card.Faction}] - {card.Cost} энергии");
                    Console.WriteLine($"   Заклинание ({spell.Subtype}): сила {spell.Power}");
                    Console.WriteLine($"   Цель: {spell.TargetType}");
                    Console.WriteLine($"   {canPlay}");
                }
                else if (card is ArtifactCard artifact)
                {
                    Console.WriteLine($"{i + 1}. {card.Name} [{card.Faction}] - {card.Cost} энергии");
                    Console.WriteLine($"   Артефакт");
                    Console.WriteLine($"   {canPlay}");
                }
                Console.WriteLine();
            }
        }

        private void PlayCreatureCard()
        {
            Console.Clear();
            ShowHand();

            var currentPlayer = _gameEngine.CurrentPlayer;

            if (!currentPlayer.Hand.Any())
            {
                Console.WriteLine("В руке нет карт для игры.");
                return;
            }

            Console.Write("Выберите карту существа для игры (номер): ");
            if (int.TryParse(Console.ReadLine(), out int cardIndex) && cardIndex >= 1 && cardIndex <= currentPlayer.Hand.Count)
            {
                var card = currentPlayer.Hand[cardIndex - 1];

                if (card is not CreatureCard creatureCard)
                {
                    Console.WriteLine("Это не карта существа!");
                    return;
                }

                bool success = _gameEngine.PlayCreatureCard(creatureCard);
                if (success)
                {
                    Console.WriteLine($"Карта {creatureCard.Name} успешно сыграна!");
                }
                else
                {
                    Console.WriteLine($"Не удалось сыграть карту {creatureCard.Name}.");
                }
            }
            else
            {
                Console.WriteLine("Неверный выбор карты.");
            }
        }

        private void PlaySpellCard()
        {
            Console.Clear();
            ShowHand();

            var currentPlayer = _gameEngine.CurrentPlayer;

            if (!currentPlayer.Hand.Any())
            {
                Console.WriteLine("В руке нет карт для игры.");
                return;
            }

            Console.Write("Выберите заклинание для игры (номер): ");
            if (int.TryParse(Console.ReadLine(), out int cardIndex) && cardIndex >= 1 && cardIndex <= currentPlayer.Hand.Count)
            {
                var card = currentPlayer.Hand[cardIndex - 1];

                if (card is not SpellCard spellCard)
                {
                    Console.WriteLine("Это не заклинание!");
                    return;
                }

                if (spellCard.TargetType == "SingleTarget")
                {
                    // Выбор цели для одиночного заклинания
                    Console.WriteLine("Выберите цель для заклинания:");

                    if (spellCard.Subtype == SpellSubtype.Attack || spellCard.Subtype == SpellSubtype.Buff)
                    {
                        Console.WriteLine("1. Ваше существо");
                        Console.WriteLine("2. Существо противника");
                        Console.Write("Ваш выбор: ");

                        if (int.TryParse(Console.ReadLine(), out int targetChoice))
                        {
                            ICreatureCard target = null;

                            if (targetChoice == 1)
                            {
                                if (currentPlayer.Field.Any())
                                {
                                    Console.WriteLine("Ваши существа:");
                                    for (int i = 0; i < currentPlayer.Field.Count; i++)
                                    {
                                        var creature = currentPlayer.Field[i];
                                        Console.WriteLine($"{i + 1}. {creature.Name} ({creature.Attack}/{creature.CurrentHealth})");
                                    }
                                    Console.Write("Выберите существо: ");
                                    if (int.TryParse(Console.ReadLine(), out int creatureIndex) &&
                                        creatureIndex >= 1 && creatureIndex <= currentPlayer.Field.Count)
                                    {
                                        target = currentPlayer.Field[creatureIndex - 1];
                                    }
                                }
                                else
                                {
                                    Console.WriteLine("У вас нет существ на поле!");
                                    return;
                                }
                            }
                            else if (targetChoice == 2)
                            {
                                var opponent = _gameEngine.OpponentPlayer;
                                if (opponent.Field.Any())
                                {
                                    Console.WriteLine("Существа противника:");
                                    for (int i = 0; i < opponent.Field.Count; i++)
                                    {
                                        var creature = opponent.Field[i];
                                        Console.WriteLine($"{i + 1}. {creature.Name} ({creature.Attack}/{creature.CurrentHealth})");
                                    }
                                    Console.Write("Выберите существо: ");
                                    if (int.TryParse(Console.ReadLine(), out int creatureIndex) &&
                                        creatureIndex >= 1 && creatureIndex <= opponent.Field.Count)
                                    {
                                        target = opponent.Field[creatureIndex - 1];
                                    }
                                }
                                else
                                {
                                    Console.WriteLine("У противника нет существ на поле!");
                                    return;
                                }
                            }

                            if (target != null)
                            {
                                bool success = _gameEngine.PlaySpellCard(spellCard, target);
                                if (success)
                                {
                                    Console.WriteLine($"Заклинание {spellCard.Name} успешно применено!");
                                }
                            }
                        }
                    }
                }
                else
                {
                    bool success = _gameEngine.PlaySpellCard(spellCard, null);
                    if (success)
                    {
                        Console.WriteLine($"Заклинание {spellCard.Name} успешно применено!");
                    }
                }
            }
        }

        private void AttackWithCreature()
        {
            Console.Clear();
            var currentPlayer = _gameEngine.CurrentPlayer;
            var opponent = _gameEngine.OpponentPlayer;

            if (!currentPlayer.Field.Any(c => c.IsAlive && c.State == CreatureState.Active))
            {
                Console.WriteLine("У вас нет существ, которые могут атаковать.");
                return;
            }

            if (!opponent.Field.Any(c => c.IsAlive))
            {
                Console.WriteLine("У противника нет существ для атаки.");
                return;
            }

            Console.WriteLine("=== ВАШИ СУЩЕСТВА ===");
            for (int i = 0; i < currentPlayer.Field.Count; i++)
            {
                var creature = currentPlayer.Field[i];
                string attackStatus = creature.IsAlive && creature.State == CreatureState.Active
                    ? "Может атаковать"
                    : "Не может атаковать";

                Console.WriteLine($"{i + 1}. {creature.Name} ({creature.Attack}/{creature.CurrentHealth}) - {attackStatus}");
            }

            Console.Write("Выберите существо для атаки (номер): ");
            if (int.TryParse(Console.ReadLine(), out int attackerIndex) &&
                attackerIndex >= 1 && attackerIndex <= currentPlayer.Field.Count)
            {
                var attacker = currentPlayer.Field[attackerIndex - 1];

                if (!attacker.IsAlive || attacker.State != CreatureState.Active)
                {
                    Console.WriteLine("Это существо не может атаковать сейчас.");
                    return;
                }

                Console.WriteLine();
                Console.WriteLine("=== СУЩЕСТВА ПРОТИВНИКА ===");
                for (int i = 0; i < opponent.Field.Count; i++)
                {
                    var creature = opponent.Field[i];
                    if (creature.IsAlive)
                    {
                        Console.WriteLine($"{i + 1}. {creature.Name} ({creature.Attack}/{creature.CurrentHealth})");
                    }
                }

                Console.Write("Выберите цель для атаки (номер): ");
                if (int.TryParse(Console.ReadLine(), out int defenderIndex) &&
                    defenderIndex >= 1 && defenderIndex <= opponent.Field.Count)
                {
                    var defender = opponent.Field[defenderIndex - 1];

                    if (!defender.IsAlive)
                    {
                        Console.WriteLine("Цель уже мертва.");
                        return;
                    }

                    bool success = _gameEngine.AttackWithCreature(attacker, defender);
                    if (success)
                    {
                        Console.WriteLine("Атака выполнена успешно!");
                    }
                }
                else
                {
                    Console.WriteLine("Неверный выбор цели.");
                }
            }
            else
            {
                Console.WriteLine("Неверный выбор атакующего.");
            }
        }

        private void AttackPlayerDirectly()
        {
            Console.Clear();
            var currentPlayer = _gameEngine.CurrentPlayer;
            var opponent = _gameEngine.OpponentPlayer;

            if (opponent.Field.Any(c => c.IsAlive))
            {
                Console.WriteLine("Нельзя атаковать игрока напрямую, пока у него есть существа.");
                return;
            }

            if (!currentPlayer.Field.Any(c => c.IsAlive && c.State == CreatureState.Active))
            {
                Console.WriteLine("У вас нет существ, которые могут атаковать.");
                return;
            }

            Console.WriteLine("=== ВАШИ СУЩЕСТВА ===");
            for (int i = 0; i < currentPlayer.Field.Count; i++)
            {
                var creature = currentPlayer.Field[i];
                if (creature.IsAlive && creature.State == CreatureState.Active)
                {
                    Console.WriteLine($"{i + 1}. {creature.Name} ({creature.Attack}/{creature.CurrentHealth})");
                }
            }

            Console.Write("Выберите существо для атаки (номер): ");
            if (int.TryParse(Console.ReadLine(), out int attackerIndex) &&
                attackerIndex >= 1 && attackerIndex <= currentPlayer.Field.Count)
            {
                var attacker = currentPlayer.Field[attackerIndex - 1];

                if (!attacker.IsAlive || attacker.State != CreatureState.Active)
                {
                    Console.WriteLine("Это существо не может атаковать сейчас.");
                    return;
                }

                bool success = _gameEngine.AttackPlayerDirectly(attacker);
                if (success)
                {
                    Console.WriteLine("Прямая атака выполнена успешно!");
                }
            }
            else
            {
                Console.WriteLine("Неверный выбор.");
            }
        }

        private void ShowGameLog()
        {
            Console.Clear();
            Console.WriteLine("=== ЛОГ ИГРЫ ===");
            Console.WriteLine();

            var logs = _gameEngine.GameLog;
            int startIndex = Math.Max(0, logs.Count - 20);

            for (int i = startIndex; i < logs.Count; i++)
            {
                Console.WriteLine(logs[i]);
            }
        }

        #endregion
    }
}