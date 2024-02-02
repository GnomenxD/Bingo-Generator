using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.Intrinsics.Arm;
using BingoGenerator;

internal class Program
{
	private const float GridLineWidth = 4.0f;
	private const int Dpi = 96;
	private const float MmToInchConversion = 0.0393701f;


	private static readonly int A4Width = (int)(297 * Dpi * MmToInchConversion);
	private static readonly int A4Height = (int)(210 * Dpi * MmToInchConversion);

	private static bool isPrinting;
	private static int numberOfMats;
	private static int numberOfRows;
	private static int numberOfColumns;

	private static WorldListManager worldListManager;

	private static void Main(string[] args)
	{
		Console.WriteLine("Welcome to Bingo Mat Generator!");
		worldListManager = new WorldListManager();

		while (true)
		{
			if(isPrinting)
			{
				PrintMats();
				continue;
			}

			Console.Write("Enter a command (Print, List, Quit): ");
			string command = Console.ReadLine()?.Trim().ToLower();

			switch (command)
			{
				case "print":
					Console.Clear();
					isPrinting = true;
					break;
				case "list":
					Console.Clear();
					worldListManager.Run();
					break;
				case "quit":
					Console.WriteLine("Goodbye!");
					return;
				default:
					Console.WriteLine("Invalid command. Please try again.");
					break;
			}
		}
	}

	private static void PrintMats()
	{
		Console.Write("Enter the number of mats to generate: ");
		if (int.TryParse(Console.ReadLine()?.Trim(), out numberOfMats) && numberOfMats > 0)
		{
			Console.Write("Enter the number of rows [4]: ");
			if (int.TryParse(Console.ReadLine()?.Trim(), out numberOfRows) && numberOfRows > 1)
			{
				Console.Write("Enter the number of columns [5]: ");
				if (int.TryParse(Console.ReadLine()?.Trim(), out numberOfColumns) && numberOfColumns > 1)
				{
					GenerateMats(worldListManager.Read(), numberOfMats, numberOfRows, numberOfColumns);
					isPrinting = false;
				}
				else
				{
					Console.WriteLine("Invalid input for the number of columns. Please try again.");
				}
			}
			else
			{
				Console.WriteLine("Invalid input for the number of rows. Please try again.");
			}
		}
	}

	private static void GenerateMats(List<string> wordList, int numberOfMats = 4, int rows = 4, int columns = 5)
	{
		if (wordList.Count < (rows + columns))
		{
			Console.WriteLine("Not enough words.");
			return;
		}

		const int matsPerPaper = 4;
		const int matSpacing = 8;

		int numberOfMatsToGenerate = numberOfMats;
		// Calculate the width and height of each cell in the grid
		int matWidth = A4Width / 2 - matSpacing * 2;
		int matHeight = A4Height / 2 - matSpacing * 2;
		int cellWidth = matWidth / columns;
		int cellHeight = matHeight / rows;

		int counter = 0;
		int paperIndex = 0;
		while (numberOfMats > 0)
		{
			using (Bitmap paper = new Bitmap(A4Width, A4Height))
			using (Graphics paperGraphics = Graphics.FromImage(paper))
			{
				paperGraphics.Clear(Color.White);

				List<string> wordsForHashCode = new List<string>();

				for (int i = 0; i < 4; i++)
				{
					if (numberOfMats <= 0)
						break;

					int matRow = i / 2;
					int matCol = i % 2;

					int startX = matCol * (matWidth) + matSpacing + (matCol > 0 ? matSpacing * 2 : 0);
					int startY = matRow * (matHeight) + matSpacing + (matRow > 0 ? matSpacing * 2 : 0);


					List<string> shuffledWords = new List<string>(wordList);

					// Create a new bitmap for each mat
					using (Bitmap mat = new Bitmap(matWidth, matHeight))
					using (Graphics matGraphics = Graphics.FromImage(mat))
					{
						// Set background color (optional)
						matGraphics.Clear(Color.White);

						// Randomly shuffle the word list for each mat
						Random rand = new Random();
						shuffledWords.Sort((x, y) => rand.Next(-1, 2));

						// Draw the grid with random words and gridlines
						for (int row = 0; row < rows; row++)
						{
							for (int col = 0; col < columns; col++)
							{
								int wordIndex = row * columns + col;
								if (wordIndex < shuffledWords.Count)
								{
									string word = shuffledWords[wordIndex];
									wordsForHashCode.Add(word);

									// Calculate the position for each cell
									int x = col * cellWidth;
									int y = row * cellHeight;

									// Draw the word in the cell
									using (StringFormat stringFormat = new StringFormat())
									{
										stringFormat.Alignment = StringAlignment.Center;
										stringFormat.LineAlignment = StringAlignment.Center;
										stringFormat.Trimming = StringTrimming.Word;

										// Draw the word in the cell with word wrapping
										RectangleF rect = new RectangleF(x, y, cellWidth, cellHeight);
										matGraphics.DrawString(word, new Font("Verdana", 11.5f), Brushes.Black, rect, stringFormat);

										matGraphics.DrawRectangle(new Pen(Color.Black, GridLineWidth), rect);
									}
								}
							}
						}

						paperGraphics.DrawImage(mat, startX, startY);
					}

					// Combine words for hashcode
					string combinedWords = string.Join(", ", shuffledWords.ToArray(), 0, numberOfRows * numberOfColumns);

					// Generate a hashcode for the words
					int wordsHashCode = combinedWords.GetHashCode();

					// Print the individual mat generation message including the words and hashcode
					Console.WriteLine($"Generating mat [{counter} / {numberOfMatsToGenerate}]: {wordsHashCode}");
					counter++;
					numberOfMats--;
				}

				paperIndex++;
			}
		}

		Console.WriteLine("Mats generated successfully.");
	}
}