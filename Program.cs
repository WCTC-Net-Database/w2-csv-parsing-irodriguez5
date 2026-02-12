using System;
using System.IO;

/// <summary>
/// Week 2: Enhanced File I/O and Parsing
///
/// This program teaches fundamental file operations in C#:
/// - Reading data from CSV files using File.ReadAllLines()
/// - Parsing comma-separated values using String.Split()
/// - Writing data back to files using File.WriteAllLines()
/// </summary>
class Program
{
    // The path to our data file - we'll read and write character data here
    static string filePath = "input.csv";
    static readonly string HeaderLine = "Name,Class,Level,HP,Equipment";

    static void Main()
    {
        // Welcome message
        Console.WriteLine("=== Console RPG Character Manager ===");
        Console.WriteLine("Week 2: File I/O Arrays and Complex CSV Parsing\n");

        // README: Menu structure requirement (Tasks section: Menu) 
        // 1. Display Characters, 2. Add Character, 3. Level Up Character, 0. Exit
        bool running = true;
        while (running)
        {
            // Display the menu options
            DisplayMenu();

            // Get user's choice
            Console.Write("\nEnter your choice: ");
            string? choice = Console.ReadLine();
            choice = choice?.Trim();

            // Process the user's choice using a switch statement
            switch (choice)
            {
                case "1":
                    DisplayAllCharacters();
                    break;
                case "2":
                    AddCharacter();
                    break;
                case "3":
                    LevelUpCharacter();
                    break;
                case "0":
                    running = false;
                    Console.WriteLine("\nGoodbye! Thanks for playing.");
                    break;
                default:
                    Console.WriteLine("\nInvalid choice. Please try again.");
                    break;
            }

            if (running)
            {
                Console.WriteLine("\nPress any key to continue...");
                Console.ReadKey();
                Console.Clear();
            }
        }
    }

    static void DisplayMenu()
    {
        Console.WriteLine("What would you like to do?");
        Console.WriteLine("1. Display All Characters");
        Console.WriteLine("2. Add New Character");
        Console.WriteLine("3. Level Up Character");
        Console.WriteLine("0. Exit");
    }

