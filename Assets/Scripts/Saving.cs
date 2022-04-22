using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using Serialization;
using Audio;

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

    private const string autosaveFileName = "save-slot-auto.dat";
    public static string AutosaveFilePath => GetSaveFileFullPath(autosaveFileName);

    private static string GetSaveFileFullPath(string filename)
        => Path.Combine(Application.persistentDataPath, filename);

    public static string GetSaveSlotIndexFullPath(int saveSlotIndex)
    {

        if (SaveSlotIndexIsInRange(saveSlotIndex))
        {
            return GetSaveFileFullPath(saveSlotFileNames[saveSlotIndex]);
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
    {

        if (SaveSlotIndexIsInRange(saveSlotIndex))
        {
            SaveData(GetSaveSlotIndexFullPath(saveSlotIndex));
        }
        else
        {
            throw new IndexOutOfRangeException("Save slot index out of range - " + saveSlotIndex);
        }

    }

    public static void Autosave()
    {
        SaveData(AutosaveFilePath);
    }

    public static void SaveData(string filePath)
    {

        FileStream stream = File.OpenWrite(filePath);
        Serialize.SerializeData(stream);
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
            { Status.Success, data => EpochTime.EpochTimeToFormattedLocalTime(data.saveTime) },
            { Status.NoData, data => "(No Data)" },
            { Status.Invalid, data => "(Data Corrupted)" }
        };

        public string StatusMessage
            => dataStatusMessages[status].Invoke(this);

        public Status status;

        public long saveTime;
        public PlayerData playerData;
        public GameSettings gameSettings;
        public GameSceneManager.SceneStack sceneStack;

        public LoadedData(Status status)
        {

            if (status == Status.Success)
                throw new ArgumentException("This initialiser should only be used when the data failed to load");

            this.status = status;

            saveTime = default;
            playerData = default;
            gameSettings = default;
            sceneStack = default;


        }

        public LoadedData(Status status, long saveTime, PlayerData playerData, GameSettings gameSettings, GameSceneManager.SceneStack sceneStack)
        {
            this.status = status;
            this.saveTime = saveTime;
            this.playerData = playerData;
            this.gameSettings = gameSettings;
            this.sceneStack = sceneStack;
        }

    }

    public static IEnumerable<LoadedData> LoadAllSlotData()
    {
        for (int i = 0; i < saveSlotFileNames.Length; i++)
            yield return LoadData(i);
    }

    public static LoadedData LoadAutosave()
        => LoadData(AutosaveFilePath);

    public static LoadedData LoadData(int saveIndex)
        => LoadData(GetSaveSlotIndexFullPath(saveIndex));

    public static LoadedData LoadData(string filename)
    {

        FileStream fileStream;

        try
        {
            fileStream = File.OpenRead(filename);
        }
        catch (FileNotFoundException)
        {
            return new LoadedData(LoadedData.Status.NoData);
        }

        try
        {

            long dataStartPos = fileStream.Position;

            ushort matchingSerializerVersion = Serialize.DeserializeSaveDataSerializerNumber(fileStream);

            //As the serializer version was read from the file stream, the stream position must be reset to where it was before this was read
            fileStream.Position = dataStartPos;

            Serialize.DeserializeData(versionNumber: matchingSerializerVersion,
                stream: fileStream,
                out long saveTime,
                out PlayerData playerData,
                out GameSettings gameSettings,
                out GameSceneManager.SceneStack sceneStack);
            
            return new LoadedData(LoadedData.Status.Success, saveTime, playerData, gameSettings, sceneStack);

        }
        catch (Serializer.SerializerVersionMismatchException e)
        {

            Debug.LogWarning($"Serializer Version Mismatch for file {filename} (intended - {e.IntendedSerializerVersion}, used - {e.UsedSerializerVersion})");
            return new LoadedData(LoadedData.Status.Invalid);
            
        }
        catch (Exception e)
        {

            Debug.LogWarning("Failed to load save data:\n" + e.Message + '\n' + e.StackTrace);

            return new LoadedData(LoadedData.Status.Invalid);

        }
        finally
        {
            fileStream.Close();
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

        GameSettings.singleton = data.gameSettings;

        MusicSourceController.singleton.SetVolume(GameSettings.singleton.musicVolume);

        GameSceneManager.LoadSceneStack(data.sceneStack);

    }

    #endregion

}
