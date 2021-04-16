using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Audio
{
    public static class AudioStorage
    {

        private static Dictionary<string, AudioClip> musicClips;
        private static Dictionary<string, AudioClip> fxClips;

        #region Loading

        private static bool audioLoaded = false;

        public static void TryLoadAll()
        {
            if (!audioLoaded)
                LoadAll();
        }

        private const string musicResourcesDirectory = "Audio/Music";
        private const string fxResourcesDirectory = "Audio/FX";

        private static void LoadAll()
        {

            musicClips = new Dictionary<string, AudioClip>();

            foreach (AudioClip clip in Resources.LoadAll<AudioClip>(musicResourcesDirectory))
                if (musicClips.ContainsKey(clip.name))
                    Debug.LogError("Duplicate music clip name - " + clip.name);
                else
                    musicClips.Add(clip.name, clip);

            fxClips = new Dictionary<string, AudioClip>();

            foreach (AudioClip clip in Resources.LoadAll<AudioClip>(fxResourcesDirectory))
                if (fxClips.ContainsKey(clip.name))
                    Debug.LogError("Duplicate fx clip name - " + clip.name);
                else
                    fxClips.Add(clip.name, clip);

            audioLoaded = true;

        }

        #endregion

        #region Clip Getting

        public const string musicStartingClipSuffix = "_start";

        public static AudioClip GetMusicClip(string name,
            bool getStartingClip = false)
        {

            string trueName = getStartingClip ? name + musicStartingClipSuffix : name;

            if (musicClips.ContainsKey(trueName))
                return musicClips[trueName];
            else
                return null;

        }

        public static AudioClip GetFXClip(string name)
        {
            if (fxClips.ContainsKey(name))
                return fxClips[name];
            else
                return null;
        }

        #endregion

    }
}