    static void DisplayAllCharacters()
    {
        Console.WriteLine("\n=== All Characters ===\n");

        if (!File.Exists(filePath))
        {
            Console.WriteLine("Data file not found.");
            return;
        }
        string[] lines = File.ReadAllLines(filePath);

        if (lines.Length == 0)
        {
            Console.WriteLine("No characters found.");
            return;
        }

        for (int i = 0; i < lines.Length; i++)
        {
            var line = lines[i];
            if (string.IsNullOrWhiteSpace(line))
                continue;

            // README Task 1: Handle Header Rows
            // Skip the header line if it matches "Name,Class,Level,HP,Equipment" (case/whitespace-insensitive)
            if (i == 0 && string.Equals(line.Trim(), HeaderLine, StringComparison.OrdinalIgnoreCase))
                continue;

            // README Task 2: Parse Quoted Names
            // README Learning Objectives: Use Trim/Split
            if (!TryParseCsvLine(line, out var name, out var profession, out var level, out var hp, out var equipmentRaw))
            {
                Console.WriteLine("Skipping malformed line.");
                continue;
            }

            // README Task 3: Handle Equipment Arrays
            // Parse equipment pipe-separated list into an array, trimming entries and skipping empties
            var equipment = equipmentRaw.Split('|', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

            Console.WriteLine($"\nName: {name}");
            Console.WriteLine($"Profession: {profession}");
            Console.WriteLine($"Level: {level}");
            Console.WriteLine($"HP: {hp}");
            Console.WriteLine("Equipment:");
            if (equipment.Length == 0)
            {
                Console.WriteLine(" - (none)");
            }
            else
            {
                foreach (var eq in equipment)
                    Console.WriteLine($" - {eq}");
            }
            Console.WriteLine("___________________________________");
        }
    }

    static void AddCharacter()
    {
        Console.WriteLine("\n=== Add New Character ===\n");

        Console.Write("Enter character name: ");
        string? name = Console.ReadLine();
        name = (name ?? string.Empty).Trim();

        Console.Write("Enter character class: ");
        string? profession = Console.ReadLine();
        profession = (profession ?? string.Empty).Trim();

        int levelValue;
        while (true)
        {
            Console.Write("Enter character level (number): ");
            string? levelInput = Console.ReadLine();
            if (int.TryParse(levelInput?.Trim(), out levelValue))
                break;
            Console.WriteLine("Invalid level. Please enter a number.");
        }

        int hpValue;
        while (true)
        {
            Console.Write("Enter character HP (number): ");
            string? hpInput = Console.ReadLine();
            if (int.TryParse(hpInput?.Trim(), out hpValue))
                break;
            Console.WriteLine("Invalid HP. Please enter a number.");
        }

        Console.Write("Enter character equipment (separate items with '|'): ");
        string? equipment = Console.ReadLine();
        equipment = (equipment ?? string.Empty).Trim();

        // README Task 4: Write Back Correctly
        // When saving, re-quote names with commas and preserve pipe-separated equipment.
        string newLine = BuildCsvLine(name, profession!, levelValue, hpValue, equipment!);

        // README: File I/O Operations (Grading rubric) - append new rows in CSV format
        File.AppendAllText(filePath, newLine + Environment.NewLine);
        Console.WriteLine("Character added.");
    }

    static void LevelUpCharacter() //copilot was used for some help with this method
    {
        Console.WriteLine("\n=== Level Up Character ===\n");

        if (!File.Exists(filePath))
        {
            Console.WriteLine("Data file not found.");
            return;
        }

        string[] lines = File.ReadAllLines(filePath);
        if (lines.Length == 0)
        {
            Console.WriteLine("No characters found.");
            return;
        }

        Console.WriteLine("Character List: ");
        var dataLineIndices = new List<int>();
        for (int i = 0; i < lines.Length; i++)
        {
            var line = lines[i];
            if (string.IsNullOrWhiteSpace(line)) continue;

            // README Task 1: Skip header when listing selectable characters
            if (i == 0 && string.Equals(line.Trim(), HeaderLine, StringComparison.OrdinalIgnoreCase)) continue;

            // README Task 2: Use robust parsing including quoted names
            if (TryParseCsvLine(line, out var name, out _, out var level, out _, out _))
            {
                dataLineIndices.Add(i);
                Console.WriteLine($"{dataLineIndices.Count}. {name}: Level {level}");
            }
        }

        if (dataLineIndices.Count == 0)
        {
            Console.WriteLine("No characters found.");
            return;
        }

        Console.Write("\nEnter the number of the character to level up: ");
        string? input = Console.ReadLine();
        if (!int.TryParse(input?.Trim(), out int selection))
        {
            Console.WriteLine("Invalid input. Please enter a number.");
            return;
        }

        int listIndex = selection - 1;
        if (listIndex < 0 || listIndex >= dataLineIndices.Count)
        {
            Console.WriteLine("Selection out of range.");
            return;
        }

        int lineIndex = dataLineIndices[listIndex];
        var lineToParse = lines[lineIndex];

        // README Task 2: Parse line fields including possible quoted name
        if (!TryParseCsvLine(lineToParse, out var nameField, out var classField, out var levelField, out var hpField, out var equipmentField))
        {
            Console.WriteLine("Selected character data is malformed.");
            return;
        }

        if (!int.TryParse(levelField, out int currentLevel))
        {
            Console.WriteLine("Character level is not a valid number.");
            return;
        }

        int newLevel = currentLevel + 1;

        // README Task 4: Write Back Correctly
        // Persist updated level, preserving CSV format and quoting rules.
        lines[lineIndex] = BuildCsvLine(nameField, classField, newLevel, int.Parse(hpField), equipmentField);
        File.WriteAllLines(filePath, lines);

        // README Task 3: Equipment arrays handled via pipe-split, show item count
        var equipmentCount = string.IsNullOrWhiteSpace(equipmentField)
            ? 0
            : equipmentField.Split('|', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).Length;

        Console.WriteLine($"Leveled up {nameField}: {classField}, Level {currentLevel} -> {newLevel}, HP {hpField}, Equipment items: {equipmentCount}.");
    }

    // CSV helpers

    static bool TryParseCsvLine(string line, out string name, out string profession, out string level, out string hp, out string equipment)
    {
        name = profession = level = hp = equipment = string.Empty;
        if (string.IsNullOrWhiteSpace(line)) return false;
        line = line.Trim();

        // README Task 2: Parse Quoted Names
        // Detect leading quote, find closing quote using IndexOf, then Substring to extract the name.
        if (line.StartsWith("\""))
        {
            int closingQuote = line.IndexOf('\"', 1);              // README: String.IndexOf usage
            if (closingQuote <= 0) return false;

            name = line.Substring(1, closingQuote - 1).Trim();     // README: String.Substring + Trim

            int afterQuote = closingQuote + 1;
            if (afterQuote >= line.Length || line[afterQuote] != ',') return false;

            string remainder = line[(afterQuote + 1)..];
            var rest = remainder.Split(',');
            if (rest.Length < 4) return false;

            profession = rest[0].Trim();
            level = rest[1].Trim();
            hp = rest[2].Trim();
            equipment = rest[3].Trim();
            return true;
        }

        // Plain CSV line (no quoted name)
        var cols = line.Split(',');
        if (cols.Length < 5) return false;

        name = cols[0].Trim();
        profession = cols[1].Trim();
        level = cols[2].Trim();
        hp = cols[3].Trim();
        equipment = cols[4].Trim();
        return true;
    }

    static string BuildCsvLine(string name, string profession, int level, int hp, string equipment)
    {
        // README Task 4: Re-quote names that contain commas when writing
        string safeName = name.Contains(',') ? $"\"{name}\"" : name;
        return $"{safeName},{profession},{level},{hp},{equipment}";
    }
}