using System;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UI.Extensions;
using UnityEngine.UIElements;

namespace Map
{
    [System.Serializable]
    public class WorldSpaceSettings
    {
        public Camera cam;

        [Tooltip("Room prefab for 2D/3D view ")]
        public GameObject roomPrefab;

        [Header("Line Settings")]
        public GameObject linePrefab;
        [Tooltip("Line point count should be > 2 to get smooth color gradients")]
        [Range(3, 10)]
        public int linePointsCount = 10;
        [Tooltip("Distance from the node till the line starting point")]
        public float offsetFromNodes = 0.5f;

        [Header("Background Settings")]
        public float xSize;
        public float yOffset;
        [Tooltip("Offset of the start/end rooms of the map from the edges of the screen")]
        public float orientationOffset;
    }

    [System.Serializable]
    public class CanvasUISettings
    {
        [Tooltip("Room prefab for UI view ")]
        public GameObject roomPrefab;

        [Tooltip("ScrollRect that will be used for orientations: Left To Right, Right To Left")]
        [SerializeField] public ScrollRect scrollRectHorizontal;
        [Tooltip("ScrollRect that will be used for orientations: Top To Bottom, Bottom To Top")]
        [SerializeField] public ScrollRect scrollRectVertical;
        [Tooltip("Multiplier to compensate for larger distances in UI pixels on the canvas compared to distances in world units")]
        [SerializeField] public float unitsToPixelsMultiplier = 50f;
        [Tooltip("Padding of the first and last rows of nodes from the sides of the scroll rect")]
        [SerializeField] public float padding = 300;
        [Tooltip("Padding of the background from the sides of the scroll rect")]
        [SerializeField] public Vector2 backgroundPadding = new Vector2(-100,-100);
        [Tooltip("Pixels per Unit multiplier for the background image")]
        [SerializeField] public float backgroundPPUMultiplier = 2f;
        [Tooltip("Distance from the room till the line starting point")]
        [SerializeField] public float offsetFromRooms = 40f;
    }

    //For 2D/3D view
    public class MapView : MonoBehaviour
    {
        public enum MapOrientation
        {
            BottomToTop,
            TopToBottom,
            RightToLeft,
            LeftToRight
        }

        public bool CanvasUI;   // Checkbox Canvas UI
        public bool WorldSpace; // Checkbox World Space

        public CanvasUISettings canvasUISettings;
        public WorldSpaceSettings worldSpaceSettings;

        public MapManager mapManager;
        public MapOrientation orientation;

        [Tooltip("List of all the MapConfig scriptable objects from the Assets folder that might be used to construct maps.")]
        public List<MapConfig> allMapConfigs;

        [Header("Background Settings")]
        [Tooltip("If the background sprite is null, background will not be shown")]
        public Sprite background;
        public Color32 backgroundColor = Color.white;

        [Header("Colors")]
        [Tooltip("Visited or Attainable room color")]
        public Color32 visitedColor = Color.white;
        [Tooltip("Locked room color")]
        public Color32 lockedColor = Color.gray;
        [Tooltip("Visited or available path color")]
        public Color32 lineVisitedColor = Color.white;
        [Tooltip("Unavailable path color")]
        public Color32 lineLockedColor = Color.gray;

        private GameObject firstParent;
        private GameObject mapParent;
        private List<List<Vector2Int>> paths;

        // ALL nodes:
        public readonly List<RoomView> roomViews = new List<RoomView>();
        private readonly List<LineConnection> lineConnections = new List<LineConnection>();

        public static MapView Instance;

        public Map Map { get; private set; }

        //
        private void Awake()
        {
            Instance = this;

            if (WorldSpace)
            {
                //worldSpaceSettings.cam = Camera.main;
            }
        }

        public void ShowMap(Map m)
        {
            if (m == null)
            {
                Debug.LogWarning("Map was null in MapView.ShowMap()");
                return;
            }

            Map = m;

            ClearMap();

            CreateMapParent();

            CreateRooms(m.rooms);

            DrawLines();

            SetOrientation();

            ResetNodesRotation();

            SetAttainableRooms();

            SetLineColors();

            CreateMapBackground(m);
        }

        //Xóa dữ liệu map cũ nếu có
        protected void ClearMap()
        {
            if (WorldSpace)
            {
                if (firstParent != null)
                    Destroy(firstParent);

                roomViews.Clear();
                //lineConnections.Clear();
            } 
                
            if(CanvasUI)
            {
                canvasUISettings.scrollRectHorizontal.gameObject.SetActive(false);
                canvasUISettings.scrollRectVertical.gameObject.SetActive(false);

                foreach (ScrollRect scrollRect in new[] { canvasUISettings.scrollRectHorizontal, canvasUISettings.scrollRectVertical })
                    foreach (Transform t in scrollRect.content)
                        Destroy(t.gameObject);

                roomViews.Clear();
                //lineConnections.Clear();
            }    
        }

