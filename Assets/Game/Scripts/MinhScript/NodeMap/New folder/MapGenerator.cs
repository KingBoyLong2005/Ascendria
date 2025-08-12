using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEditor.Rendering;
using UnityEngine;
using UnityEngine.UIElements;
using static UnityEngine.Rendering.STP;

namespace Map
{
    public class MapGenerator
    {
        private static MapConfig mapConfig;
        private static List<float> floorDistances;
        private static readonly List<List<Room>> rooms = new List<List<Room>>();

        //Generate map với mapConfig từ scriptableObject (có thể gán vào button hoặc action khác tùy cách)
        public static Map GetMap(MapConfig config)
        {
            //Check null
            if (config == null)
            {
                Debug.Log("MapConfig was null in MapGenerator.GetMap()");
                return null;
            }

            //Lưu cấu hình được nhập mới và xóa dữ liệu rooms cũ nếu có
            mapConfig = config;
            rooms.Clear();

            //Sinh khoảng cách giữa các tầng
            GenerateFloorDistances();

            //Sinh các room trên từng tầng
            for (int i = 0; i < config.floors.Count; i++)
            {
                PlaceFloor(i);
            }

            //Tạo đường đi giữa các phòng
            List<List<Vector2Int>> paths = GeneratePaths();

            //Tạo khoảng cách phòng ngẫu nhiên
            RandomizeRoomPositions();

            //Thiết lập đường vào và đường ra của các phòng
            SetUpConnections(paths);
            
            //Xóa đường giao nhau giữa các phòng
            RemoveCrossConnections();

            //LinQ - lấy tất cả các phòng có ít nhất 1 đường và gộp thành 1 danh sách phòng
            List<Room> roomsList = rooms.SelectMany(room => room).Where(room => room.incoming.Count > 0 || room.outgoing.Count > 0).ToList();
            foreach (var room in roomsList)
            {
                string outgoingStr = string.Join(", ", room.outgoing.Select(v => $"({v.x},{v.y})"));
                string incomingStr = string.Join(", ", room.incoming.Select(v => $"({v.x},{v.y})"));

                //Debug.Log($"Room: ({room.roomAddress}) | Incoming: [{incomingStr}] | Outgoing: [{outgoingStr}]");
                room.Log();
            }
            
            //Lấy tên phòng boss ngẫu nhiên từ danh sách (hiện chỉ có 1 boss)
            string bossRoomName = mapConfig.roomNodes.Where(b => b.roomType == RoomType.Boss).ToList().Random().name;

            //Debug.Log($"Total distance: {GetDistanceToFloor(5)}");
            //Debug.Log("Rooms: " + string.Join(", ", roomsList.Select(r => $"{r.roomAddress} ({r.roomType})")));
            //Trả về 1 bản lưu bản đồ với tên cấu hình, tên boss, danh sách phòng, danh sách mới đường đi của player
            return new Map(mapConfig.name, bossRoomName, roomsList, new List<Vector2Int>());
        }

        //Trả về danh sách khoảng cách giữa các tầng
        private static void GenerateFloorDistances()
        {
            floorDistances = new List<float>();

            //Duyệt từng tầng trong danh sách cấu hình tầng của cấu hình map được sử dụng
            for (int i = 0; i < mapConfig.floors.Count; i++)
            {
                if (i == 0)
                {
                    floorDistances.Add(0f);
                }
                else 
                {
                    //GetValue trả về giá trị ngẫu nhiên giữa min và max của cấu hình tầng
                    floorDistances.Add(mapConfig.floors[i].distanceFromPreviousFloor.GetValue());
                }
            }    
        }

        //Trả về tổng khoảng cách từ tầng đầu (index = 0) đến tầng cần kiểm tra
        private static float GetDistanceToFloor(int floorIndex)
        {
            if (floorIndex < 0 || floorIndex > floorDistances.Count)
            {  return 0; }
            return floorDistances.Take(floorIndex + 1).Sum();
        }


