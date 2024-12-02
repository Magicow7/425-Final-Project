﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = System.Random;
using Graphs;
using System;
using Locomotion;
using UnityEngine.Serialization;


public class Generator2D : MonoBehaviour {

    public static Generator2D instance;

    

    [FormerlySerializedAs("size"),SerializeField]
    Vector2Int _size;
    [FormerlySerializedAs("worldScale"),SerializeField]
    float _worldScale;
    [FormerlySerializedAs("roomPlacePasses"),SerializeField]
    private List<RoomPlaceParams> _roomPlacePasses = new List<RoomPlaceParams>();
    /*
    [SerializeField]
    
    int roomCount;
    [SerializeField]
    Vector2Int roomMaxSize;
    [SerializeField]
    Vector2Int roomMinSize;
    */
    [FormerlySerializedAs("hallwayTile"),SerializeField]
    GameObject _hallwayTile;
    [FormerlySerializedAs("roomTile"),SerializeField]
    GameObject _roomTile;
    [FormerlySerializedAs("player"),SerializeField]
    GameObject _player;

    [FormerlySerializedAs("enemySpawner"),SerializeField]
    EnemySpawner _enemySpawner;

    [FormerlySerializedAs("navMeshSetup"),SerializeField]
    NavMeshSetup _navMeshSetup;

    [FormerlySerializedAs("nonMSTHallwayChance"),SerializeField]
    float _nonMstHallwayChance; // default was 1.25f

    //[SerializeField]
    //HeroSpawner HeroSpawner;

    Random _random;
    Grid2D<CellType> _grid;//grid used for making hallways
    Grid2D<Tile> _tileGrid;//grid used for putting tiles on hallways
    Dictionary<string, Room> _rooms;
    
    Delaunay2D _delaunay;
    DijkstrasGraph<Room> _dij;
    HashSet<Prim.Edge> _selectedEdges;
    HashSet<Prim.Edge> _savedMst = new HashSet<Prim.Edge>();
    [FormerlySerializedAs("spawnedThings")] public List<GameObject> _spawnedThings;
    List<BetweenRoomPath> _betweenRoomPaths = new List<BetweenRoomPath>();
    Room _startRoom;
    Room _endRoom;

    int _failedRoomPlacements = 0;

    int _generationIteration = 0;

    private int _addedRooms = 0;

    [Serializable]
    struct RoomPlaceParams {
        [FormerlySerializedAs("roomCount")] public int _roomCount;
        [FormerlySerializedAs("roomMaxSize")] public Vector2Int _roomMaxSize;
        [FormerlySerializedAs("roomMinSize")] public Vector2Int _roomMinSize;
    }

    void Start() {
        Destroy(instance);
        instance = this;
        Generate();
    }

    void Regenerate(){
        foreach(GameObject obj in _spawnedThings){
            Destroy(obj);
        }
        _generationIteration += 1;
        Generate();
    }

    void Generate() {
        
        var currentGenerationIteration = _generationIteration;
        _random = new Random();//new Random(0) for set seed I think?
        _grid = new Grid2D<CellType>(_size, Vector2Int.zero);
        _tileGrid = new Grid2D<Tile>(_size, Vector2Int.zero);
        _rooms = new Dictionary<string, Room>();
        _dij = new DijkstrasGraph<Room>();
        _betweenRoomPaths = new List<BetweenRoomPath>();
        _spawnedThings = new List<GameObject>();
        _startRoom = null;
        _endRoom = null;
        Tile.ClearActivatedWalls();

        if(currentGenerationIteration == _generationIteration){
            for(int i = 0; i < _roomPlacePasses.Count; i++){
                PlaceRooms(_roomPlacePasses[i]);
            }
            
        }
        if(currentGenerationIteration == _generationIteration){
            Triangulate();
        }
        if(currentGenerationIteration == _generationIteration){
            CreateHallways();
        }
        if(currentGenerationIteration == _generationIteration){
            PathfindHallways();
        }
        if(currentGenerationIteration == _generationIteration){
            //FindEndRoom();
        }
        if(currentGenerationIteration == _generationIteration){
            GeneratePathingInHallWays();
        }
        if(currentGenerationIteration == _generationIteration){
            SetTileWalls();
        }
        if(currentGenerationIteration == _generationIteration){
            PlacePlayer();
        }
        if(currentGenerationIteration == _generationIteration){
            SetupEnemies();
        }
        if(currentGenerationIteration == _generationIteration){
            //Instantiate(HeroSpawner,transform.position,Quaternion.identity);
        }
        
        
   
    }

