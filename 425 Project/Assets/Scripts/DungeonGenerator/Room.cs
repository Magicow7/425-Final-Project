using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class Room : MonoBehaviour
{
        [FormerlySerializedAs("bounds")] public RectInt _bounds;
        [FormerlySerializedAs("identifier")] public string _identifier;
        //public BattleHandler battleHandler;
        //public Dictionary<string, BetweenRoomPath> paths;

        public Room(Vector2Int location, Vector2Int size, string identifier) {
            _bounds = new RectInt(location, size);
            _identifier = identifier;
            //paths = new Dictionary<string, BetweenRoomPath>();
        }

        public static bool Intersect(Room a, Room b) {
            return !((a._bounds.position.x >= (b._bounds.position.x + b._bounds.size.x)) || ((a._bounds.position.x + a._bounds.size.x) <= b._bounds.position.x)
                || (a._bounds.position.y >= (b._bounds.position.y + b._bounds.size.y)) || ((a._bounds.position.y + a._bounds.size.y) <= b._bounds.position.y));
        }

        public static bool WithinRange(Vector2Int position, Room b){
            return position.x >= b._bounds.position.x && position.x <= b._bounds.position.x + b._bounds.size.x-1 && position.y >= b._bounds.position.y && position.y <= b._bounds.position.y + b._bounds.size.y-1;
        }
        /*
        public void addPath(BetweenRoomPath path){
            paths.Add(path.endRoom.identifier, path);
        }*/

}
