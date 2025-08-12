using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UI.Extensions;

namespace Map
{
    //For Canvas UI
    public class MapViewUI : MapView
    {
        protected override void AddLineConnection(RoomView from, RoomView to)
        {
            //UILineRenderer lineRenderer = Instantiate(uiLinePrefab, mapParent.transform);
            //lineRenderer.transform.SetAsFirstSibling();
            //RectTransform fromRT = from.transform as RectTransform;
            //RectTransform toRT = to.transform as RectTransform;
            //Vector2 fromPoint = fromRT.anchoredPosition +
            //                    (toRT.anchoredPosition - fromRT.anchoredPosition).normalized * offsetFromNodes;

            //Vector2 toPoint = toRT.anchoredPosition +
            //                  (fromRT.anchoredPosition - toRT.anchoredPosition).normalized * offsetFromNodes;

            //// drawing lines in local space:
            //lineRenderer.transform.position = from.transform.position +
            //                                  (Vector3)(toRT.anchoredPosition - fromRT.anchoredPosition).normalized *
            //                                  offsetFromNodes;

            //// line renderer with 2 points only does not handle transparency properly:
            //List<Vector2> list = new List<Vector2>();
            //for (int i = 0; i < linePointsCount; i++)
            //{
            //    list.Add(Vector3.Lerp(Vector3.zero, toPoint - fromPoint +
            //                                        2 * (fromRT.anchoredPosition - toRT.anchoredPosition).normalized *
            //                                        offsetFromNodes, (float)i / (linePointsCount - 1)));
            //}

            //Debug.Log("From: " + fromPoint + " to: " + toPoint + " last point: " + list[list.Count - 1]);

            //lineRenderer.Points = list.ToArray();

            //DottedLineRenderer dottedLine = lineRenderer.GetComponent<DottedLineRenderer>();
            //if (dottedLine != null) dottedLine.ScaleMaterial();

            //lineConnections.Add(new LineConnection(null, lineRenderer, from, to));
        }
    }
}
/*
using System;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;


namespace Map
{
    [System.Serializable]
    public class WorldSpaceSettings
    {
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
        [SerializeField] public float padding = 1000;
        [Tooltip("Padding of the background from the sides of the scroll rect")]
        [SerializeField] public Vector2 backgroundPadding = new Vector2(-100,-100);
        [Tooltip("Pixels per Unit multiplier for the background image")]
        [SerializeField] public float backgroundPPUMultiplier = 2;
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

        [HideInInspector] public bool CanvasUI;   // Checkbox Canvas UI
        [HideInInspector] public bool WorldSpace; // Checkbox World Space

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

        // ALL nodes:
        public readonly List<RoomView> roomViews = new List<RoomView>();

        public static MapView Instance;

        public Map Map { get; private set; }

        //
        private void Awake()
        {
            Instance = this;
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

            CreateMapBackground(m);
        }

        //Xóa dữ liệu map cũ nếu có
        protected void ClearMap()
        {
            if (WorldSpace)
            {
            }

            if (CanvasUI)
            {
                canvasUISettings.scrollRectHorizontal.gameObject.SetActive(false);
                canvasUISettings.scrollRectVertical.gameObject.SetActive(false);

                foreach (ScrollRect scrollRect in new[] { canvasUISettings.scrollRectHorizontal, canvasUISettings.scrollRectVertical })
                    foreach (Transform t in scrollRect.content)
                        Destroy(t.gameObject);

                roomViews.Clear();
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
                                                : GetNodePosition(room); // If CanvasUI

            return roomView;
        }

        // Calculates the 2D position of a Room on the map based on the map's orientation.
        // Uses the map's length and adjusts the position with background padding and room position, flipped for horizontal orientations.
        private Vector2 GetNodePosition(Room room)
        {
            float length = canvasUISettings.padding + Map.DistanceBetweenFirstAndLastFloors() * canvasUISettings.unitsToPixelsMultiplier;

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


        private void CreateMapBackground(Map m)
        {
            if (background == null) return;

            if (WorldSpace)
            {
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

        protected MapConfig GetConfig(string configName)
        {
            MapConfig config = allMapConfigs.FirstOrDefault(c => c.name == configName);
            if (config == null)
            {
                return null;
            }
            else
            {
                return config;
            }
        }

        private RoomNode GetBlueprint(string blueprintName)
        {
            MapConfig config = GetConfig(mapManager.currentMap.configName);
            RoomNode roomNode = config.roomNodes.FirstOrDefault(n => n.name == blueprintName);
            if (roomNode != null)
            {
                return roomNode;
            }
            else
            {
                return null;
            }
        }
    }
}
*/