        // Creates and configures parent GameObjects for the map, setting up hierarchy and components.
        // For WorldSpace: Sets up mapParent with ScrollNonUI and BoxCollider, freezing X or Y based on map orientation.
        // For CanvasUI: Configures firstParent and mapParent with RectTransforms, attaches them to the appropriate ScrollRect, stretches them to fit, and sets map length and scroll position.
        protected virtual void CreateMapParent()
        {
            //Create object
            firstParent = new GameObject("OuterMapParent");
            mapParent = new GameObject("MapParentWithAScroll");

            if (WorldSpace)
            {
                //mapParent.transform.SetParent(firstParent.transform);
                //ScrollNonUI scrollNonUi = mapParent.AddComponent<ScrollNonUI>();
                //scrollNonUi.freezeX = orientation == MapOrientation.BottomToTop || orientation == MapOrientation.TopToBottom;
                //scrollNonUi.freezeY = orientation == MapOrientation.LeftToRight || orientation == MapOrientation.RightToLeft;
                //BoxCollider boxCollider = mapParent.AddComponent<BoxCollider>();
                //boxCollider.size = new Vector3(100, 100, 1);
            }    
                
            if (CanvasUI)
            {
                //Get scroll rect
                ScrollRect scrollRect = GetScrollRectForMap();
                scrollRect.gameObject.SetActive(true);

                //Đặt firstParent làm con của scrollRect.content
                firstParent.transform.SetParent(scrollRect.content);
                firstParent.transform.localScale = Vector3.one;
                RectTransform fprt = firstParent.AddComponent<RectTransform>();
                Stretch(fprt);

                //Đặt firstParent làm con của firstParent
                mapParent.transform.SetParent(firstParent.transform);
                mapParent.transform.localScale = Vector3.one;
                RectTransform mprt = mapParent.AddComponent<RectTransform>();
                Stretch(mprt);

                SetMapLength();
                ScrollToOrigin();
            }
        }

        // Returns the appropriate ScrollRect based on the map's orientation.
        // If the map orientation is LeftToRight or RightToLeft, returns the horizontal ScrollRect.
        // Otherwise, returns the vertical ScrollRect.
        private ScrollRect GetScrollRectForMap()
        {
            return orientation == MapOrientation.LeftToRight || orientation == MapOrientation.RightToLeft
                ? canvasUISettings.scrollRectHorizontal
                : canvasUISettings.scrollRectVertical;
        }

        // Stretches a RectTransform to fully occupy its parent's space by setting anchors to cover the entire parent area,
        // resetting local position, size delta, and anchored position to zero for precise alignment.
        private static void Stretch(RectTransform rt)
        {
            rt.localPosition = Vector3.zero;
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.sizeDelta = Vector2.zero;
            rt.anchoredPosition = Vector2.zero;
        }

        // Sets the size of the ScrollRect's content based on the map's orientation and length.
        // Calculates the length using the distance between the first and last floors, a pixel multiplier, and padding.
        // Updates the content's width (for LeftToRight/RightToLeft) or height (for TopToBottom/BottomToTop) accordingly.
        private void SetMapLength()
        {
            //Lấy RectTransform của content scroll
            RectTransform rt = GetScrollRectForMap().content;
            Vector2 sizeDelta = rt.sizeDelta;
            float length = canvasUISettings.padding + Map.DistanceBetweenFirstAndLastFloors() * canvasUISettings.unitsToPixelsMultiplier;
            if (orientation == MapOrientation.LeftToRight || orientation == MapOrientation.RightToLeft)
                sizeDelta.x = length;
            else
                sizeDelta.y = length;
            rt.sizeDelta = sizeDelta;
        }

        // Scrolls the ScrollRect to the origin of the map based on its orientation.
        private void ScrollToOrigin()
        {
            switch (orientation)
            {
                case MapOrientation.BottomToTop: //Bottom of the content (0, 0).
                    canvasUISettings.scrollRectVertical.normalizedPosition = Vector2.zero;
                    break;
                case MapOrientation.TopToBottom: //Top of the content (0, 1).
                    canvasUISettings.scrollRectVertical.normalizedPosition = new Vector2(0, 1);
                    break;
                case MapOrientation.RightToLeft: //Right side of the content (1, 0).
                    canvasUISettings.scrollRectHorizontal.normalizedPosition = new Vector2(1, 0);
                    break;
                case MapOrientation.LeftToRight: //Left side of the content (0, 0)
                    canvasUISettings.scrollRectHorizontal.normalizedPosition = Vector2.zero;
                    break;
                default:
                    break;
            }
        }


