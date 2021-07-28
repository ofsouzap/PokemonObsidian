using System.Collections.Generic;

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

            id = int.Parse(entry[0]);
            name = entry[1];
            badgeName = entry[2];

            gymList.Add(new Gym()
            {
                id = id,
                name = name,
                badgeName = badgeName
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

}