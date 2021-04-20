using System.Collections.Generic;
using UnityEngine;

namespace FreeRoaming.WildPokemonArea
{
    [ExecuteInEditMode]
    [RequireComponent(typeof(WildPokemonAreaController))]
    public class WildGrassGenerator : MonoBehaviour
    {

        public Collider2D Collider => GetComponent<Collider2D>();
        public GameObject grassPrefab;

        private void Start()
        {

            if (grassPrefab == null)
            {
                Debug.LogError("Grass prefab not set");
            }
            else
            {
                Generate();
            }

        }

        private void ClearChildren()
        {

            foreach (Transform child in transform)
            {

                DestroyImmediate(child.gameObject);

            }

        }

        public void Generate()
        {

            ClearChildren();

            List<Vector3> grassPositions = new List<Vector3>();

            int minX, maxX, minY, maxY;

            Vector3 bottomLeft = Collider.bounds.center - Collider.bounds.extents;
            Vector3 topRight = Collider.bounds.center + Collider.bounds.extents;

            minX = Mathf.FloorToInt(bottomLeft.x);
            maxX = Mathf.CeilToInt(topRight.x);
            minY = Mathf.FloorToInt(bottomLeft.y);
            maxY = Mathf.CeilToInt(topRight.y);

            for (int posX = minX; posX <= maxX; posX++)
                for (int posY = minY; posY <= maxY; posY++)
                {

                    Vector3 currPos = new Vector3(posX, posY);
                    
                    if (Collider.bounds.Contains(currPos))
                    {
                        grassPositions.Add(currPos);
                    }

                }

            foreach (Vector3 pos in grassPositions)
                GenerateGameObject(pos);

        }

        private GameObject GenerateGameObject(Vector3 position)
        {

            GameObject go = Instantiate(grassPrefab, transform);
            go.transform.position = position;

            return go;

        }

    }
}