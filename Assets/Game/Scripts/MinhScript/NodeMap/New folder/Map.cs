using Newtonsoft.Json;
using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Map
{
    public class Map
    {
        public List<Room> rooms;
        public List<Vector2Int> path;

        public string bossRoomName;
        public string configName;

        public Map(string configName, string bossName, List<Room> rooms, List<Vector2Int> path)
        {
            this.configName = configName;
            this.bossRoomName = bossName;
            this.rooms = rooms;
            this.path = path;
        }

        public Room GetBossRoom()
        {
            return rooms.FirstOrDefault(room => room.roomType == RoomType.Boss);
        }

        public float DistanceBetweenFirstAndLastFloors()
        {
            Room bossRoom = GetBossRoom();
            Room firstFloorRoom = rooms.FirstOrDefault(room => room.roomAddress.y == 0);
            
            if(bossRoom == null || firstFloorRoom == null)
            {
                return 0f;
            }

            return bossRoom.roomAddress.y - firstFloorRoom.roomAddress.y;
        }

        public Room GetRoom(Vector2Int roomAddress)
        {
            return rooms.FirstOrDefault(room => room.roomAddress.Equals(roomAddress));
        }

        public string ToJson()
        {
            return JsonConvert.SerializeObject(this, Formatting.Indented, new JsonSerializerSettings
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore
            });
        }
    }
}
