using UnityEngine;
using Newtonsoft.Json;
using System.Linq;

namespace Map
{
    public class MapManager : MonoBehaviour
    {
        public MapConfig mapConfig;
        public MapView mapView;

        public Map currentMap;

        void Start ()
        {
            //Kiểm tra xem có bản đồ đã lưu không
            if (PlayerPrefs.HasKey("Map"))
            {
                string mapJson = PlayerPrefs.GetString("Map");
                Map map = JsonConvert.DeserializeObject<Map>(mapJson);

                //Nếu đã xong phòng boss - tạo lại map mới
                if (map.path.Any(p => p.Equals(map.GetBossRoom().roomAddress)))
                {
                    GenerateNewMap();
                }
                else
                {
                    currentMap = map;
                    mapView.ShowMap(map);
                }    
            }
            else
            {
                GenerateNewMap();
            }    
        }

        public void GenerateNewMap()
        {
            Map map = MapGenerator.GetMap(mapConfig);
            currentMap = map;
            //Debug.Log(map.ToJson());
            mapView.ShowMap(map);
        }    

        public void SaveMap()
        {
            if (currentMap == null) return;

            string json = JsonConvert.SerializeObject(currentMap, Formatting.Indented,
                new JsonSerializerSettings { ReferenceLoopHandling = ReferenceLoopHandling.Ignore });
            PlayerPrefs.SetString("Map", json);
            PlayerPrefs.Save();
        }    
    }
}