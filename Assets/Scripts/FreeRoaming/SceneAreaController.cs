using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Audio;

namespace FreeRoaming
{
    /// <summary>
    /// A controller for areas of scenes e.g. towns, routes
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    public class SceneAreaController : MonoBehaviour
    {

        private SceneArea? area;

        public SceneArea? GetArea() => area;

        public static SceneAreaController[] GetSceneWildPokemonAreas(Scene scene)
        {

            return FindObjectsOfType<SceneAreaController>()
                .Where(x => x.gameObject.scene == scene)
                .ToArray();

        }

        public static SceneAreaController GetPositionSceneArea(Vector2 pos, Scene scene)
        {

            IEnumerable<SceneAreaController> matches = GetSceneWildPokemonAreas(scene).Where(x => x.ContainsPosition(pos));

            if (matches.Count() <= 0)
                return null;
            else
                return matches.First();

        }

        public int id = 0;

        private void Start()
        {

            if (id != 0)
                area = SceneArea.registry.LinearSearch(id);
            else
                area = null;

        }

        public void OnPlayerEnterArea()
        {

            area?.TryPlayAreaMusic();

            area?.TryDisplayAreaNameSign(gameObject.scene);

            area?.TrySetWeatherCanvasWeather(gameObject.scene);

        }

        public bool ContainsPosition(Vector2 pos)
            => GetComponent<Collider2D>().bounds.Contains(pos);

    }
}
