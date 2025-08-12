using UnityEngine;
using UnityEditor;

namespace Map
{
    [System.Serializable]
    public class FloorConfig
    {
        [Tooltip("Default room type for this floor. If Randomize Nodes is 0, you will get this type 100% of the time")]
        public RoomType roomType;

        [Tooltip("Distance From Previous Floor: Min/Max")]
        [MinMaxSlider(2, 5)]
        public MinMaxFloat distanceFromPreviousFloor;

        [Range(2f, 3f)] public float distanceBetweenRoomsOnFloor;

        [Tooltip("If this is set to 0, nodes on this layer will appear in a straight line. Closer to 1f = more position randomization")]
        [Range(0f, 1f)] public float randomizePosition;

        [Tooltip("Chance to get a random room type that is different from the default type on this floor")]
        [Range(0f, 1f)] public float randomizeRooms;
    }
}