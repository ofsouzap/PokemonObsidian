using System.Collections.Generic;
using UnityEngine;

public class Gym : IHasId
{

    private const string dataFileResourcesPath = "Data/gyms";

    #region Registry

    public static Registry<Gym> registry = new Registry<Gym>();

    private static bool registryLoaded = false;

    public static void TryLoadRegistry()
    {
        if (!registryLoaded)
            LoadRegistry();
    }

    private static void LoadRegistry()
    {

        List<Gym> gymList = new List<Gym>();

        string[][] gymData = CSV.ReadCSVResource(dataFileResourcesPath, true);

        foreach (string[] entry in gymData)
        {

            int id;
            string name, badgeName;
            byte obedienceCap;

            id = int.Parse(entry[0]);
            name = entry[1];
            badgeName = entry[2];
            
            if (!byte.TryParse(entry[3], out obedienceCap))
            {
                Debug.LogError("Invalid obedience cap for gym id - " + id);
                obedienceCap = 0;
            }

            gymList.Add(new Gym()
            {
                id = id,
                name = name,
                badgeName = badgeName,
                obedienceCap = obedienceCap
            });

        }

        registry.SetValues(gymList.ToArray());

        registryLoaded = true;

    }

    #endregion

    public int id;
    public int GetId() => id;

    public string name;
    public string badgeName;

    /// <summary>
    /// The obedience cap of the player once they have completed this gym
    /// </summary>
    public byte obedienceCap;

}