using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using Serialization;

public static class Saving
{

    public const string saveTimeDisplayFormat = "dd/MM/yy HH:mm";

    private static readonly string[] saveSlotFileNames = new string[]
    {
        "save-slot-0.dat",
        "save-slot-1.dat",
        "save-slot-2.dat",
        "save-slot-3.dat",
        "save-slot-4.dat"
    };

    public static string GetSaveSlotIndexFullPath(int saveSlotIndex)
    {

        if (SaveSlotIndexIsInRange(saveSlotIndex))
        {
            return Path.Combine(Application.persistentDataPath, saveSlotFileNames[saveSlotIndex]);
        }
        else
            throw new IndexOutOfRangeException("Save slot index out of range - " + saveSlotIndex);

    }

    private static bool SaveSlotIndexIsInRange(int saveSlotIndex)
    {
        if (saveSlotIndex < 0)
            return false;
        else if (saveSlotIndex >= saveSlotFileNames.Length)
            return false;
        else
            return true;
    }

    #region Saving

    public static void SaveData(int saveSlotIndex)
        => SaveData(Serialize.SerializeData(), saveSlotIndex);

    public static void SaveData(byte[] data, int saveSlotIndex)
    {

        if (SaveSlotIndexIsInRange(saveSlotIndex))
        {
            SaveData(data, GetSaveSlotIndexFullPath(saveSlotIndex));
        }
        else
        {
            throw new IndexOutOfRangeException("Save slot index out of range - " + saveSlotIndex);
        }

    }

    public static void SaveData(byte[] data, string filename)
    {

        FileStream stream = File.OpenWrite(filename);
        stream.Write(data, 0, data.Length);
        stream.Close();

    }

    #endregion

    #region Loading

    public struct LoadedData
    {

        public enum Status
        {
            Success,
            NoData,
            Invalid
        }

        public static readonly Dictionary<Status, Func<LoadedData, string>> dataStatusMessages = new Dictionary<Status, Func<LoadedData, string>>()
        {
            { Status.Success, data => EpochTime.EpochTimeToDateTime(data.saveTime).ToString(Saving.saveTimeDisplayFormat) },
            { Status.NoData, data => "(No Data)" },
            { Status.Invalid, data => "(Data Corrupted)" }
        };

        public string StatusMessage
            => dataStatusMessages[status].Invoke(this);

        public Status status;

        public long saveTime;
        public PlayerData playerData;
        public GameSceneManager.SceneStack sceneStack;

        public LoadedData(Status status)
        {

            if (status == Status.Success)
                throw new ArgumentException("This initialiser should only be used when the data failed to load");

            this.status = status;

            saveTime = default;
            playerData = default;
            sceneStack = default;


        }

        public LoadedData(Status status, long saveTime, PlayerData playerData, GameSceneManager.SceneStack sceneStack)
        {
            this.status = status;
            this.saveTime = saveTime;
            this.playerData = playerData;
            this.sceneStack = sceneStack;
        }

    }

    public static IEnumerable<LoadedData> LoadAllSlotDatas()
    {
        for (int i = 0; i < saveSlotFileNames.Length; i++)
            yield return LoadData(i);
    }

    public static LoadedData LoadData(int saveSlotIndex)
    {

        if (SaveSlotIndexIsInRange(saveSlotIndex))
        {
            return LoadData(GetSaveSlotIndexFullPath(saveSlotIndex));
        }
        else
        {
            throw new IndexOutOfRangeException("Save slot index out of range - " + saveSlotIndex);
        }

    }

    public static LoadedData LoadData(string filename)
    {

        byte[] data;

        try
        {
            data = File.ReadAllBytes(filename);
        }
        catch (FileNotFoundException)
        {
            return new LoadedData(LoadedData.Status.NoData);
        }

        try
        {

            Serialize.DeserializeData(data, 0,
                out long saveTime,
                out PlayerData playerData,
                out GameSceneManager.SceneStack sceneStack,
                out _);

            return new LoadedData(LoadedData.Status.Success, saveTime, playerData, sceneStack);

        }
        catch (Exception e)
        {

            Debug.LogWarning("Failed to load save data:\n" + e.Message);

            return new LoadedData(LoadedData.Status.Invalid);

        }

    }

    public static void LaunchLoadedData(LoadedData data)
    {

        if (data.status != LoadedData.Status.Success)
        {
            Debug.LogError("Data isn't loaded successfully");
        }

        PlayerData.singleton = data.playerData;

        FreeRoaming.PlayerController.singleton.RefreshSpriteFromPlayerData();

        GameSceneManager.LoadSceneStack(data.sceneStack);

    }

    #endregion

}
