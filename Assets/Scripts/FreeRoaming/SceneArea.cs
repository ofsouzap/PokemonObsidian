using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Audio;

namespace FreeRoaming
{
    public struct SceneArea : IHasId
    {

        #region Registry

        private const string dataFileResourcesPath = "Data/sceneAreas";

        public static Registry<SceneArea> registry = new Registry<SceneArea>();

        private static bool registryLoaded = false;

        public static void TryLoadRegistry()
        {
            if (!registryLoaded)
                LoadRegistry();
        }

        private static void LoadRegistry()
        {

            List<SceneArea> areas = new List<SceneArea>();

            string[][] entries = CSV.ReadCSVResource(dataFileResourcesPath, true);

            foreach (string[] entry in entries)
            {

                int id, weatherId;
                string name, musicName;
                AreaNameSignController.SignBackground background;

                id = int.Parse(entry[0]);

                if (id == 0)
                {
                    Debug.LogError("Id of scene area shouldn't be 0");
                    return;
                }

                name = entry[1];
                musicName = entry[2];

                if (!AreaNameSignController.TryParseSignBackground(entry[3], out background))
                {
                    Debug.LogError("Unable to parse sign background - " + entry[3]);
                    return;
                }

                string weatherIdEntry = entry[4];

                if (string.IsNullOrEmpty(weatherIdEntry))
                {
                    weatherId = Weather.defaultWeatherId;
                }
                else
                {
                    if (!int.TryParse(entry[4], out weatherId))
                    {
                        Debug.LogError("Invalid weather entry for scene area id " + id);
                        weatherId = Weather.defaultWeatherId;
                    }
                }

                areas.Add(new SceneArea()
                {
                    id = id,
                    name = name,
                    musicName = musicName,
                    signBackground = background,
                    weatherId = weatherId
                });

            }

            registry.SetValues(areas.ToArray());

            registryLoaded = true;

        }

        #endregion

        public int id;
        public int GetId() => id;

        public string name;
        public string musicName;
        public AreaNameSignController.SignBackground signBackground;
        public int weatherId;

        public void TryPlayAreaMusic()
        {

            if (musicName != null && musicName != "")
                MusicSourceController.singleton.SetTrack(musicName,
                        fadeTracks: true);

        }

        public void TryDisplayAreaNameSign(Scene scene)
        {

            if (name != null && name != "")
                AreaNameSignController.GetAreaNameSignController(scene).DisplayAreaName(name, signBackground);

        }

        public void TrySetWeatherCanvasWeather(Scene scene)
        {

            if (name != null && name != "")
            {
                WeatherCanvasController weatherCanvas = WeatherCanvasController.GetWeatherCanvasController(scene);
                if (weatherCanvas != null)
                    weatherCanvas.SetDisplayedWeather(Weather.GetWeatherById(weatherId));
            }

        }

        public Weather GetWeather()
        {

            Weather w = Weather.GetWeatherById(weatherId);

            if (w != null)
                return w;
            else
            {
                Debug.LogError("Weather id for scene area doesn't exist.");
                return Weather.GetWeatherById(Weather.defaultWeatherId);
            }

        }

    }
}
