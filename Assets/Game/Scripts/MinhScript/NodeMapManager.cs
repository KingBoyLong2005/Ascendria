//using NUnit.Framework;
//using System.Collections.Generic;
//using TMPro;
//using UnityEngine;
//using UnityEngine.UI;

//public enum RoomType
//{
//    Rest,
//    Shop,
//    Mob,
//    Event,
//    Boss
//}

//public class RoomNode
//{
//    public int floor;
//    public RoomType type;
//    public Button roomButton;
//    public List<RoomNode> connectedNextRoom = new List<RoomNode>();
//    public bool isReachable = false;
//}

//public class NodeMapManager : MonoBehaviour
//{
//    [SerializeField] private Button mapGenerate;
//    [SerializeField] private Button mapChoose;
//    [SerializeField] private GameObject roomPrefab;
//    [SerializeField] private Transform mapContainer;

//    private List<List<RoomNode>> floors = new List<List<RoomNode>>();
//    private RoomNode selectedRoom;

//    private void Start()
//    {
//        mapGenerate.onClick.AddListener(OnMapGenerate);
//        mapChoose.onClick.AddListener(OnMapChoose);
//        mapChoose.gameObject.SetActive(false);
//    }

//    public void OnMapGenerate()
//    {
//        foreach (Transform child in mapContainer)
//        {
//            Destroy(child.gameObject);
//        } 

//        floors.Clear();

//        for (int i = 0; i <15; i++)
//        {
//            int roomCount = (i == 0 || i == 14) ? 1 : Random.Range(1,4);
//            List<RoomNode> floor = new List<RoomNode>();

//            for (int j = 0; j < roomCount; j++)
//            {
//                GameObject roomGO = Instantiate(roomPrefab, mapContainer);
//                Button roomBtn = roomGO.GetComponent<Button>();

//                RoomNode node = new RoomNode
//                {
//                    floor = i,
//                    type = (i == 0) ? RoomType.Rest :
//                       (i == 14) ? RoomType.Boss :
//                       (RoomType)Random.Range(0, 4),
//                    roomButton = roomBtn
//                };

//                TMP_Text label = roomGO.GetComponentInChildren<TMP_Text>();
//                if (label != null)
//                    label.text = node.type.ToString();
//                else
//                    Debug.LogError("Không tìm thấy TMP_Text trong roomPrefab");

//                RoomNode capturedNode = node;
//                roomBtn.onClick.AddListener(() => OnRoomClicked(capturedNode));
//                Debug.Log($"Listener added for room: {node.type} at floor {i}, index {j}");

//                // Cập nhật vị trí UI (layout theo chiều dọc và ngang tuỳ vào tầng và số phòng)
//                roomGO.GetComponent<RectTransform>().anchoredPosition = new Vector2(j * 120, i * 150);

//                floor.Add(node);
//            }

//            floors.Add(floor);
//        }

//        // Nối các node giữa các tầng
//        for (int i = 0; i < floors.Count - 1; i++)
//        {
//            foreach (var room in floors[i])
//            {
//                // Đảm bảo mỗi phòng nối tới ít nhất một phòng tầng tiếp theo
//                int connectCount = Random.Range(1, floors[i + 1].Count + 1);
//                List<RoomNode> nextRooms = new List<RoomNode>(floors[i + 1]);
//                for (int j = 0; j < connectCount; j++)
//                {
//                    int randIndex = Random.Range(0, nextRooms.Count);
//                    room.connectedNextRoom.Add(nextRooms[randIndex]);
//                    nextRooms.RemoveAt(randIndex);
//                }
//            }
//        }

//        // Đánh dấu node đầu tiên là có thể đến được
//        floors[0][0].isReachable = true;
//    }

//    private void OnRoomClicked(RoomNode node)
//    {
//        // Chỉ cho chọn nếu node có thể đến được từ phòng trước
//        if (!node.isReachable) return;

//        selectedRoom = node;
//        mapChoose.gameObject.SetActive(true);
//    }

//    public void OnMapChoose()
//    {
//        // Khi chọn phòng, đánh dấu các phòng tầng tiếp theo là có thể đến từ phòng này
//        foreach (var next in selectedRoom.connectedNextRoom)
//        {
//            next.isReachable = true;
//            // Có thể thay đổi màu phòng để đánh dấu khả năng đi được
//            next.roomButton.image.color = Color.green;
//        }

//        selectedRoom.roomButton.image.color = Color.cyan; // Đánh dấu phòng đã chọn
//        mapChoose.gameObject.SetActive(false);
//    }
//}
