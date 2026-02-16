using UnityEngine;

namespace bullet.fx.pack {
    [RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
    public sealed class Cylinder : MonoBehaviour
    {
        private void Start() {
            var customMesh = new Mesh();
            customMesh.name = "Fire3DCylinder";

            customMesh.vertices = SetVertices();

            customMesh.triangles = SetTriangles();

            customMesh.uv = SetUVs(customMesh.vertices);

            customMesh.RecalculateNormals();
            GetComponent<MeshFilter>().mesh = customMesh;
        }

        private Vector3[] SetVertices() {

            var coef = 0.3f;
            var coef2 = 0.2f;

            var radiusMedium = Mathf.Cos(Mathf.PI / 4f);
            var radiusMediumHalf = radiusMedium * 0.4f;

            return new Vector3[]{

                new Vector3(-0.4f * coef2, -2.5f, 0),
                new Vector3(-radiusMediumHalf * coef2, -2.5f, radiusMediumHalf * coef2),
                new Vector3(0, -2.5f, 0.4f * coef2),
                new Vector3(radiusMediumHalf * coef2, -2.5f, radiusMediumHalf * coef2),
                new Vector3(0.4f * coef2, -2.5f, 0),
                new Vector3(radiusMediumHalf * coef2, -2.5f, -radiusMediumHalf * coef2),
                new Vector3(0, -2.5f, -0.4f * coef2),
                new Vector3(-radiusMediumHalf * coef2, -2.5f, -radiusMediumHalf * coef2),

                new Vector3(-0.4f * coef, -0.5f, 0),
                new Vector3(-radiusMediumHalf * coef, -0.5f, radiusMediumHalf * coef),
                new Vector3(0, -0.5f, 0.4f * coef),
                new Vector3(radiusMediumHalf * coef, -0.5f, radiusMediumHalf * coef),
                new Vector3(0.4f * coef, -0.5f, 0),
                new Vector3(radiusMediumHalf * coef, -0.5f, -radiusMediumHalf * coef),
                new Vector3(0, -0.5f, -0.4f * coef),
                new Vector3(-radiusMediumHalf * coef, -0.5f, -radiusMediumHalf * coef),
            };
        }

        private int[] SetTriangles() {
            return new int[]{
                8, 0, 1,
                8, 1, 9,
                9, 1, 2,
                9, 2, 10,
                10, 2, 3,
                10, 3, 11,
                11, 3, 4,
                11, 4, 12,
                12, 4, 5,
                12, 5, 13,
                13, 5, 6,
                13, 6, 14,
                14, 6, 7,
                14, 7, 15,
                15, 7, 8,
                8, 7, 0,
            };
        }

        private Vector2[] SetUVs(Vector3[] vertices) {
            Vector2[] uvs = new Vector2[vertices.Length];
            float height = 2.0f;

            for (int i = 0; i < vertices.Length; i++) {
                Vector3 vertex = vertices[i];

                if (vertex.y > 0) {
                    float u = Mathf.Atan2(vertex.z, vertex.x) / (2 * Mathf.PI);
                    float v = (vertex.y + height * 0.5f) / height;

                    uvs[i] = new Vector2(u, v);
                } else {
                    float u = Mathf.Atan2(vertex.z, vertex.x) / (2 * Mathf.PI);
                    float v = (vertex.y + height * 0.5f) / height;

                    uvs[i] = new Vector2(u, v);
                }
            }
            return uvs;
        }
    }
}