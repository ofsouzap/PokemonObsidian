using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class CSV
{
    
    public static string[][] ReadCSVResource(string filePath,
        bool ignoreFirstLine = false)
    {

        List<string[]> data = new List<string[]>();

        TextAsset file = Resources.Load(filePath) as TextAsset;

        string rawFileText = file.text;
        string processedFileText = rawFileText.Replace("\r", string.Empty);

        string[] lines = processedFileText.Split('\n');

        string[] linesToUse = new string[ignoreFirstLine ? lines.Length - 1 : lines.Length];

        Array.Copy(lines,
            ignoreFirstLine ? 1 : 0,
            linesToUse,
            0,
            ignoreFirstLine ? lines.Length - 1 : lines.Length);

        foreach (string line in linesToUse)
        {

            if (line == "")
                continue;

            List<string> values = new List<string>();
            bool inQuoteValue = false;
            string currentValue = "";

            foreach (char c in line)
            {

                if (c == '"')
                {
                    inQuoteValue = !inQuoteValue;
                }
                else if (c == ',' && !inQuoteValue)
                {
                    values.Add(currentValue);
                    currentValue = "";
                }
                else
                {
                    currentValue += c;
                }

            }

            values.Add(currentValue);

            data.Add(values.ToArray());

        }

        return data.ToArray();

    }

}