        // Creates a RoomView for each Room in the provided collection and adds it to the roomViews list.
        // Iterates through the rooms, generating a visual representation (RoomView) for each one.
        private void CreateRooms(List<Room> rooms)
        {
            foreach (Room room in rooms)
            {
                RoomView roomView = CreateRoomView(room);
                roomViews.Add(roomView);
            }
        }

        // Creates and configures a RoomView for the given Room.
        private RoomView CreateRoomView(Room room)
        {
            GameObject prefab = WorldSpace ? worldSpaceSettings.roomPrefab :
                                CanvasUI ? canvasUISettings.roomPrefab : null;

            if (prefab == null)
            {
                Debug.LogError("Prefab không được chỉ định.");
                return null;
            }

            //Create object using prefab and set its parent
            GameObject roomViewObject = Instantiate(prefab, mapParent.transform);
            // Instantiates a RoomView from the roomPrefab under mapParent, retrieves its blueprint (RoomNode),
            RoomView roomView = roomViewObject.GetComponent<RoomView>();
            if (roomView == null)
            {
                Debug.LogError("Prefab không chứa component RoomView.");
                return null;
            }
            RoomNode roomNode = GetBlueprint(room.blueprintName);
            // sets up the RoomView with the Room data and blueprint
            roomView.SetUp(room, roomNode);

            roomView.transform.localPosition = WorldSpace
                                                ? room.position // If WorldSpace
                                                : GetRoomPosition(room); // If CanvasUI

            return roomView;
        }

        // Calculates the 2D position of a Room on the map based on the map's orientation.
        // Uses the map's length and adjusts the position with background padding and room position, flipped for horizontal orientations.
        private Vector2 GetRoomPosition(Room room)
        {
            //Debug.Log($"Total distance: {Map.DistanceBetweenFirstAndLastFloors()}");
            float length = canvasUISettings.padding + Map.DistanceBetweenFirstAndLastFloors() * canvasUISettings.unitsToPixelsMultiplier;

            //Debug.Log($"Total length: {length}");
            //Debug.Log($"Room: {room.roomAddress}");

            switch (orientation)
            {
                case MapOrientation.BottomToTop:
                    return new Vector2(-canvasUISettings.backgroundPadding.x / 2f, (canvasUISettings.padding - length) / 2f) +
                           room.position * canvasUISettings.unitsToPixelsMultiplier;
                case MapOrientation.TopToBottom:
                    return new Vector2(canvasUISettings.backgroundPadding.x / 2f, (length - canvasUISettings.padding) / 2f) -
                           room.position * canvasUISettings.unitsToPixelsMultiplier;
                case MapOrientation.RightToLeft:
                    return new Vector2((length - canvasUISettings.padding) / 2f, canvasUISettings.backgroundPadding.y / 2f) -
                           Flip(room.position) * canvasUISettings.unitsToPixelsMultiplier;
                case MapOrientation.LeftToRight:
                    return new Vector2((canvasUISettings.padding - length) / 2f, -canvasUISettings.backgroundPadding.y / 2f) +
                           Flip(room.position) * canvasUISettings.unitsToPixelsMultiplier;
                default:
                    return Vector2.zero;
            }
        }

        private static Vector2 Flip(Vector2 other) => new Vector2(other.y, other.x);

        private void DrawLines()
        {
            foreach (RoomView room in roomViews)
            {
                foreach (Vector2Int connection in room.room.outgoing)
                    AddLineConnection(room, GetRoom(connection));
            }
        }

