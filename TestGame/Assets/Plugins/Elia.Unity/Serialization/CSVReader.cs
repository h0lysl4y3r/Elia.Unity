using System;
using System.Linq;
using UnityEngine;

namespace Elia.Unity.Serialization
{
    /// <summary>
    /// Super simple reader of CSV files.
    /// </summary>
	public static class CSVReader
	{
        /// <summary>
        /// Creates cell grid by splitting the CSV text with "\r\n" string to create rows (lines) and <see cref="SplitCsvLine"/> to create cells.
        /// </summary>
        /// <param name="csvText">CSV text</param>
        /// <returns>Grid of splitted CSV values</returns>
		public static string[,] SplitCsvGrid(string csvText)
		{
			//string[] lines = csvText.Split("\r\n"[0]);
			string[] lines = csvText.Split(new string[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries);

			// finds the max width of row
			int width = 0;
			for (int i = 0; i < lines.Length; i++)
			{
				string[] row = SplitCsvLine(lines[i]);
				width = Mathf.Max(width, row.Length);
			}

			// creates new 2D string grid to output to
			string[,] outputGrid = new string[width + 1, lines.Length + 1];
			for (int y = 0; y < lines.Length; y++)
			{
				string[] row = SplitCsvLine(lines[y]);
				for (int x = 0; x < row.Length; x++)
				{
					outputGrid[x, y] = row[x];

					// This line was to replace "" with " in my output. 
					// Include or edit it as you wish.
					outputGrid[x, y] = outputGrid[x, y].Replace("\"\"", "\"");
				}
			}

			return outputGrid;
		}

        /// <summary>
        /// Splits single CSV text row (line) into cells.
        /// </summary>
        /// <param name="line">CSV text row (line)</param>
        /// <returns>Array of CSV text cells of single row (line)</returns>
		public static string[] SplitCsvLine(string line)
		{
			return (from System.Text.RegularExpressions.Match m in System.Text.RegularExpressions.Regex.Matches(line,
			@"(((?<x>(?=[,\r\n]+))|""(?<x>([^""]|"""")+)""|(?<x>[^,\r\n]+)),?)",
			System.Text.RegularExpressions.RegexOptions.ExplicitCapture)
					select m.Groups[1].Value).ToArray();
		}
	}
}

