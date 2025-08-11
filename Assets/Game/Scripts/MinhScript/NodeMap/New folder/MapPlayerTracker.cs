using DG.Tweening;
using System;
using System.Linq;
using UnityEngine;


namespace Map
{
    public class MapPlayerTracker : MonoBehaviour
    {
        public bool lockAfterSelecting = false;
        public float enterNodeDelay = 1f;
        public MapManager mapManager;
        public MapView view;

        public static MapPlayerTracker Instance;

        public bool Locked { get; set; }

        private void Awake()
        {
            Instance = this;
        }

        public void SelectNode(RoomView roomView)
        {
            if (Locked) return;

            // Debug.Log("Selected node: " + mapNode.Node.point);

            if (mapManager.currentMap.path.Count == 0)
            {
                // player has not selected the node yet, he can select any of the nodes with y = 0
                if (roomView.room.roomAddress.y == 0)
                    SendPlayerToNode(roomView);
                else
                    PlayWarningThatNodeCannotBeAccessed();
            }
            else
            {
                Vector2Int currentPoint = mapManager.currentMap.path[mapManager.currentMap.path.Count - 1];
                Room currentRoom = mapManager.currentMap.GetRoom(currentPoint);

                if (currentRoom != null && currentRoom.outgoing.Any(point => point.Equals(roomView.room.roomAddress)))
                    SendPlayerToNode(roomView);
                else
                    PlayWarningThatNodeCannotBeAccessed();
            }
        }

        private void SendPlayerToNode(RoomView roomView)
        {
            Locked = lockAfterSelecting;
            mapManager.currentMap.path.Add(roomView.room.roomAddress);
            mapManager.SaveMap();
            view.SetAttainableRooms();
            view.SetLineColors();
            roomView.ShowSwirlAnimation();

            DOTween.Sequence().AppendInterval(enterNodeDelay).OnComplete(() => EnterNode(roomView));
        }

        private static void EnterNode(RoomView roomView)
        {
            // we have access to blueprint name here as well
            Debug.Log("Entering node: " + roomView.room.blueprintName + " of type: " + roomView.room.roomType);
            // load appropriate scene with context based on nodeType:
            // or show appropriate GUI over the map: 
            // if you choose to show GUI in some of these cases, do not forget to set "Locked" in MapPlayerTracker back to false
            switch (roomView.room.roomType)
            {
                case RoomType.NormalMonster:
                    break;
                case RoomType.EliteMonster:
                    break;
                case RoomType.RestSite:
                    break;
                case RoomType.Shop:
                    break;
                case RoomType.Event:
                    break;
                case RoomType.Boss:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void PlayWarningThatNodeCannotBeAccessed()
        {
            Debug.Log("Selected node cannot be accessed");
        }
    }
}