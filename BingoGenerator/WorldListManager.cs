using F23.StringSimilarity;

namespace BingoGenerator
{
	internal class WorldListManager
	{
		private static List<string> wordList = new List<string>();

		private bool isAddingWords;
		private bool isRemovingWords;

		private string lastRemovedWord;

		public WorldListManager()
		{
			LoadWordListFromFile();
		}

		private void LoadWordListFromFile()
		{
			// Specify the file path where you saved the word list
			string filePath = "wordlist.txt";

			try
			{
				// Check if the file exists
				if (File.Exists(filePath))
				{
					// Read all lines from the file and add them to the word list
					wordList.AddRange(File.ReadAllLines(filePath));
					Console.WriteLine($"Word list loaded from: {filePath}");
				}
				else
				{
					Console.WriteLine("Word list file does not exist. Creating a new one.");
				}
			}
			catch (Exception ex)
			{
				Console.WriteLine($"Error loading word list: {ex.Message}");
			}
		}

		private void SaveWordListToFile()
		{
			string filePath = "wordlist.txt";

			try
			{
				// Write each word to the file
				File.WriteAllLines(filePath, wordList);
			}
			catch (Exception ex)
			{
				Console.WriteLine($"Error saving word list: {ex.Message}");
			}
		}

		public void Run()
		{
			while (true)
			{
				Console.Write("Enter a command (Add, Remove, Show, Back): ");
				string command = Console.ReadLine()?.Trim().ToLower();

				switch (command)
				{
					case "add":
						isAddingWords = true;
						ShowWordList();
						Console.WriteLine();
						AddWordsCommand();
						break;
					case "remove":
						isRemovingWords = true;
						Console.WriteLine();
						ShowWordList();
						RemoveWordsCommand();
						break;
					case "show":
						ShowWordList();
						break;
					case "back":
						SaveWordListToFile();
						Console.Clear();
						ShowWordList();
						return;
					default:
						Console.WriteLine("Invalid command. Please try again.");
						break;
				}
			}
		}

		public List<string> Read() => wordList;

		private void AddWordsCommand()
		{
			while (isAddingWords)
			{
				Console.Write("Enter a word to add to the list: ");
				string newWord = Console.ReadLine()?.Trim();

				if (!string.IsNullOrEmpty(newWord))
				{
					if (newWord.Trim().ToLower() == "undo")
					{
						if (wordList.Count > 0)
						{
							string removeWord = wordList[wordList.Count - 1];
							Console.WriteLine($"Removed last added word '{removeWord}'");
							wordList.RemoveAt(wordList.Count - 1);
							SaveWordListToFile();
						}
						else
						{
							Console.WriteLine("No words to undo.");
						}
					}
					else if (wordList.Contains(newWord))
					{
						Console.WriteLine($"Word list already contains '{newWord}'");
					}
					else
					{
						wordList.Add(newWord);
						SaveWordListToFile();
					}
				}
				else
				{
					isAddingWords = false;
				}
			}
		}

		private void RemoveWordsCommand()
		{
			while (isRemovingWords)
			{
				Console.Write("Enter a word to remove from the list: ");
				string wordToRemove = Console.ReadLine()?.Trim();

				if (!string.IsNullOrEmpty(wordToRemove))
				{
					if (wordToRemove.Trim().ToLower() == "undo")
					{
						if (!string.IsNullOrEmpty(lastRemovedWord))
						{
							Console.WriteLine($"Add last removed word '{lastRemovedWord}' back to the list.");
							wordList.Add(lastRemovedWord);
							lastRemovedWord = string.Empty;
							SaveWordListToFile();
						}
						else
						{
							Console.WriteLine($"No word to undo.");
						}
					}
					else if (wordList.Contains(wordToRemove, StringComparer.OrdinalIgnoreCase))
					{
						lastRemovedWord = wordToRemove;
						wordList.RemoveAll(word => string.Equals(word, wordToRemove, StringComparison.OrdinalIgnoreCase));
						Console.WriteLine($"Word '{wordToRemove}' removed from the list.");
						SaveWordListToFile();
					}
					else
					{
						string closestMatch = FindClosestMatch(wordToRemove, wordList.ToArray());

						if (!string.IsNullOrEmpty(closestMatch))
						{
							Console.WriteLine($"Did you mean '{closestMatch}'?");
							Console.Write("Enter 'yes' to confirm removal or any other key to cancel: ");
							string confirmation = Console.ReadLine()?.Trim().ToLower();

							if (confirmation == "yes" || confirmation == "confirm")
							{
								lastRemovedWord = closestMatch;
								wordList.RemoveAll(word => string.Equals(word, closestMatch, StringComparison.OrdinalIgnoreCase));
								Console.WriteLine($"Word '{closestMatch}' removed from the list.");
								SaveWordListToFile();
							}
							else
							{
								Console.WriteLine("Removal canceled.");
							}
						}
						else
						{
							Console.WriteLine($"Word '{wordToRemove}' not found in the list.");
						}
					}
				}
				else
				{
					isRemovingWords = false;
				}
			}
		}

		private void ShowWordList()
		{
			Console.WriteLine("Word List:");
			foreach (string word in wordList)
			{
				Console.WriteLine($"- {word}");
			}
		}

		private static string FindClosestMatch(string input, string[] wordList)
		{
			double bestScore = double.MinValue;
			string closestMatch = null;

			JaroWinkler jaroWinkler = new JaroWinkler();

			foreach (string word in wordList)
			{
				double similarity = jaroWinkler.Similarity(input, word);

				// You can adjust the threshold as needed
				if (similarity > bestScore && similarity >= 0.5)
				{
					bestScore = similarity;
					closestMatch = word;
				}
			}

			return closestMatch;
		}
	}
}