    //randomly places rooms around the grid with the given paramaters
    void PlaceRooms(RoomPlaceParams param) {
        
        for (int i = 0; i < param._roomCount; i++) {
            Vector2Int location = new Vector2Int(
                _random.Next(0, _size.x),
                _random.Next(0, _size.y)
            );

            Vector2Int roomSize = new Vector2Int(
                _random.Next(param._roomMinSize.x, param._roomMaxSize.x + 1),
                _random.Next(param._roomMinSize.y, param._roomMaxSize.y + 1)
            );

            bool add = true;
            Room newRoom = new Room(location, roomSize, (_addedRooms + 1).ToString());
            Room buffer = new Room(location + new Vector2Int(-1, -1), roomSize + new Vector2Int(2, 2), "0");

            foreach (string room in _rooms.Keys) {
                if (Room.Intersect(_rooms[room], buffer)) {
                    add = false;
                    break;
                }
            }

            if (newRoom._bounds.xMin < 0 || newRoom._bounds.xMax >= _size.x
                || newRoom._bounds.yMin < 0 || newRoom._bounds.yMax >= _size.y) {
                add = false;
            }

            if (add) {
                _addedRooms += 1;
                _rooms.Add(newRoom._identifier, newRoom);
                //GameObject newBattle = Instantiate(battleHandler,new Vector3(newRoom.bounds.center.x,0,newRoom.bounds.center.y), Quaternion.identity);
                //newRoom.battleHandler = newBattle.GetComponent<BattleHandler>();
                //spawnedThings.Add(newBattle);
                int rand = _random.Next(0,2);
                if(rand == 0){
                    //newBattle.transform.eulerAngles = new Vector3(0, 90, 0);
                }
                PlaceRoom(newRoom._bounds.position,newRoom._bounds.size,newRoom);
                    


                foreach (var pos in newRoom._bounds.allPositionsWithin) {
                    _grid[pos] = CellType.Room;
                }
            }else{
                if(_failedRoomPlacements > param._roomCount * 50){
                    _failedRoomPlacements = 0;
                    Debug.Log("Regenerating dungeon");
                    Regenerate();
                    return;
                }else{
                    _failedRoomPlacements ++;
                    i--;
                }
                
            }
        }
    }
    
    
    //triangulates the distances between rooms with delauney triangulation, which I don't really understand
    void Triangulate() {
        List<Vertex> vertices = new List<Vertex>();

        foreach (string room in _rooms.Keys) {
            vertices.Add(new Vertex<Room>((Vector2)_rooms[room]._bounds.position + ((Vector2)_rooms[room]._bounds.size) / 2, _rooms[room]));
        }

        _delaunay = Delaunay2D.Triangulate(vertices);
    }

    //finds the minimum spanning tree and makes hallway lines between on the MST paths and a few random non MST paths based on random chance.
    void CreateHallways() {
        List<Prim.Edge> edges = new List<Prim.Edge>();

        foreach (var edge in _delaunay.Edges) {
            edges.Add(new Prim.Edge(edge.U, edge.V));
        }

       
        List<Prim.Edge> mst = Prim.MinimumSpanningTree(edges, edges[0].U);    

        _selectedEdges = new HashSet<Prim.Edge>(mst);
        var remainingEdges = new HashSet<Prim.Edge>(edges);
        remainingEdges.ExceptWith(_selectedEdges);

        int remainingCount = (int)((float)remainingEdges.Count * _nonMstHallwayChance);
        Debug.Log(remainingCount + "/" + remainingEdges.Count + "extra paths added");
        for(int i = 0; i <  remainingCount; i++){
            List<Prim.Edge> setToList = new List<Prim.Edge>();
            foreach (var edge in remainingEdges) {
                setToList.Add(edge);
            }
            Prim.Edge element = setToList[_random.Next(setToList.Count)];
            _selectedEdges.Add(element);
            remainingEdges.Remove(element);

        }

        _savedMst = _selectedEdges;

        
        /*
        foreach (var edge in remainingEdges) {
            if (random.NextDouble() < nonMSTHallwayChance) {
                selectedEdges.Add(edge);
            }
        }
        */
    }

