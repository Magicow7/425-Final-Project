using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BetweenRoomPath : MonoBehaviour
{
    public Room startRoom;
    public Room endRoom;
    public List<Vector2> pathList;
    public bool blocked;
    public BetweenRoomPath(List<Vector2> pathList, Room startRoom, Room endRoom){
        blocked = false;
        this.startRoom = startRoom;
        this.endRoom = endRoom;
        this.pathList = pathList;


    }
}
