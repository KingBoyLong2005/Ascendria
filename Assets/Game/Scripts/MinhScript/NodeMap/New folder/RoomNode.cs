using UnityEngine;

namespace Map
{
    public enum RoomType
    {
        NormalMonster,
        EliteMonster,
        RestSite,
        Shop,
        Event,
        Boss
    }

    [CreateAssetMenu(menuName = "ScriptableObject/RoomNode")]
    public class RoomNode : ScriptableObject
    {
        public RoomType roomType;
        public Sprite sprite;
    }
}