    //find closest bit of each room to the centerpoint of the opposite room.
    List<Vector2Int> GetClosestEdgeOfRoom(Room room1, Room room2){
        List<Vector2Int> returnList = new List<Vector2Int>();
        Vector2Int room1Point = Vector2Int.RoundToInt(room1._bounds.center);
        Vector2Int room2Point = Vector2Int.RoundToInt(room2._bounds.center);
        while(room1Point.x != room2Point.x){
            if(room1Point.x < room2Point.x){
                room1Point.x++;
                if(!Room.WithinRange(room1Point,room1)){
                    room1Point.x--;
                    break;
                }
            }else{
                room1Point.x--;
                if(!Room.WithinRange(room1Point,room1)){
                    room1Point.x++;
                    break;
                }
            }
        }
        while(room1Point.y != room2Point.y){
            if(room1Point.y < room2Point.y){
                room1Point.y++;
                if(!Room.WithinRange(room1Point,room1)){
                    room1Point.y--;
                    break;
                }
            }else{
                room1Point.y--;
                if(!Room.WithinRange(room1Point,room1)){
                    room1Point.y++;
                    break;
                }
            }
        }
        while(room2Point.x != room1Point.x){
            if(room2Point.x < room1Point.x){
                room2Point.x++;
                if(!Room.WithinRange(room2Point,room2)){
                    room2Point.x--;
                    break;
                }
            }else{
                room2Point.x--;
                if(!Room.WithinRange(room2Point,room2)){
                    room2Point.x++;
                    break;
                }
            }
        }
        while(room2Point.y != room1Point.y){
            if(room2Point.y < room1Point.y){
                room2Point.y++;
                if(!Room.WithinRange(room2Point,room2)){
                    room2Point.y--;
                    break;
                }
            }else{
                room2Point.y--;
                if(!Room.WithinRange(room2Point,room2)){
                    room2Point.y++;
                    break;
                }
            }
        }
        returnList.Add(room1Point);
        returnList.Add(room2Point);

        return returnList;
    }

    //uses A* algorithm to pathfind between the rooms for each edge found during createHallways();
    void PathfindHallways() {
        DungeonPathfinder2D aStar = new DungeonPathfinder2D(_size);

        foreach (var edge in _selectedEdges) {
            var startRoom = (edge.U as Vertex<Room>).Item;
            var endRoom = (edge.V as Vertex<Room>).Item;

            List<Vector2Int> optimalStartPoints = GetClosestEdgeOfRoom(startRoom, endRoom);

            var startPosf = optimalStartPoints[0];
            var endPosf = optimalStartPoints[1];
            var startPos = new Vector2Int((int)startPosf.x, (int)startPosf.y);
            var endPos = new Vector2Int((int)endPosf.x, (int)endPosf.y);

            var path = aStar.FindPath(startPos, endPos, (DungeonPathfinder2D.Node a, DungeonPathfinder2D.Node b) => {
                var pathCost = new DungeonPathfinder2D.PathCost();
                
                pathCost.cost = Vector2Int.Distance(b.Position, endPos);    //heuristic

                if (_grid[b.Position] == CellType.Room) {
                    pathCost.cost += 50;
                } else if (_grid[b.Position] == CellType.None) {
                    pathCost.cost += 5;
                } else if (_grid[b.Position] == CellType.Hallway) {
                    pathCost.cost += 1;
                }

                pathCost.traversable = true;

                return pathCost;
            });

            if (path != null) {
                for (int i = 0; i < path.Count; i++) {
                    var current = path[i];

                    if (_grid[current] == CellType.None) {
                        _grid[current] = CellType.Hallway;
                    }

                    if (i > 0) {
                        var prev = path[i - 1];

                        var delta = current - prev;
                    }
                }

                foreach (var pos in path) {
                    if (_grid[pos] == CellType.Hallway) {
                        PlaceHallway(pos);
                    }
                }
            }
        }
    }


