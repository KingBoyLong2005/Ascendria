using System;
using System.Collections.Generic;
using UnityEngine;

namespace Map
{
    [CreateAssetMenu(menuName = "ScriptableObject/MapConfig")]
    public class MapConfig : ScriptableObject
    {
        [Tooltip("List of all scriptable object room node")]
        public List<RoomNode> roomNodes;    

        [Tooltip("Room types that will be used on floors with Randomize Rooms > 0")]
        public List<RoomType> randomRooms = new List<RoomType>
            {RoomType.NormalMonster, RoomType.EliteMonster, RoomType.Shop, RoomType.Event, RoomType.RestSite};

        //Calculate the width of the map based on the floor which have the most number of rooms
        public int gridWidth => Mathf.Max(numOfPreBossRooms.max, numOfStartingRooms.max);

        [Tooltip("Number of rooms on each floor before boss")]
        [MinMaxSlider(1, 5)]
        public MinMaxInt numOfPreBossRooms;
        [Tooltip("Number of rooms on the starting floor")]
        [MinMaxSlider(1, 5)]
        public MinMaxInt numOfStartingRooms;

        [Tooltip("Increase this number to generate more paths")]
        public int extraPaths;
        public List<FloorConfig> floors;
    }
}