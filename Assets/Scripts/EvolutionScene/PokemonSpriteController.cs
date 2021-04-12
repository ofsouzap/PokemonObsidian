using System.Collections;
using UnityEngine;

namespace EvolutionScene
{
    public class PokemonSpriteController : MonoBehaviour
    {

        public SpriteRenderer pokemonSpriteRenderer;
        public SpriteMask pokemonSpriteMask;
        public GameObject pokemonSpriteObject;

        private Vector3 initialScale;

        private void Start()
        {

            initialScale = pokemonSpriteObject.transform.localScale;

        }

        public void SetSprite(Sprite sprite)
        {
            pokemonSpriteRenderer.sprite = sprite;
            pokemonSpriteMask.sprite = sprite;
        }

        public void SetPosition(Vector3 position)
            => pokemonSpriteObject.transform.position = position;

        public void SetScale(float factor)
            => pokemonSpriteObject.transform.localScale = initialScale * factor;

        public void SetWhiteness(float factor)
            => pokemonSpriteRenderer.color = new Color(
                pokemonSpriteRenderer.color.r,
                pokemonSpriteRenderer.color.g,
                pokemonSpriteRenderer.color.b,
                1- factor);

    }
}