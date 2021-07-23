using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using FreeRoaming;
using FreeRoaming.Menu;
using FreeRoaming.AreaControllers;

public static class GameSceneManager
{

    /// <summary>
    /// The identifier for the battle scene. This can be in any format that the Unity SceneManager can use
    /// </summary>
    public const string battleSceneIdentifier = "Battle";

    /// <summary>
    /// The identifier for the evolution scene. This can be in any format that the Unity SceneManager can use
    /// </summary>
    public const string evolutionSceneIdentifier = "Evolution Scene";

    public const float sceneChangePlayerMoveDelay = 0.5F;

    #region Scene Records

    //TODO (new GameSceneManager implementation) - scene stack

    private static bool battleSceneInUse;
    /// <summary>
    /// The battle scene if in use
    /// </summary>
    private static Scene battleScene;

    private static bool evolutionSceneInUse;
    private static Scene evolutionScene;

    private static bool playerMenuSceneInUse;
    private static Scene playerMenuScene;

    #endregion

    public static void Initialise()
    {

        //TODO - clear scene stack
        battleSceneInUse = false;
        evolutionSceneInUse = false;
        playerMenuSceneInUse = false;

    }

    private static GameObject PlayerGameObject => PlayerController.singleton.gameObject;
    private static GameObject FreeRoamMenuGameObject => FreeRoamMenuController.singleton.gameObject;

    private static FreeRoamSceneController GetFreeRoamSceneController(Scene scene)
    {

        List<FreeRoamSceneController> controllers = new List<FreeRoamSceneController>();

        foreach (GameObject go in scene.GetRootGameObjects())
            controllers.AddRange(go.GetComponentsInChildren<FreeRoamSceneController>());

        switch (controllers.Count)
        {

            case 0:
                Debug.LogWarning("Unable to find FreeRoamSceneController in scene");
                return null;

            case 1:
                return controllers[0];

            default:
                Debug.LogWarning("Multiple FreeRoamSceneController found in scene");
                return controllers[0];

        }

    }

    public delegate void OnComplete();

    #region Scene Changing

    #region Scene Fading

    private static event OnComplete FadeOutComplete;
    private static event OnComplete FadeInComplete;

    private static void StartFadeOut()
    {
        FadeOutComplete = null;
        BlackFadeController goController = UnityEngine.Object.Instantiate(Resources.Load<GameObject>("Prefabs/Black Fade")).GetComponent<BlackFadeController>();
        goController.FadeOut();
        goController.FadeCompleted += () => FadeOutComplete?.Invoke();
        goController.FadeCompleted += () => UnityEngine.Object.Destroy(goController.gameObject);
    }

    private static void StartFadeIn()
    {
        FadeInComplete = null;
        BlackFadeController goController = UnityEngine.Object.Instantiate(Resources.Load<GameObject>("Prefabs/Black Fade")).GetComponent<BlackFadeController>();
        goController.FadeIn();
        goController.FadeCompleted += () => FadeInComplete?.Invoke();
    }

    #endregion

    private static void RefreshCurrentSceneStack()
    {

        throw new NotImplementedException();
        //TODO (new GameSceneManager implementation)

    }

    public static void UseDoor(SceneDoorDetails doorDetails)
    {

        throw new NotImplementedException();
        //TODO (new GameSceneManager implementation)

    }

    #region Battle Opening/Closing

    public static void LaunchBattleScene()
    {

        throw new NotImplementedException();
        //TODO (new GameSceneManager implementation)

    }

    public static void CloseBattleScene()
    {

        throw new NotImplementedException();
        //TODO (new GameSceneManager implementation)

    }

    #endregion

    #region Evolution Scene Opening/Closing

    public static void LaunchEvolutionScene()
    {

        throw new NotImplementedException();
        //TODO (new GameSceneManager implementation)

    }

    public static event OnComplete EvolutionSceneClosed;

    public static void CloseEvolutionScene()
    {

        throw new NotImplementedException();
        //TODO (new GameSceneManager implementation)

    }

    #endregion

    #region Player Menu Scenes Opening/Closing

    public static void LaunchPlayerMenuScene(string sceneIdentifier)
    {

        throw new NotImplementedException();
        //TODO (new GameSceneManager implementation)

    }

    public static void ClosePlayerMenuScene()
    {

        throw new NotImplementedException();
        //TODO (new GameSceneManager implementation)

    }

    #endregion

    public static void OpenStartingScene(Scene startUpScene,
        string sceneName,
        GameObject playerGameObject,
        GameObject freeRoamMenuGameObject,
        Vector2Int playerSceneStartPosition)
    {

        throw new NotImplementedException();
        //TODO (new GameSceneManager implementation)

    }

    #endregion

}