        private void SetOrientation()
        {
            if (WorldSpace)
            {
                //ScrollNonUI scrollNonUi = mapParent.GetComponent<ScrollNonUI>();
                //float span = mapManager.currentMap.DistanceBetweenFirstAndLastLayers();
                //MapNode bossNode = RoomViews.FirstOrDefault(node => node.Node.nodeType == NodeType.Boss);
                //Debug.Log("Map span in set orientation: " + span + " camera aspect: " + cam.aspect);

                //// setting first parent to be right in front of the camera first:
                //firstParent.transform.position = new Vector3(cam.transform.position.x, cam.transform.position.y, 0f);
                //float offset = orientationOffset;
                //switch (orientation)
                //{
                //    case MapOrientation.BottomToTop:
                //        if (scrollNonUi != null)
                //        {
                //            scrollNonUi.yConstraints.max = 0;
                //            scrollNonUi.yConstraints.min = -(span + 2f * offset);
                //        }
                //        firstParent.transform.localPosition += new Vector3(0, offset, 0);
                //        break;
                //    case MapOrientation.TopToBottom:
                //        mapParent.transform.eulerAngles = new Vector3(0, 0, 180);
                //        if (scrollNonUi != null)
                //        {
                //            scrollNonUi.yConstraints.min = 0;
                //            scrollNonUi.yConstraints.max = span + 2f * offset;
                //        }
                //        // factor in map span:
                //        firstParent.transform.localPosition += new Vector3(0, -offset, 0);
                //        break;
                //    case MapOrientation.RightToLeft:
                //        offset *= cam.aspect;
                //        mapParent.transform.eulerAngles = new Vector3(0, 0, 90);
                //        // factor in map span:
                //        firstParent.transform.localPosition -= new Vector3(offset, bossNode.transform.position.y, 0);
                //        if (scrollNonUi != null)
                //        {
                //            scrollNonUi.xConstraints.max = span + 2f * offset;
                //            scrollNonUi.xConstraints.min = 0;
                //        }
                //        break;
                //    case MapOrientation.LeftToRight:
                //        offset *= cam.aspect;
                //        mapParent.transform.eulerAngles = new Vector3(0, 0, -90);
                //        firstParent.transform.localPosition += new Vector3(offset, -bossNode.transform.position.y, 0);
                //        if (scrollNonUi != null)
                //        {
                //            scrollNonUi.xConstraints.max = 0;
                //            scrollNonUi.xConstraints.min = -(span + 2f * offset);
                //        }
                //        break;
                //    default:
                //        throw new ArgumentOutOfRangeException();
                //}
            }    
            

            if (CanvasUI)
            {

            }    
        }

        // Resets the rotation of all RoomView objects in the roomViews list to their default orientation (no rotation).
        private void ResetNodesRotation()
        {
            foreach (RoomView room in roomViews)
                room.transform.rotation = Quaternion.identity;
        }

        // Updates the state of all RoomView objects based on the player's progress on the current map.
        public void SetAttainableRooms()
        {
            // Set all the rooms as locked:
            foreach (RoomView room in roomViews)
                room.SetState(RoomStates.Locked);

            if (mapManager.currentMap.path.Count == 0)
            {
                // we have not started traveling on this map yet, set entire first floor as attainable:
                foreach (RoomView room in roomViews.Where(n => n.room.roomAddress.y == 0))
                    room.SetState(RoomStates.Attainable);
            }
            else
            {
                // We have already started moving on this map, first highlight the path as visited:
                foreach (Vector2Int point in mapManager.currentMap.path)
                {
                    RoomView roomView = GetRoom(point);
                    if (roomView != null)
                        roomView.SetState(RoomStates.Visited);
                }

                // Get current room
                Vector2Int currentPoint = mapManager.currentMap.path[mapManager.currentMap.path.Count - 1];
                Room currentRoom = mapManager.currentMap.GetRoom(currentPoint);

                // Set all the rooms that we can travel to as attainable:
                foreach (Vector2Int point in currentRoom.outgoing)
                {
                    RoomView roomView = GetRoom(point);
                    if (roomView != null)
                        roomView.SetState(RoomStates.Attainable);
                }
            }
        }

        private void CreateMapBackground(Map m)
        {
            if (background == null) return;

            if (WorldSpace)
            {
                //GameObject backgroundObject = new GameObject("Background");
                //backgroundObject.transform.SetParent(mapParent.transform);
                //RoomView bossNode = roomViews.FirstOrDefault(node => node.room.roomType == RoomType.Boss);
                //float span = m.DistanceBetweenFirstAndLastFloors();
                //backgroundObject.transform.localPosition = new Vector3(bossNode.transform.localPosition.x, span / 2f, 0f);
                //backgroundObject.transform.localRotation = Quaternion.identity;
                //SpriteRenderer sr = backgroundObject.AddComponent<SpriteRenderer>();
                //sr.color = backgroundColor;
                //sr.drawMode = SpriteDrawMode.Sliced;
                //sr.sprite = background;
                //sr.size = new Vector2(xSize, span + yOffset * 2f);
            }

            // Creates and configures a background GameObject for the map.
            // Attaches it to mapParent, stretches it to fit with backgroundPadding, and sets it as the first sibling.
            // Adds an Image component with the specified background sprite, color, sliced type, and pixel multiplier.
            if (CanvasUI)
            {
                GameObject backgroundObject = new GameObject("Background");
                backgroundObject.transform.SetParent(mapParent.transform);
                backgroundObject.transform.localScale = Vector3.one;
                RectTransform rt = backgroundObject.AddComponent<RectTransform>();
                Stretch(rt);
                rt.SetAsFirstSibling();
                rt.sizeDelta = canvasUISettings.backgroundPadding;

                UnityEngine.UI.Image image = backgroundObject.AddComponent<UnityEngine.UI.Image>();
                image.color = backgroundColor;
                image.type = UnityEngine.UI.Image.Type.Sliced;
                image.sprite = background;
                image.pixelsPerUnitMultiplier = canvasUISettings.backgroundPPUMultiplier;
            }    
        }

       

        

