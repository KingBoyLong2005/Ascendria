using Map;
using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.Linq;

public class Room
{
    public readonly Vector2Int roomAddress;
    public readonly List<Vector2Int> incoming = new List<Vector2Int>();
    public readonly List<Vector2Int> outgoing = new List<Vector2Int>();

    [JsonConverter(typeof(StringEnumConverter))]
    public readonly RoomType roomType;

    public readonly string blueprintName;
    public Vector2 position;

    public Room(RoomType roomType, string blueprintName, Vector2Int roomAddress)
    {
        this.roomType = roomType;
        this.blueprintName = blueprintName;
        this.roomAddress = roomAddress;
    }

    public void Log()
    {
        //Debug.Log($"Room: {roomAddress.x},{roomAddress.y} - Type: {roomType} - Name: {blueprintName} - Pos: {position}");
    }


    public void AddIncoming(Vector2Int address)
    {
        if(incoming.Any(value  => value.Equals(address))) return;
        incoming.Add(address);
    }
    
    public void AddOutgoing(Vector2Int address)
    {
        if(outgoing.Any(value => value.Equals(address))) return;
        outgoing.Add(address);
    }

    public void RemoveIncoming(Vector2Int address)
    {
        incoming.RemoveAll(value => value.Equals(address));
    }

    public void RemoveOutgoing(Vector2Int address)
    {
        outgoing.RemoveAll(value => value.Equals(address));
    }

    public bool HasNoConnections()
    {
        return incoming.Count == 0 && outgoing.Count == 0;
    }
}
