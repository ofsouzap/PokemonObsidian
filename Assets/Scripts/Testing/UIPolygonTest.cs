using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

#if UNITY_EDITOR

namespace Testing
{
    [ExecuteInEditMode]
    public class UIPolygonTest : MaskableGraphic
    {

        public float multiplier = 10;

        protected override void Start()
        {
            SetAllDirty();
        }

        private static readonly Vector2[] directions = new Vector2[]
        {
            new Vector2(0,1).normalized,
            new Vector2(1,0).normalized,
            new Vector2(0,-1).normalized,
            new Vector2(-1,0).normalized
        };

        private static readonly byte[][] tris = new byte[][]
        {
            new byte[] { 0, 1, 2 },
            new byte[] { 2, 3, 0 }
        };

        private static readonly float[] values = new float[]
        {
            1,5,3,2,7,3,6
        };

        protected override void OnPopulateMesh(VertexHelper vh)
        {
            
            vh.Clear();

            for (int i = 0; i < directions.Length; i++)
            {
                UIVertex v = UIVertex.simpleVert;
                v.position = directions[i] * values[i] * multiplier;
                vh.AddVert(v);
            }

            foreach (byte[] t in tris)
            {
                vh.AddTriangle(t[0], t[1], t[2]);
            }

        }

    }
}

#endif
