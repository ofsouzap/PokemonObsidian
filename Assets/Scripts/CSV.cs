using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class CSV
{
    
    public static string[][] ReadCSVResource(string filePath)
    {

        List<string[]> data = new List<string[]>();

        TextAsset file = Resources.Load(filePath) as TextAsset;

        string[] lines = file.text.Split('\n');

        foreach (string rawLine in lines)
        {

            string line = string.Concat(rawLine.Where((x) => !char.IsWhiteSpace(x)));

            if (line == "")
                continue;

            data.Add(line.Split(','));

        }

        return data.ToArray();

    }

}