    /*void FindEndRoom(){
        List<int> costs = new List<int>();
        foreach(string room in rooms.Keys){
            //add rooms to graph and add
            dij.addVertex(room, rooms[room]);

        }
        foreach(Prim.Edge edge in selectedEdges){
            var room1 = (edge.U as Vertex<Room>).Item;
            var room2 = (edge.V as Vertex<Room>).Item;
            dij.addEdge(room1.identifier,room2.identifier,5);//the value 5 is the default cost to move between two rooms
        }


        float before = Time.realtimeSinceStartup;
        List<string> orderedRooms = dij.doBreadthFirstSearch("1");
        float after = Time.realtimeSinceStartup;
        float duration = (after-before);
        Debug.Log("Duration in milliseconds to perform breadthFirst: " + duration);    

        //check to see how far away the start and end rooms are, if they are too close, regenerate the dungeon
        List<string> dijkstrasPath = dij.doDijkstras(rooms[orderedRooms[0]].identifier,rooms[orderedRooms[orderedRooms.Count-1]].identifier);
        if(dijkstrasPath.Count < minimumStartToEndDistance){
            Debug.Log("Regenerating dungeon");
            Regenerate();
            return;
        }

        //deprecated code to display start and end rooms
        /*
        PlaceCube(rooms[orderedRooms[0]].bounds.position,CellType.None, 1);
        PlaceCube(rooms[orderedRooms[orderedRooms.Count-1]].bounds.position,CellType.None, 1);
        

        new GlobalPathfindingCenter(dij,rooms,rooms[orderedRooms[0]],rooms[orderedRooms[orderedRooms.Count-1]]);
    }*/

    void GeneratePathingInHallWays(){
        //displays dungeon as text in debug console
        /*
        CellType[][] gottenList = grid.getAs2DArray();
        string printString = "";
        for(int i = 0; i < size.x; i++){
            for(int j = 0; j < size.y; j++){
                CellType found = gottenList[i][j];
                if(found == CellType.None){
                    printString += "N";
                }else if(found == CellType.Room){
                    printString += "O";
                }else{
                    printString += "H";
                }
            }
            printString += "\n";
        }
        Debug.Log(printString);
        */
        

        DungeonPathfinder2D aStar = new DungeonPathfinder2D(_size);

        foreach (var edge in _selectedEdges) {
            var startRoom = (edge.U as Vertex<Room>).Item;
            var endRoom = (edge.V as Vertex<Room>).Item;

            List<Vector2Int> optimalStartPoints = GetClosestEdgeOfRoom(startRoom, endRoom);

            var startPosf = optimalStartPoints[0];
            var endPosf = optimalStartPoints[1];
            var startPos = new Vector2Int((int)startPosf.x, (int)startPosf.y);
            var endPos = new Vector2Int((int)endPosf.x, (int)endPosf.y);

            List<Vector2Int> tempList = aStar.FindPath(startPos, endPos, (DungeonPathfinder2D.Node a, DungeonPathfinder2D.Node b) => {
                var pathCost = new DungeonPathfinder2D.PathCost();
                
                pathCost.cost = Vector2Int.Distance(b.Position, endPos);    //heuristic

                if (_grid[b.Position] == CellType.Room) {
                    pathCost.cost += 20;
                } else if (_grid[b.Position] == CellType.None) {
                    pathCost.cost += 50;
                } else if (_grid[b.Position] == CellType.Hallway) {
                    pathCost.cost += 1;
                }

                pathCost.traversable = true;

                return pathCost;
            });
            List<Vector2> startToEndPath = new List<Vector2>();
            foreach(Vector2Int vec in tempList){
                startToEndPath.Add(new Vector2(vec.x + 0.5f,vec.y + 0.5f));
            }
            List<Vector2> endToStartPath = new List<Vector2>();
            for(int i = startToEndPath.Count - 1; i >= 0; i--){
                endToStartPath.Add(startToEndPath[i]);
            }
            
            //Debug.Log(startRoom.identifier + "to" + endRoom.identifier);
            BetweenRoomPath path1 = new BetweenRoomPath(startToEndPath,startRoom,endRoom);
            path1._pathList.Insert(0,startRoom._bounds.center);
            path1._pathList.Add(endRoom._bounds.center);
            BetweenRoomPath path2 = new BetweenRoomPath(endToStartPath,endRoom,startRoom);
            path2._pathList.Insert(0,endRoom._bounds.center);
            path2._pathList.Add(startRoom._bounds.center);
            
            
            //startRoom.addPath(path1);
            //endRoom.addPath(path2);
            _betweenRoomPaths.Add(path1);
            _betweenRoomPaths.Add(path2);


            
        }



    }

