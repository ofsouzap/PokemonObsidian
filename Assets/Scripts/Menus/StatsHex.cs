using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace Menus
{
    [ExecuteInEditMode]
    public class StatsHex : MaskableGraphic
    {

        public float[] values = new float[] { 1, 1, 1, 1, 1, 1 };

        public static readonly Vector2[] directions = new Vector2[]
        {
            new Vector2(0f, 1f).normalized,
            new Vector2(3f, 1.732f).normalized,
            new Vector2(3f, -1.732f).normalized,
            new Vector2(0f, -1f).normalized,
            new Vector2(-3f, -1.732f).normalized,
            new Vector2(-3f, 1.732f).normalized
        };

        protected override void OnPopulateMesh(VertexHelper vh)
        {
            
            if (values.Length < 6)
            {
                Debug.LogError("Not enough values provided. 6 Values must be provided");
                return;
            }
            
            vh.Clear();

            UIVertex centerVert = UIVertex.simpleVert;
            centerVert.position = Vector2.zero;
            centerVert.color = color;
            vh.AddVert(centerVert);

            for (int i = 0; i < directions.Length; i++)
            {

                UIVertex vert = UIVertex.simpleVert;
                vert.position = new Vector2(
                    directions[i].x * values[i] * GetComponent<RectTransform>().rect.width / 2,
                    directions[i].y * values[i] * GetComponent<RectTransform>().rect.height / 2
                    );
                vert.color = color;
                vh.AddVert(vert);

            }

            for (int i = 0; i < directions.Length - 1; i++)
            {

                vh.AddTriangle(0, i + 1, i + 2);

            }

            vh.AddTriangle(0, directions.Length, 1);

        }

    }
}