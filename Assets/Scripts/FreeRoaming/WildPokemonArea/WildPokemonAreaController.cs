using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using Pokemon;

namespace FreeRoaming.WildPokemonArea
{
    [RequireComponent(typeof(Collider2D))]
    public class WildPokemonAreaController : MonoBehaviour
    {

        public Scene Scene => gameObject.scene;

        public static WildPokemonAreaController[] GetSceneWildPokemonAreas(Scene scene)
        {

            return FindObjectsOfType<WildPokemonAreaController>()
                .Where(x => x.gameObject.scene == scene)
                .ToArray();

        }

        public static WildPokemonAreaController GetPositionWildPokemonArea(Vector2 pos, Scene scene)
        {

            IEnumerable<WildPokemonAreaController> matches = GetSceneWildPokemonAreas(scene).Where(x => x.ContainsPosition(pos));

            if (matches.Count() <= 0)
                return null;
            else
                return matches.First();

        }

        [SerializeField]
        private string battleBackgroundResourceName;
        public string GetBattleBackgroundResourceName() => battleBackgroundResourceName;

        [SerializeField]
        private int wildPokemonAreaId;

        public WildPokemonAreaData.WildPokemonAreaSpecification AreaSpec
            => WildPokemonAreaData.GetAreaSpecificationById(wildPokemonAreaId);

        public PokemonInstance.WildSpecification GetPokemonWildSpecification()
            => WildPokemonAreaData.GetPokemonSpecificationById(wildPokemonAreaId);

        public PokemonInstance GenerateWildPokemon()
            => GetPokemonWildSpecification().Generate();

        public float GetEncounterChance()
            => AreaSpec.encounterChance;

        public bool RunEncounterCheck(float chanceMultiplier = 1)
            => Random.Range(0F, 1F) <= GetEncounterChance() * chanceMultiplier;

        public bool ContainsPosition(Vector2 pos)
            => GetComponent<Collider2D>().bounds.Contains(pos);

    }
}