        public void SetLineColors()
        {
            // set all lines to grayed out first:
            foreach (LineConnection connection in lineConnections)
                connection.SetColor(lineLockedColor);

            // set all lines that are a part of the path to visited color:
            // if we have not started moving on the map yet, leave everything as is:
            if (mapManager.currentMap.path.Count == 0)
                return;

            // in any case, we mark outgoing connections from the final node with visible/attainable color:
            Vector2Int currentPoint = mapManager.currentMap.path[mapManager.currentMap.path.Count - 1];
            Room currentRoom = mapManager.currentMap.GetRoom(currentPoint);

            foreach (Vector2Int point in currentRoom.outgoing)
            {
                LineConnection lineConnection = lineConnections.FirstOrDefault(conn => conn.from.room == currentRoom &&
                                                                            conn.to.room.roomAddress.Equals(point));
                lineConnection?.SetColor(lineVisitedColor);
            }

            if (mapManager.currentMap.path.Count <= 1) return;

            for (int i = 0; i < mapManager.currentMap.path.Count - 1; i++)
            {
                Vector2Int current = mapManager.currentMap.path[i];
                Vector2Int next = mapManager.currentMap.path[i + 1];
                LineConnection lineConnection = lineConnections.FirstOrDefault(conn => conn.@from.room.roomAddress.Equals(current) &&
                                                                            conn.to.room.roomAddress.Equals(next));
                lineConnection?.SetColor(lineVisitedColor);
            }
        }

        

       

        

        protected virtual void AddLineConnection(RoomView from, RoomView to)
        {
            if (WorldSpace)
            { 

            }
            if (CanvasUI)
            {
                GameObject line = new GameObject("Line Connection");
                line.transform.SetParent (mapParent.transform, false);
                line.transform.SetAsFirstSibling();

                UIDottedCircleLine dottedLine = line.AddComponent<UIDottedCircleLine>();
                dottedLine.maskable = true;

                Vector2 start = from.transform.localPosition;
                Vector2 end = to.transform.localPosition;

                Vector2 dir = (end - start).normalized;
                start += dir * canvasUISettings.offsetFromRooms;
                end -= dir * canvasUISettings.offsetFromRooms;

                dottedLine.SetLine(start, end);
                lineConnections.Add(new LineConnection(null, dottedLine, from, to));
            }
        }

        protected RoomView GetRoom(Vector2Int p)
        {
            return roomViews.FirstOrDefault(n => n.room.roomAddress.Equals(p));
        }

        protected MapConfig GetConfig(string configName)
        {
            //Debug.Log($"Find config: {configName}");
            MapConfig config = allMapConfigs.FirstOrDefault(c => c.name == configName);
            if (config == null)
            {
                //Debug.Log("Null ở GetConfig");
                return null; 
            }
            else
            {
                return config;
            }    
        }

        //protected RoomNode GetBlueprint(RoomType type)
        //{
        //    MapConfig config = GetConfig(mapManager.currentMap.configName);
        //    return config.roomNodes.FirstOrDefault(n => n.roomType == type);
        //}

        private RoomNode GetBlueprint(string blueprintName)
        {
            //Debug.Log($"Find Blueprint: {blueprintName}");
            MapConfig config = GetConfig(mapManager.currentMap.configName);
            //Debug.Log("Chạy đến đây 1");
            RoomNode roomNode = config.roomNodes.FirstOrDefault(n => n.name == blueprintName);
            if(roomNode != null)
            {
                //Debug.Log($"Không null: {roomNode.roomType}");
                return roomNode;
            }
            else
            {
                //Debug.Log("Null rồi");
                return null;    
            }    
        }
    }
}