    void SetTileWalls(){
        
        //add walls
        for(int i = 0; i < _size.x; i++){
            for(int j = 0; j < _size.y; j++){
                Tile foundTile = _tileGrid[i,j];
                if(foundTile != null){
                    //checkLeft
                    if(i > 0){
                        
                        Tile adjTile = _tileGrid[i-1,j];
                        if(adjTile == null || adjTile.GetCellType() != foundTile.GetCellType()){
                            foundTile.SetTexture(new Vector2Int(-1,0),Tile.TextureType.Wall);
                        }
                    }else{
                        foundTile.SetTexture(new Vector2Int(-1,0),Tile.TextureType.Wall);
                    }

                    //checkUp
                    if(j < _size.y-1){
                        
                        Tile adjTile = _tileGrid[i,j + 1];
                        if(adjTile == null || adjTile.GetCellType() != foundTile.GetCellType()){
                            foundTile.SetTexture(new Vector2Int(0,1),Tile.TextureType.Wall);
                        }
                    }else{
                        foundTile.SetTexture(new Vector2Int(0,1),Tile.TextureType.Wall);
                    }

                    //checkRight
                    if(i < _size.x-1){
                        
                        Tile adjTile = _tileGrid[i+1,j];
                        if(adjTile == null || adjTile.GetCellType() != foundTile.GetCellType()){
                            foundTile.SetTexture(new Vector2Int(1,0),Tile.TextureType.Wall);
                        }
                    }else{
                        foundTile.SetTexture(new Vector2Int(1,0),Tile.TextureType.Wall);
                    }

                    //checkDown
                    if(j > 0){
                        
                        Tile adjTile = _tileGrid[i,j-1];
                        if(adjTile == null || adjTile.GetCellType() != foundTile.GetCellType()){
                            foundTile.SetTexture(new Vector2Int(0,-1),Tile.TextureType.Wall);
                        }
                    }else{
                        foundTile.SetTexture(new Vector2Int(0,-1),Tile.TextureType.Wall);
                    }
                }
            }
        }

        //add doorways
        
        foreach(BetweenRoomPath path in _betweenRoomPaths){
            //uses the 2nd and 3rd points of the path because the first on is in the center of the room.
            Vector2 temp = path._pathList[1];
            Vector2Int startPoint = new Vector2Int((int)temp.x,(int)temp.y);
            Vector2 temp2 = path._pathList[2];
            Vector2Int secondPoint = new Vector2Int((int)temp2.x,(int)temp2.y);
            Tile startTile = _tileGrid[startPoint];
            Tile secondTile = _tileGrid[secondPoint];
            if(startTile == null){
                Debug.Log("startTile null");
            }
            if(secondTile == null){
                Debug.Log("secondTile null");
            }

            if(startPoint.x > secondPoint.x){
                startTile.SetTexture(new Vector2Int(-1,0),Tile.TextureType.Door);
                secondTile.SetTexture(new Vector2Int(1,0),Tile.TextureType.Door);
            }
            else if(startPoint.x < secondPoint.x){
                startTile.SetTexture(new Vector2Int(1,0),Tile.TextureType.Door);
                secondTile.SetTexture(new Vector2Int(-1,0),Tile.TextureType.Door);
            }
            else if(startPoint.y > secondPoint.y){
                startTile.SetTexture(new Vector2Int(0,-1),Tile.TextureType.Door);
                secondTile.SetTexture(new Vector2Int(0,1),Tile.TextureType.Door);
            }
            else if(startPoint.y < secondPoint.y){
                startTile.SetTexture(new Vector2Int(0,1),Tile.TextureType.Door);
                secondTile.SetTexture(new Vector2Int(0,-1),Tile.TextureType.Door);
            }
        }

        //call on tile script to spawn torches
        /*
        Tile.torch = torch;
        Tile.spawnTorches();
        Tile.decor = decor;
        Tile.spawnDecor();
        */
    }
    

