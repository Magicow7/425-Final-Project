using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = System.Random;
using Graphs;
using System;
using Locomotion;


public class Generator2D : MonoBehaviour {

    public static Generator2D instance;

    

    [SerializeField]
    Vector2Int size;
    [SerializeField]
    float worldScale;
    [SerializeField]
    private List<RoomPlaceParams> roomPlacePasses = new List<RoomPlaceParams>();
    /*
    [SerializeField]
    
    int roomCount;
    [SerializeField]
    Vector2Int roomMaxSize;
    [SerializeField]
    Vector2Int roomMinSize;
    */
    [SerializeField]
    GameObject hallwayTile;
    [SerializeField]
    GameObject roomTile;
    [SerializeField]
    GameObject player;

    [SerializeField]
    EnemySpawner enemySpawner;

    [SerializeField]
    NavMeshSetup navMeshSetup;

    [SerializeField]
    float nonMSTHallwayChance; // default was 1.25f

    //[SerializeField]
    //HeroSpawner HeroSpawner;

    Random random;
    Grid2D<CellType> grid;//grid used for making hallways
    Grid2D<Tile> tileGrid;//grid used for putting tiles on hallways
    Dictionary<string, Room> rooms;
    
    Delaunay2D delaunay;
    DijkstrasGraph<Room> dij;
    HashSet<Prim.Edge> selectedEdges;
    HashSet<Prim.Edge> savedMst = new HashSet<Prim.Edge>();
    public List<GameObject> spawnedThings;
    List<BetweenRoomPath> betweenRoomPaths = new List<BetweenRoomPath>();
    Room startRoom;
    Room endRoom;

    int failedRoomPlacements = 0;

    int generationIteration = 0;

    private int addedRooms = 0;

    [Serializable]
    struct RoomPlaceParams {
        public int roomCount;
        public Vector2Int roomMaxSize;
        public Vector2Int roomMinSize;
    }

    void Start() {
        Destroy(instance);
        instance = this;
        Generate();
    }

    void Regenerate(){
        foreach(GameObject obj in spawnedThings){
            Destroy(obj);
        }
        generationIteration += 1;
        Generate();
    }

    void Generate() {
        
        var currentGenerationIteration = generationIteration;
        random = new Random();//new Random(0) for set seed I think?
        grid = new Grid2D<CellType>(size, Vector2Int.zero);
        tileGrid = new Grid2D<Tile>(size, Vector2Int.zero);
        rooms = new Dictionary<string, Room>();
        dij = new DijkstrasGraph<Room>();
        betweenRoomPaths = new List<BetweenRoomPath>();
        spawnedThings = new List<GameObject>();
        startRoom = null;
        endRoom = null;
        Tile.clearActivatedWalls();

        if(currentGenerationIteration == generationIteration){
            for(int i = 0; i < roomPlacePasses.Count; i++){
                PlaceRooms(roomPlacePasses[i]);
            }
            
        }
        if(currentGenerationIteration == generationIteration){
            Triangulate();
        }
        if(currentGenerationIteration == generationIteration){
            CreateHallways();
        }
        if(currentGenerationIteration == generationIteration){
            PathfindHallways();
        }
        if(currentGenerationIteration == generationIteration){
            //FindEndRoom();
        }
        if(currentGenerationIteration == generationIteration){
            GeneratePathingInHallWays();
        }
        if(currentGenerationIteration == generationIteration){
            setTileWalls();
        }
        if(currentGenerationIteration == generationIteration){
            PlacePlayer();
        }
        if(currentGenerationIteration == generationIteration){
            SetupEnemies();
        }
        if(currentGenerationIteration == generationIteration){
            //Instantiate(HeroSpawner,transform.position,Quaternion.identity);
        }
        
        
   
    }

