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

        string[] lines = file.text.Split('\n');

        string[] linesToUse = new string[ignoreFirstLine ? lines.Length - 1 : lines.Length];

        Array.Copy(lines,
            ignoreFirstLine ? 1 : 0,
            linesToUse,
            0,
            ignoreFirstLine ? lines.Length - 1 : lines.Length);

        foreach (string rawLine in linesToUse)
        {

            string line = string.Concat(rawLine.Where((x) => !char.IsWhiteSpace(x)));

            if (line == "")
                continue;

            data.Add(line.Split(','));

        }

        return data.ToArray();

    }

}