        private static void PlaceFloor(int floorIndex)
        {
            //Lấy thông tin cấu hình của tầng
            FloorConfig floorConfig = mapConfig.floors[floorIndex];
            //Khởi tạo danh sách phòng
            List<Room> roomsOnThisFloor = new List<Room>();

            //Tính khoảng cách của phòng đầu tiên so với lề
            float offset = floorConfig.distanceBetweenRoomsOnFloor * mapConfig.gridWidth / 2f;

            //Lặp dựa trên số lượng phòng tối đa theo chiều ngang
            for (int i = 0; i < mapConfig.gridWidth; i++)
            {
                //Lấy danh sách phòng ngẫu nhiên từ cấu hình map
                var supportedRandomRoomTypes = mapConfig.randomRooms.Where(t => mapConfig.roomNodes.Any(b => b.roomType == t)).ToList();

                //Xác định kiểu phòng với tỷ lệ
                RoomType roomType = Random.Range(0f,1f) < floorConfig.randomizeRooms && supportedRandomRoomTypes.Count > 0 
                    ? supportedRandomRoomTypes.Random()
                    : floorConfig.roomType;

                //Chọn phòng ngẫu nhiên đúng với kiểu phòng
                string blueprintName = mapConfig.roomNodes.Where(b => b.roomType == roomType).ToList().Random().name;

                //Tạo phòng với vị trí trên bản đồ là phòng thứ i của tầng floorIndex
                Room room = new Room(roomType, blueprintName, new Vector2Int(i, floorIndex))
                {
                    //Vị trí phòng trên tọa độ màn hình
                    position = new Vector2(-offset + i * floorConfig.distanceBetweenRoomsOnFloor, GetDistanceToFloor(floorIndex))
                };

                //Thêm phòng vào danh sách phòng của tầng
                roomsOnThisFloor.Add(room);
            }

            //Thêm danh sách phòng của tầng vào danh sách phòng của bản đồ
            rooms.Add(roomsOnThisFloor);
            //Debug.Log($"Floor: {floorIndex}" + string.Join(", ", roomsOnThisFloor.Select(r => $"{r.roomAddress}")));
        }

        //Tạo khoảng cách phòng ngẫu nhiên
        private static void RandomizeRoomPositions()
        {
            for (int index = 0; index < rooms.Count; index++)
            {
                List<Room> list = rooms[index];
                FloorConfig floorConfig = mapConfig.floors[index];

                float distanceToNextFloor = index + 1 >= floorDistances.Count ? 0f : floorDistances[index + 1];
                float distToPreviousFloor = floorDistances[index];

                foreach (Room room in list)
                {
                    float xRnd = Random.Range(-0.4f, 0.4f);
                    float yRnd = Random.Range(-0.4f, 0.4f);

                    float x = xRnd * floorConfig.distanceBetweenRoomsOnFloor;
                    float y = yRnd < 0 ? distToPreviousFloor * yRnd : distanceToNextFloor* yRnd;

                    room.position += new Vector2(x, y) * floorConfig.randomizePosition;
                }
            }
        }

        
        private static Room GetRoom(Vector2Int roomAddress)
        {
            if (roomAddress.y >= rooms.Count) return null;
            if (roomAddress.x >= rooms[roomAddress.y].Count) return null;

            return rooms[roomAddress.y][roomAddress.x];
        }

        //Vị trí phòng cuối cùng trong bản đồ (phòng boss)
        private static Vector2Int GetFinalRoom()
        {
            int y = mapConfig.floors.Count() - 1;
            if (mapConfig.gridWidth % 2 == 1)
            {
                return new Vector2Int(mapConfig.gridWidth / 2, y);
            }

            return Random.Range(0, 2) == 0 
                ? new Vector2Int(mapConfig.gridWidth / 2, y) 
                : new Vector2Int(mapConfig.gridWidth / 2 - 1, y);
        }