    //randomly places rooms around the grid with the given paramaters
    void PlaceRooms(RoomPlaceParams param) {
        
        for (int i = 0; i < param.roomCount; i++) {
            Vector2Int location = new Vector2Int(
                random.Next(0, size.x),
                random.Next(0, size.y)
            );

            Vector2Int roomSize = new Vector2Int(
                random.Next(param.roomMinSize.x, param.roomMaxSize.x + 1),
                random.Next(param.roomMinSize.y, param.roomMaxSize.y + 1)
            );

            bool add = true;
            Room newRoom = new Room(location, roomSize, (addedRooms + 1).ToString());
            Room buffer = new Room(location + new Vector2Int(-1, -1), roomSize + new Vector2Int(2, 2), "0");

            foreach (string room in rooms.Keys) {
                if (Room.Intersect(rooms[room], buffer)) {
                    add = false;
                    break;
                }
            }

            if (newRoom.bounds.xMin < 0 || newRoom.bounds.xMax >= size.x
                || newRoom.bounds.yMin < 0 || newRoom.bounds.yMax >= size.y) {
                add = false;
            }

            if (add) {
                addedRooms += 1;
                rooms.Add(newRoom.identifier, newRoom);
                //GameObject newBattle = Instantiate(battleHandler,new Vector3(newRoom.bounds.center.x,0,newRoom.bounds.center.y), Quaternion.identity);
                //newRoom.battleHandler = newBattle.GetComponent<BattleHandler>();
                //spawnedThings.Add(newBattle);
                int rand = random.Next(0,2);
                if(rand == 0){
                    //newBattle.transform.eulerAngles = new Vector3(0, 90, 0);
                }
                PlaceRoom(newRoom.bounds.position,newRoom.bounds.size,newRoom);
                    


                foreach (var pos in newRoom.bounds.allPositionsWithin) {
                    grid[pos] = CellType.Room;
                }
            }else{
                if(failedRoomPlacements > param.roomCount * 50){
                    failedRoomPlacements = 0;
                    Debug.Log("Regenerating dungeon");
                    Regenerate();
                    return;
                }else{
                    failedRoomPlacements ++;
                    i--;
                }
                
            }
        }
    }
    
    
    //triangulates the distances between rooms with delauney triangulation, which I don't really understand
    void Triangulate() {
        List<Vertex> vertices = new List<Vertex>();

        foreach (string room in rooms.Keys) {
            vertices.Add(new Vertex<Room>((Vector2)rooms[room].bounds.position + ((Vector2)rooms[room].bounds.size) / 2, rooms[room]));
        }

        delaunay = Delaunay2D.Triangulate(vertices);
    }

    //finds the minimum spanning tree and makes hallway lines between on the MST paths and a few random non MST paths based on random chance.
    void CreateHallways() {
        List<Prim.Edge> edges = new List<Prim.Edge>();

        foreach (var edge in delaunay.Edges) {
            edges.Add(new Prim.Edge(edge.U, edge.V));
        }

       
        List<Prim.Edge> mst = Prim.MinimumSpanningTree(edges, edges[0].U);    

        selectedEdges = new HashSet<Prim.Edge>(mst);
        var remainingEdges = new HashSet<Prim.Edge>(edges);
        remainingEdges.ExceptWith(selectedEdges);

        int remainingCount = (int)((float)remainingEdges.Count * nonMSTHallwayChance);
        Debug.Log(remainingCount + "/" + remainingEdges.Count + "extra paths added");
        for(int i = 0; i <  remainingCount; i++){
            List<Prim.Edge> setToList = new List<Prim.Edge>();
            foreach (var edge in remainingEdges) {
                setToList.Add(edge);
            }
            Prim.Edge element = setToList[random.Next(setToList.Count)];
            selectedEdges.Add(element);
            remainingEdges.Remove(element);

        }

        savedMst = selectedEdges;

        
        /*
        foreach (var edge in remainingEdges) {
            if (random.NextDouble() < nonMSTHallwayChance) {
                selectedEdges.Add(edge);
            }
        }
        */
    }

