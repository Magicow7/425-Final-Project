using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Room : MonoBehaviour
{
        public RectInt bounds;
        public string identifier;
        //public BattleHandler battleHandler;
        //public Dictionary<string, BetweenRoomPath> paths;

        public Room(Vector2Int location, Vector2Int size, string identifier) {
            bounds = new RectInt(location, size);
            this.identifier = identifier;
            //paths = new Dictionary<string, BetweenRoomPath>();
        }

        public static bool Intersect(Room a, Room b) {
            return !((a.bounds.position.x >= (b.bounds.position.x + b.bounds.size.x)) || ((a.bounds.position.x + a.bounds.size.x) <= b.bounds.position.x)
                || (a.bounds.position.y >= (b.bounds.position.y + b.bounds.size.y)) || ((a.bounds.position.y + a.bounds.size.y) <= b.bounds.position.y));
        }

        public static bool withinRange(Vector2Int position, Room b){
            return position.x >= b.bounds.position.x && position.x <= b.bounds.position.x + b.bounds.size.x-1 && position.y >= b.bounds.position.y && position.y <= b.bounds.position.y + b.bounds.size.y-1;
        }
        /*
        public void addPath(BetweenRoomPath path){
            paths.Add(path.endRoom.identifier, path);
        }*/

}
