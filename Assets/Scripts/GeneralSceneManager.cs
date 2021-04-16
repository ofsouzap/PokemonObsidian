using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

public abstract class GeneralSceneManager : MonoBehaviour
{

    public Scene Scene => gameObject.scene;

    protected bool sceneEnabled = true;
    public bool SceneIsEnabled => sceneEnabled;

    protected List<Camera> camerasToReEnableOnSceneEnable = new List<Camera>();
    protected List<AudioListener> audioListenersToReEnableOnSceneEnable = new List<AudioListener>();
    protected List<Renderer> renderersToReEnableOnSceneEnable = new List<Renderer>();
    protected List<Canvas> canvasesToReEnableOnSceneEnable = new List<Canvas>();
    protected EventSystem eventSystemToReEnableOnSceneEnable = null;
    protected bool sceneShouldBeRunningOnSceneEnable = true;

    protected virtual void EnableScene()
    {

        foreach (Camera c in camerasToReEnableOnSceneEnable)
            if (c != null)
                c.enabled = true;

        camerasToReEnableOnSceneEnable.Clear();

        foreach (AudioListener al in audioListenersToReEnableOnSceneEnable)
            if (al != null)
                al.enabled = true;

        audioListenersToReEnableOnSceneEnable.Clear();

        foreach (Renderer r in renderersToReEnableOnSceneEnable)
            if (r != null)
                r.enabled = true;

        renderersToReEnableOnSceneEnable.Clear();

        foreach (Canvas c in canvasesToReEnableOnSceneEnable)
            if (c != null)
                c.enabled = true;

        canvasesToReEnableOnSceneEnable.Clear();

        if (eventSystemToReEnableOnSceneEnable != null)
        {
            eventSystemToReEnableOnSceneEnable.enabled = true;
            eventSystemToReEnableOnSceneEnable = null;
        }

    }

    protected virtual void DisableScene()
    {

        EnableScene();

        foreach (Camera camera in FindObjectsOfType<Camera>().Where(x => x.gameObject.scene == Scene))
        {
            if (camera.enabled)
            {
                camerasToReEnableOnSceneEnable.Add(camera);
                camera.enabled = false;
            }
        }

        foreach (AudioListener al in FindObjectsOfType<AudioListener>().Where(x => x.gameObject.scene == Scene))
        {
            if (al.enabled)
            {
                audioListenersToReEnableOnSceneEnable.Add(al);
                al.enabled = false;
            }
        }

        foreach (Renderer renderer in FindObjectsOfType<Renderer>().Where(x => x.gameObject.scene == Scene))
        {
            if (renderer.enabled)
            {
                renderersToReEnableOnSceneEnable.Add(renderer);
                renderer.enabled = false;
            }
        }

        foreach (Canvas canvas in FindObjectsOfType<Canvas>().Where(x => x.gameObject.scene == Scene))
        {
            if (canvas.enabled)
            {
                canvasesToReEnableOnSceneEnable.Add(canvas);
                canvas.enabled = false;
            }
        }

        EventSystem eventSystem = FindObjectsOfType<EventSystem>().Where(x => x.gameObject.scene == Scene).ToArray()[0];
        if (eventSystem != null && eventSystem.enabled)
        {
            eventSystemToReEnableOnSceneEnable = eventSystem;
            eventSystem.enabled = false;
        }

        //Just in case this object or component was disabled
        enabled = true;
        gameObject.SetActive(true);

    }

    protected void RefreshEnabledState()
    {
        if (sceneEnabled)
            EnableScene();
        else
            DisableScene();
    }

    public void SetEnabledState(bool state)
    {
        sceneEnabled = state;
        RefreshEnabledState();
    }

}