    //find closest bit of each room to the centerpoint of the opposite room.
    List<Vector2Int> getClosestEdgeOfRoom(Room room1, Room room2){
        List<Vector2Int> returnList = new List<Vector2Int>();
        Vector2Int room1Point = Vector2Int.RoundToInt(room1.bounds.center);
        Vector2Int room2Point = Vector2Int.RoundToInt(room2.bounds.center);
        while(room1Point.x != room2Point.x){
            if(room1Point.x < room2Point.x){
                room1Point.x++;
                if(!Room.withinRange(room1Point,room1)){
                    room1Point.x--;
                    break;
                }
            }else{
                room1Point.x--;
                if(!Room.withinRange(room1Point,room1)){
                    room1Point.x++;
                    break;
                }
            }
        }
        while(room1Point.y != room2Point.y){
            if(room1Point.y < room2Point.y){
                room1Point.y++;
                if(!Room.withinRange(room1Point,room1)){
                    room1Point.y--;
                    break;
                }
            }else{
                room1Point.y--;
                if(!Room.withinRange(room1Point,room1)){
                    room1Point.y++;
                    break;
                }
            }
        }
        while(room2Point.x != room1Point.x){
            if(room2Point.x < room1Point.x){
                room2Point.x++;
                if(!Room.withinRange(room2Point,room2)){
                    room2Point.x--;
                    break;
                }
            }else{
                room2Point.x--;
                if(!Room.withinRange(room2Point,room2)){
                    room2Point.x++;
                    break;
                }
            }
        }
        while(room2Point.y != room1Point.y){
            if(room2Point.y < room1Point.y){
                room2Point.y++;
                if(!Room.withinRange(room2Point,room2)){
                    room2Point.y--;
                    break;
                }
            }else{
                room2Point.y--;
                if(!Room.withinRange(room2Point,room2)){
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
        DungeonPathfinder2D aStar = new DungeonPathfinder2D(size);

        foreach (var edge in selectedEdges) {
            var startRoom = (edge.U as Vertex<Room>).Item;
            var endRoom = (edge.V as Vertex<Room>).Item;

            List<Vector2Int> optimalStartPoints = getClosestEdgeOfRoom(startRoom, endRoom);

            var startPosf = optimalStartPoints[0];
            var endPosf = optimalStartPoints[1];
            var startPos = new Vector2Int((int)startPosf.x, (int)startPosf.y);
            var endPos = new Vector2Int((int)endPosf.x, (int)endPosf.y);

            var path = aStar.FindPath(startPos, endPos, (DungeonPathfinder2D.Node a, DungeonPathfinder2D.Node b) => {
                var pathCost = new DungeonPathfinder2D.PathCost();
                
                pathCost.cost = Vector2Int.Distance(b.Position, endPos);    //heuristic

                if (grid[b.Position] == CellType.Room) {
                    pathCost.cost += 50;
                } else if (grid[b.Position] == CellType.None) {
                    pathCost.cost += 5;
                } else if (grid[b.Position] == CellType.Hallway) {
                    pathCost.cost += 1;
                }

                pathCost.traversable = true;

                return pathCost;
            });

            if (path != null) {
                for (int i = 0; i < path.Count; i++) {
                    var current = path[i];

                    if (grid[current] == CellType.None) {
                        grid[current] = CellType.Hallway;
                    }

                    if (i > 0) {
                        var prev = path[i - 1];

                        var delta = current - prev;
                    }
                }

                foreach (var pos in path) {
                    if (grid[pos] == CellType.Hallway) {
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
        

        DungeonPathfinder2D aStar = new DungeonPathfinder2D(size);

        foreach (var edge in selectedEdges) {
            var startRoom = (edge.U as Vertex<Room>).Item;
            var endRoom = (edge.V as Vertex<Room>).Item;

            List<Vector2Int> optimalStartPoints = getClosestEdgeOfRoom(startRoom, endRoom);

            var startPosf = optimalStartPoints[0];
            var endPosf = optimalStartPoints[1];
            var startPos = new Vector2Int((int)startPosf.x, (int)startPosf.y);
            var endPos = new Vector2Int((int)endPosf.x, (int)endPosf.y);

            List<Vector2Int> tempList = aStar.FindPath(startPos, endPos, (DungeonPathfinder2D.Node a, DungeonPathfinder2D.Node b) => {
                var pathCost = new DungeonPathfinder2D.PathCost();
                
                pathCost.cost = Vector2Int.Distance(b.Position, endPos);    //heuristic

                if (grid[b.Position] == CellType.Room) {
                    pathCost.cost += 20;
                } else if (grid[b.Position] == CellType.None) {
                    pathCost.cost += 50;
                } else if (grid[b.Position] == CellType.Hallway) {
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
            path1.pathList.Insert(0,startRoom.bounds.center);
            path1.pathList.Add(endRoom.bounds.center);
            BetweenRoomPath path2 = new BetweenRoomPath(endToStartPath,endRoom,startRoom);
            path2.pathList.Insert(0,endRoom.bounds.center);
            path2.pathList.Add(startRoom.bounds.center);
            
            
            //startRoom.addPath(path1);
            //endRoom.addPath(path2);
            betweenRoomPaths.Add(path1);
            betweenRoomPaths.Add(path2);


            
        }



    }

    void setTileWalls(){
        
        //add walls
        for(int i = 0; i < size.x; i++){
            for(int j = 0; j < size.y; j++){
                Tile foundTile = tileGrid[i,j];
                if(foundTile != null){
                    //checkLeft
                    if(i > 0){
                        
                        Tile AdjTile = tileGrid[i-1,j];
                        if(AdjTile == null || AdjTile.getCellType() != foundTile.getCellType()){
                            foundTile.setTexture(new Vector2Int(-1,0),Tile.TextureType.Wall);
                        }
                    }else{
                        foundTile.setTexture(new Vector2Int(-1,0),Tile.TextureType.Wall);
                    }

                    //checkUp
                    if(j < size.y-1){
                        
                        Tile AdjTile = tileGrid[i,j + 1];
                        if(AdjTile == null || AdjTile.getCellType() != foundTile.getCellType()){
                            foundTile.setTexture(new Vector2Int(0,1),Tile.TextureType.Wall);
                        }
                    }else{
                        foundTile.setTexture(new Vector2Int(0,1),Tile.TextureType.Wall);
                    }

                    //checkRight
                    if(i < size.x-1){
                        
                        Tile AdjTile = tileGrid[i+1,j];
                        if(AdjTile == null || AdjTile.getCellType() != foundTile.getCellType()){
                            foundTile.setTexture(new Vector2Int(1,0),Tile.TextureType.Wall);
                        }
                    }else{
                        foundTile.setTexture(new Vector2Int(1,0),Tile.TextureType.Wall);
                    }

                    //checkDown
                    if(j > 0){
                        
                        Tile AdjTile = tileGrid[i,j-1];
                        if(AdjTile == null || AdjTile.getCellType() != foundTile.getCellType()){
                            foundTile.setTexture(new Vector2Int(0,-1),Tile.TextureType.Wall);
                        }
                    }else{
                        foundTile.setTexture(new Vector2Int(0,-1),Tile.TextureType.Wall);
                    }
                }
            }
        }

        //add doorways
        
        foreach(BetweenRoomPath path in betweenRoomPaths){
            //uses the 2nd and 3rd points of the path because the first on is in the center of the room.
            Vector2 temp = path.pathList[1];
            Vector2Int startPoint = new Vector2Int((int)temp.x,(int)temp.y);
            Vector2 temp2 = path.pathList[2];
            Vector2Int secondPoint = new Vector2Int((int)temp2.x,(int)temp2.y);
            Tile startTile = tileGrid[startPoint];
            Tile secondTile = tileGrid[secondPoint];
            if(startTile == null){
                Debug.Log("startTile null");
            }
            if(secondTile == null){
                Debug.Log("secondTile null");
            }

            if(startPoint.x > secondPoint.x){
                startTile.setTexture(new Vector2Int(-1,0),Tile.TextureType.Door);
                secondTile.setTexture(new Vector2Int(1,0),Tile.TextureType.Door);
            }
            else if(startPoint.x < secondPoint.x){
                startTile.setTexture(new Vector2Int(1,0),Tile.TextureType.Door);
                secondTile.setTexture(new Vector2Int(-1,0),Tile.TextureType.Door);
            }
            else if(startPoint.y > secondPoint.y){
                startTile.setTexture(new Vector2Int(0,-1),Tile.TextureType.Door);
                secondTile.setTexture(new Vector2Int(0,1),Tile.TextureType.Door);
            }
            else if(startPoint.y < secondPoint.y){
                startTile.setTexture(new Vector2Int(0,1),Tile.TextureType.Door);
                secondTile.setTexture(new Vector2Int(0,-1),Tile.TextureType.Door);
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
            go = Instantiate(roomTile, new Vector3(location.x * worldScale, zPos, location.y * worldScale), Quaternion.identity);
            newTile = go.GetComponent<Tile>();
            
        }else{
            go = Instantiate(hallwayTile, new Vector3(location.x * worldScale, zPos, location.y * worldScale), Quaternion.identity);
            newTile = go.GetComponent<Tile>();
        }
            go.transform.localScale *= worldScale;
            tileGrid[location] = newTile;
            newTile.setCellType(type);
            newTile.setTexture(new Vector2Int(0,0),Tile.TextureType.Floor);
            newTile.setTexture(new Vector2Int(1,1),Tile.TextureType.Floor);
        
            spawnedThings.Add(go);
        
    }

    void PlaceRoom(Vector2Int location, Vector2Int Size, Room room) {
        //Debug.Log(room == null);
        for(int i = 0; i < Size.x; i++){
            for(int j = 0; j < Size.y; j++){
                PlaceCube(new Vector2Int(location.x+i,location.y+j), CellType.Room, room);
            }
        }
        
    }

    void PlaceHallway(Vector2Int location) {
        PlaceCube(location, CellType.Hallway, null);
    }

    void PlacePlayer(){
        LocomotionManager.Instance.Teleport(spawnedThings[0].transform.position + new Vector3(0.5f,0.5f,0.5f));
    }

    void SetupEnemies(){
        navMeshSetup.SetupNavMesh();
        enemySpawner.SpawnEnemies(spawnedThings, new Vector3(0.5f,0.5f,0.5f));
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