        //Tạo các đường đi từ phòng bắt đầu đến phòng gần boss và nối thêm phòng boss
        private static List<List<Vector2Int>> GeneratePaths()
        {
            Vector2Int finalRoom = GetFinalRoom();
            //Debug.Log($"Đây là final Room tìm được: {finalRoom}");
            //Khởi tạo danh sách đường đi
            var paths = new List<List<Vector2Int>>();

            //Xác định số lượng phòng bắt đầu và phòng trước boss
            int numOfStartingRooms = mapConfig.numOfStartingRooms.GetValue();
            int numOfPreBossRooms = mapConfig.numOfPreBossRooms.GetValue();
             
            //Danh sách vị trí phòng của tầng
            List<int> roomPosXs = new List<int>();
            for (int i = 0; i < mapConfig.gridWidth; i++)
            {
                roomPosXs.Add(i);
            }

            //Trộn
            roomPosXs.Shuffle();
            //Lấy 3 vị trí đầu
            IEnumerable<int> startingXs = roomPosXs.Take(numOfStartingRooms);
            //Tạo danh sách vị trí room của tầng đầu tiên
            List<Vector2Int> startingRooms = (from x in startingXs select new Vector2Int(x, 0)).ToList();

            //Trộn
            roomPosXs.Shuffle();
            //Lấy 3 vị trí đầu
            IEnumerable<int> preBossXs = roomPosXs.Take(numOfPreBossRooms);
            //Tạo danh sách vị trí room của tầng đầu tiên trước boss
            List<Vector2Int> preBossRooms = (from x in preBossXs select new Vector2Int(x, finalRoom.y - 1)).ToList();

            //Tạo số lượng đường tối đa
            int numOfPaths = Mathf.Max(numOfStartingRooms, numOfPreBossRooms) + Mathf.Max(0, mapConfig.extraPaths);
            for (int i = 0; i < numOfPaths; ++i)
            {
                Vector2Int startRoom = startingRooms[i % numOfStartingRooms];
                Vector2Int endRoom = preBossRooms[i % numOfPreBossRooms];

                //Debug.Log($"startRoom: {startRoom} - endRoom: {endRoom}");

                //Tạo dường đi giữa startRoom và endRoom với Path
                List<Vector2Int> path = Path(startRoom, endRoom);
                //Thêm boss room vào cuối path
                path.Add(finalRoom);
                //Thêm đường đi vào danh sách
                paths.Add(path);
            }


            return paths;
        }

        private static List<Vector2Int> Path(Vector2Int fromRoom, Vector2Int toRoom)
        {
            //Vị trí phòng đích
            int toRow = toRoom.y;
            int toCol = toRoom.x;

            //Vị trí phòng bắt đầu đi
            int startRoomCol = fromRoom.x;

            //Khởi tạo danh sách đường đi với phòng đầu tiên
            List<Vector2Int> path = new List<Vector2Int> { fromRoom };
            //Danh sách các phòng có thể đi
            List<int> roomPosXs = new List<int>();

            //Bắt đầu từ tầng 1 tới tầng toRow - 1
            for (int row = 1; row < toRow; ++row)
            {
                roomPosXs.Clear();
                
                //Khoảng cách tới tầng cuối
                int verticalDistance = toRow - row;

                int horizontalDistance;

                //Middle - add vào list nếu có thể đi thẳng 1 tầng mà vẫn có khả năng tới toRoom
                int forwardCol = startRoomCol;
                horizontalDistance = Mathf.Abs(toCol - forwardCol);
                if (horizontalDistance <= verticalDistance)
                {
                    roomPosXs.Add(startRoomCol);
                }

                //Left - add vào list nếu có thể rẽ trái 1 tầng (không ra ngoài grid) mà vẫn có khả năng tới toRoom
                int leftCol = startRoomCol - 1;
                horizontalDistance = Mathf.Abs(toCol - leftCol);
                if (leftCol >= 0 && horizontalDistance <= verticalDistance)
                    roomPosXs.Add(leftCol);

                //Right - add vào list nếu có thể rẽ phải 1 tầng (không ra ngoài grid) mà vẫn có khả năng tới toRoom
                int rightCol = startRoomCol + 1;
                horizontalDistance = Mathf.Abs(toCol - rightCol);
                if (rightCol < mapConfig.gridWidth && horizontalDistance <= verticalDistance)
                    roomPosXs.Add(rightCol);
                
                //Chọn ngẫu nhiên 1 room từ danh sách room có thể đi
                int randomRoomPosXIndex = Random.Range(0, roomPosXs.Count);
                int roomPosX = roomPosXs[randomRoomPosXIndex];
                Vector2Int nextRoom = new Vector2Int(roomPosX, row);
                
                //Thêm room vào danh sách đường đi
                path.Add(nextRoom);

                //Cập nhật lại vị trí room bắt đầu đi của loop
                startRoomCol = roomPosX;
            }

            //Thêm room đích vào danh sách
            path.Add(toRoom);

            //Debug.Log("Path: " + string.Join(", ", path));

            return path;
        }

