using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class BetweenRoomPath : MonoBehaviour
{
    [FormerlySerializedAs("startRoom")] public Room _startRoom;
    [FormerlySerializedAs("endRoom")] public Room _endRoom;
    [FormerlySerializedAs("pathList")] public List<Vector2> _pathList;
    [FormerlySerializedAs("blocked")] public bool _blocked;
    public BetweenRoomPath(List<Vector2> pathList, Room startRoom, Room endRoom){
        _blocked = false;
        _startRoom = startRoom;
        _endRoom = endRoom;
        _pathList = pathList;


    }
}