    void PlaceCube(Vector2Int location, CellType type, Room room, float zPos = 0 ) {
        GameObject go;
        Tile newTile;
        if(type == CellType.Room){            
            go = Instantiate(_roomTile, new Vector3(location.x * _worldScale, zPos, location.y * _worldScale), Quaternion.identity);
            newTile = go.GetComponent<Tile>();
            
        }else{
            go = Instantiate(_hallwayTile, new Vector3(location.x * _worldScale, zPos, location.y * _worldScale), Quaternion.identity);
            newTile = go.GetComponent<Tile>();
        }
            go.transform.localScale *= _worldScale;
            _tileGrid[location] = newTile;
            newTile.SetCellType(type);
            newTile.SetTexture(new Vector2Int(0,0),Tile.TextureType.Floor);
            newTile.SetTexture(new Vector2Int(1,1),Tile.TextureType.Floor);
        
            _spawnedThings.Add(go);
        
    }

    void PlaceRoom(Vector2Int location, Vector2Int size, Room room) {
        //Debug.Log(room == null);
        for(int i = 0; i < size.x; i++){
            for(int j = 0; j < size.y; j++){
                PlaceCube(new Vector2Int(location.x+i,location.y+j), CellType.Room, room);
            }
        }
        
    }

    void PlaceHallway(Vector2Int location) {
        PlaceCube(location, CellType.Hallway, null);
    }

    void PlacePlayer(){
        LocomotionManager.Instance.Teleport(_spawnedThings[0].transform.position + new Vector3(0.5f,0.5f,0.5f));
    }

    void SetupEnemies(){
        _navMeshSetup.SetupNavMesh();
    }

    void OnDrawGizmos(){
        /*
        if(savedMst.Count > 0){
            foreach(Prim.Edge m in savedMst){
                Gizmos.color = Color.green;
                Vector3 pos1 = new Vector3((m.U as Vertex<Room>).Item.bounds.center.x,0,(m.U as Vertex<Room>).Item.bounds.center.y);
                Vector3 pos2 = new Vector3((m.V as Vertex<Room>).Item.bounds.center.x,0,(m.V as Vertex<Room>).Item.bounds.center.y);
                Gizmos.DrawLine(pos1,pos2);
            }
        }
        */
        /*
        if(betweenRoomPaths.Count > 0){
            foreach(BetweenRoomPath path in betweenRoomPaths){
                for(int i = 0; i < path.pathList.Count - 1; i ++){
                    Gizmos.color = Color.yellow;
                    Vector3 pos1 = new Vector3(path.pathList[i].x,2,path.pathList[i].y);
                    Vector3 pos2 = new Vector3(path.pathList[i+1].x,2,path.pathList[i+1].y);
                    Gizmos.DrawLine(pos1,pos2);
                }
            }
        }
        */
        
    }

    
}