        //Thêm incoming với outgoing cho các room trong danh sách đường đi
        private static void SetUpConnections(List<List<Vector2Int>> paths)
        {
            foreach (List<Vector2Int> path in paths)
            {
                for (int i = 0; i < path.Count - 1; ++i)
                {
                    Room room = GetRoom(path[i]);
                    Room nextRoom = GetRoom(path[i + 1]);
                    room.AddOutgoing(nextRoom.roomAddress);
                    nextRoom.AddIncoming(room.roomAddress);
                }
            }
        }

        //Loại bỏ các đường đi cắt nhau
        private static void RemoveCrossConnections()
        {
            for (int i = 0; i < mapConfig.gridWidth - 1; ++i)
            {
                for(int j = 0; j < mapConfig.floors.Count - 1; ++j)
                {
                    //Nếu không tồn tạo room hoặc room không có kết nối nào thì bỏ qua vòng lặp
                    Room room = GetRoom(new Vector2Int(i, j));
                    if (room == null || room.HasNoConnections()) continue;
                    Room right = GetRoom(new Vector2Int(i + 1, j));
                    if (right == null || right.HasNoConnections()) continue;
                    Room top = GetRoom(new Vector2Int(i, j + 1));
                    if (top == null || top.HasNoConnections()) continue;
                    Room topRight = GetRoom(new Vector2Int(i + 1, j + 1));
                    if (topRight == null || topRight.HasNoConnections()) continue;

                    //Nếu 2 phòng được duyệt bên dưới không đồng thời nối chéo lên 2 phòng được duyệt bên trên thì bỏ qua
                    if (!room.outgoing.Any(element => element.Equals(topRight.roomAddress))) continue;
                    if (!right.outgoing.Any(element => element.Equals(top.roomAddress))) continue;

                    //Nếu có kết nối chéo
                    //Tạo kết nối dọc
                    room.AddOutgoing(top.roomAddress);
                    top.AddIncoming(room.roomAddress);

                    right.AddOutgoing(topRight.roomAddress);
                    topRight.AddIncoming(right.roomAddress);

                    //Loại bỏ kết nối chéo theo tỷ lệ ngẫu nhiên
                    float rnd = Random.Range(0f, 1f);
                    if (rnd < 0.2f) //Xóa cả 2 đường chéo
                    {
                        room.RemoveOutgoing(topRight.roomAddress);
                        topRight.RemoveIncoming(room.roomAddress);
                        right.RemoveOutgoing(top.roomAddress);
                        top.RemoveIncoming(right.roomAddress);
                    }
                    else if (rnd < 0.6f) //Xóa 1 đường chéo từ phòng dưới trái đến trên phải
                    {
                        room.RemoveOutgoing(topRight.roomAddress);
                        topRight.RemoveIncoming(room.roomAddress);
                    }
                    else //Xóa 1 đường chéo từ phòng dưới phải đến trên trái
                    {
                        right.RemoveOutgoing(top.roomAddress);
                        top.RemoveIncoming(right.roomAddress);
                    }
                }  
            }    
        }
    }
}
