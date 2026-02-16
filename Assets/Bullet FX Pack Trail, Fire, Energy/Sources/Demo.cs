using UnityEngine;

namespace bullet.fx.pack {
    public class Demo : MonoBehaviour
    {
        public Gun gun;

        void Start()
        {
            GameObject lightObj = new GameObject("Red Light");
            Light light = lightObj.AddComponent<Light>();
            light.type = LightType.Point;
            light.color = new Color32(164, 164, 164, 255);
            light.range = 10;
            light.intensity = 3;
            lightObj.transform.position = new Vector3(0, 6, 0);
        }

        private void Update() {
            if (Input.GetMouseButtonDown(0)) {
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                    if (Physics.Raycast(ray, out RaycastHit hit)) {
                        gun.Shoot(hit.point);
                    } else {
                        var forw = new Vector3(transform.forward.x, transform.forward.y + 2f, transform.forward.z);
                        gun.Shoot(forw);
                    }
            }
        }
    }
}