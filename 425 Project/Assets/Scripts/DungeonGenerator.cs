using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Random = System.Random;
using Locomotion;
using Unity.AI.Navigation;


public class DungeonGenerator : MonoBehaviour
{
    public static List<Vector3> possibleSpawnPoints;

    public static DungeonGenerator instance;

    //parameter structs   
    [Serializable]
    struct DungeonRoomParameters{
        //weight in spawn room function
        public float spawnWeight;
        //room prefab to spawn
        public DungeonRoomObject roomObject;
        //does the room have doors on different levels
        public bool verticalRoom;
        //bounding box parameters
        public Vector3 boundingBoxMinPoint;
        public Vector3 boundingBoxMaxPoint;
        //centerpoint that bounding boxes branch from, usually at the center of the floor of the room
        public Vector3 centerPoint;
        //should be 0 by default, used when trying to spawn rooms.
        public Vector3 rotation;
        //list of door parameters for doors on this room
        public List<DungeonDoorParameters> relativeDoorParameters;
    }

    [Serializable]
    struct DungeonDoorParameters{
        public DungeonDoorParameters(Vector3 relativePosition, Vector3 angle){
            this.relativePosition = relativePosition;
            this.angle = angle;
        }
        //angle of the door in relation to the room's zForward. forward is 0, right is 90, back is 180, left is 270
        public Vector3 angle;
        //relative position of door in relation to the centerpoint of the room., should be at floor level of the room
        public Vector3 relativePosition;
    }

    static int dungeonRoomCurrID = 0;
    //connector structs with data for a graph
    class DungeonRoom{
        public readonly int roomId;
        public List<GameObject> spawnedPrefabs = new List<GameObject>();
        //room position in world space
        public Vector3 worldPosition;
        //room rotation in worldspace
        public Quaternion worldRotation;
        //rotated bounding box of room, used to check against new rooms to see if they overlap
        public OBB boundingBox;
        //base parameters of room
        public DungeonRoomParameters parameters;
        //list of attatched doorways
        public List<DungeonDoorway> doorways;
        public int graphDistanceFromCenter;
        public DungeonRoom(DungeonRoomParameters parameters, Vector3 worldPosition, Quaternion worldRotation, OBB boundingBox, int graphDistanceFromCenter){
            doorways = new List<DungeonDoorway>();
            this.parameters = parameters;
            this.worldPosition = worldPosition;
            this.worldRotation = worldRotation;
            this.boundingBox = boundingBox;
            this.graphDistanceFromCenter = graphDistanceFromCenter;
            roomId = dungeonRoomCurrID;
            dungeonRoomCurrID++;
        }
        public void RemoveRoomFromConnections(DungeonRoom room){
            foreach(DungeonDoorway doorway in doorways){
                if(doorway.connectingRooms.Contains(room)){
                    doorway.connectingRooms.Remove(room);
                }
            }
        }

    }

    class DungeonDoorway{
        //doorway position in worldspace
        public Vector3 worldPosition;
        //parent room of door
        public DungeonRoom parentRoom;
        //list of rooms this door connects, should be max 2
        public List<DungeonRoom> connectingRooms;
        //base door parameters
        public DungeonDoorParameters parameters;
        public GameObject spawnedBlocker;
        public GameObject spawnedOpenDoorway;
        public NavMeshLink spawnedNavMeshLink;
        
        //can we branch off this room, will be set to false if the generator deems no room can be spawned here.
        public bool branchable;
        public DungeonDoorway(DungeonDoorParameters parameters, Vector3 worldPosition, DungeonRoom parentRoom){
            this.parentRoom = parentRoom;
            this.parameters = parameters;
            this.worldPosition = worldPosition;
            connectingRooms = new List<DungeonRoom>();
            connectingRooms.Add(parentRoom);
            branchable = true;
        }

        
    }

    //struct for non-axis aligned bounding boxes
    struct OBB{
        public Vector3 Center;      // Center of the OBB
        public Vector3[] Axes;      // Local X, Y, and Z axes (3 unit vectors)
        public Vector3 HalfExtents; // Half-width, half-height, and half-depth along each axis

        public OBB(DungeonRoomParameters roomParams, Vector3 worldPosition, Quaternion worldRotation){
            Vector3 localCenter = Vector3.Lerp(roomParams.boundingBoxMaxPoint,roomParams.boundingBoxMinPoint,0.5f);
            this.Center = worldPosition + localCenter;
            this.Axes = new Vector3[3];

            Axes[0] = worldRotation * new Vector3(1,0,0);
            Axes[1] = worldRotation * new Vector3(0,1,0);
            Axes[2] = worldRotation * new Vector3(0,0,1);

            float halfWidth = Mathf.Abs(roomParams.boundingBoxMaxPoint.x - roomParams.boundingBoxMinPoint.x)/2f;
            float halfHeight = Mathf.Abs(roomParams.boundingBoxMaxPoint.y - roomParams.boundingBoxMinPoint.y)/2f;
            float halfDepth = Mathf.Abs(roomParams.boundingBoxMaxPoint.z - roomParams.boundingBoxMinPoint.z)/2f;
            this.HalfExtents = new Vector3(halfWidth, halfHeight, halfDepth);
        }
    }

    public enum GenerationMode{
        StaticGeneration,
        DynamicGeneration
    }

    //random
    Random random;
    //generation mode
    [SerializeField]
    private GenerationMode generationMode = GenerationMode.DynamicGeneration;
    //number of rooms to spawn
    [Header("Settings for Static Generation")]
    [SerializeField]
    private int roomsToSpawn = 5;
    //if bounding boxes are farther away from eachother than this value, they will not test for collision
    [SerializeField]
    private int boundingBoxCheckPruneDistance = 50;
    //quota of vertical rooms to spawn for each level
    [SerializeField]
    private float verticalRoomsPerLevelQuota = 5;
    private float verticalRoomsOnCurrentLevel = 0;
    //the maximum height a bounding box can reach currently. goes down as level increses
    [SerializeField]
    private float maxPossibleDungeonRoomHeight = 5;
    // if a door is below this y value, it cannot be branched from. this value is decremented when level increases. this is done to stop generation on lower levels before higher levels are finished
    [SerializeField]
    private float availableDoorheight = 0;
    
    [SerializeField]
    private bool forceLevels = false;
    //y height of each level
    [SerializeField]
    private float levelHeight = 15;

    //once this amount of rooms is added, level is incremented
    [SerializeField]
    private int roomsToAddBeforeBlacklistingLevel = 100;

    private int roomsAddedInLevel = 0;

    //current level value
    [SerializeField]
    private int currlevel = 1;

    [Header("Settings for Dynamic Generation")]
    //the limit of how far a room can be from the center of the graph without being removed
    [SerializeField]
    private int maxGraphDistanceFromCenter;
    public int getMaxGraphDistanceFromCenter()
    {
        return maxGraphDistanceFromCenter;
    }

    [SerializeField]
    private float precentageOfDoorsToBeUsable;
    [SerializeField]
    public Transform player;
    [SerializeField]
    private GameObject navMeshLinkHolder;
    [SerializeField]
    private float doorWidth;

    [Header("Settings for Enemies")]
    [SerializeField]
    EnemySpawner enemySpawner;
    

    [Header("Settings for Debugging")]
    // visualize bounding boxes for debuging
    [SerializeField]
    private bool visualizeBoundingBoxes = false;

    //debug object
    [SerializeField]
    GameObject testObject;

    [Header("General Settings")]
    //choose to randomize seed
    [SerializeField]
    private bool randomSeed = false;
    //seed value, can set if random seed is false
    [SerializeField]
    private int seed = 0;
    
    //parameters of the start room
    [SerializeField]
    private DungeonRoomParameters startRoom;

    //this object gets spawned at doorways that connect to nothing after generation.
    [SerializeField]
    private GameObject doorwayBlocker;

    //this is for open doors
    [SerializeField]
    private GameObject doorwayOpen;

    //list of possible room parameters that can be added
    [SerializeField]
    private List<DungeonRoomParameters> possibleRoomParameters = new List<DungeonRoomParameters>();

    //private vars 
    private List<DungeonRoom> rooms = new List<DungeonRoom>();
    private List<DungeonDoorway> doorways = new List<DungeonDoorway>();
    //private List<GameObject> spawnedPrefabs = new List<GameObject>();
    //current room player is in
    private DungeonRoom currentCenterRoom;
    public Vector3 getCurrentCenterRoomWorldPosition()
    {
        return this.currentCenterRoom.worldPosition;
    }

    void Start()
    {
        //set singleton instance
        instance = this;
        //randomize seed if needed
        if(randomSeed){
            random = new Random();
            seed = random.Next(0,Int32.MaxValue);
        }
        Debug.Log("SEED IS:" + seed);
        random = new Random(seed);
        //spawn start room with 
        AddRoomToDungeon();
        //set center room to start room
        currentCenterRoom = rooms[0];
        if(generationMode == GenerationMode.StaticGeneration){
            GenerateStaticDungeon();
        }else if(generationMode == GenerationMode.DynamicGeneration){
            GenerateDynamicDungeon(rooms[0]);
        }
        PlacePlayer();
        SetupEnemies();
        
    }

    void Update(){
        if(generationMode == GenerationMode.StaticGeneration){
            return;
        }
        //check if player is in current room bounds
        bool playerInCurrRoomBounds = IsPointInsideOBB(player.position, currentCenterRoom.boundingBox);
        //if not in bounds, check if in neighbor bounds, if so, update center room to neighbor, if not, player out of bounds
        if(!playerInCurrRoomBounds){
            DungeonRoom newCenterRoom = currentCenterRoom;
            bool newCenterFound = false;
            List<DungeonRoom> neighbors = GetDungeonRoomNeighbors(currentCenterRoom);
            foreach(DungeonRoom neighbor in neighbors){
                if(!newCenterFound && IsPointInsideOBB(player.position, neighbor.boundingBox)){
                    newCenterRoom = neighbor;
                    newCenterFound = true;
                }
            }
            currentCenterRoom = newCenterRoom;
            if(!newCenterFound){
                Debug.Log("DYNAMIC WORLD GENERATION LOST PLAYER, LIKELY OUT OF BOUNDS");
            }else{
                Debug.Log("NEW CENTER ROOM SET, GENERATING DYNAMIC DUNGEON");
                GenerateDynamicDungeon(currentCenterRoom);
            }
        }
    }

    private void GenerateDynamicDungeon(DungeonRoom newCenterRoom){
        //set new centerRoom and update distances
        UpdateNeighborCenterDistances(newCenterRoom, newCenterRoom);
        //destroy old rooms out of range
        List<DungeonRoom> destroyQueue = new List<DungeonRoom>();
        foreach(DungeonRoom room in rooms){
            if(room.graphDistanceFromCenter > maxGraphDistanceFromCenter){
                destroyQueue.Add(room);
            }
        }
        DestroyRooms(destroyQueue);

        //add new rooms: fo all rooms with distance to center less than range, add rooms
        //function to pass to dungeon addition to check if branch doors are in bounds and branchable
        Func<DungeonDoorway, bool> doorwayBoundFunc = door => door.branchable && door.parentRoom.graphDistanceFromCenter < maxGraphDistanceFromCenter ? true : false;
        bool addedRoomToDungeon = true;
        //Add rooms to dungeon until failure
        while(addedRoomToDungeon){
            addedRoomToDungeon = AddRoomToDungeon(doorwayBoundFunc);
        }
        //UpdateNeighborCenterDistances(newCenterRoom, newCenterRoom);
        SpawnRoomObjects();
        
        //update possible spawn positions
        List<Vector3> temp = new List<Vector3>();
        foreach(DungeonRoom room in rooms){
            if(room.graphDistanceFromCenter == maxGraphDistanceFromCenter){
                temp.Add(room.worldPosition + new Vector3(0,1,0));
            }
        }
        possibleSpawnPoints = temp;
    }

    //function to destroy a list of rooms, removing it from the graph and deleting their gameobjects
    private void DestroyRooms(List<DungeonRoom> roomsToDestroy){
        
        foreach(DungeonRoom room in roomsToDestroy){
            //remove prefabs
            foreach(GameObject prefab in room.spawnedPrefabs){
                Destroy(prefab.gameObject);
            }
            //cleanse from graph
            List<DungeonRoom> neighbors = GetDungeonRoomNeighbors(room);
            foreach(DungeonRoom neighbor in neighbors){
                neighbor.RemoveRoomFromConnections(room);
            }

            foreach(DungeonDoorway doorway in room.doorways){
                if(doorway.spawnedNavMeshLink != null){
                    Destroy(doorway.spawnedNavMeshLink);
                }
                
                doorways.Remove(doorway);
            }
            rooms.Remove(room);
        }
        
    }

    //function to generate full dungeon given parameters in inspector
    private void GenerateStaticDungeon(){
        while(roomsToSpawn > 0){
            //if room adition fails, debug and stop
            if(AddRoomToDungeon() == false){
                Debug.Log("FAILED TO ADD ROOM");
                roomsToSpawn = 0;
            //increment values if room added
            }else{
                Debug.Log("SUCESSFULLY ADDED ROOM");
                roomsAddedInLevel++;
                if(roomsAddedInLevel > roomsToAddBeforeBlacklistingLevel){
                    roomsAddedInLevel -= roomsToAddBeforeBlacklistingLevel;
                    availableDoorheight -= levelHeight;
                    maxPossibleDungeonRoomHeight -= levelHeight;
                    currlevel++;
                    verticalRoomsOnCurrentLevel = 0;

                }
            }
            roomsToSpawn--;
        }
        SpawnRoomObjects();
        
    }

    private void SpawnRoomObjects(){
        //spawn in room objects after graph is completed
        foreach(DungeonRoom room in rooms){
            if(room.spawnedPrefabs.Count > 0){
                //room object already exists
            }else{
                //visualize bounding boxes if specified
                if(visualizeBoundingBoxes){
                    GameObject boundingBoxVisualization = Instantiate(testObject, room.worldPosition, room.worldRotation);
                    room.spawnedPrefabs.Add(boundingBoxVisualization.gameObject);
                    Vector3 pointA = room.parameters.boundingBoxMaxPoint + room.worldPosition;
                    Vector3 pointB = room.parameters.boundingBoxMinPoint + room.worldPosition;
                    Vector3 size = new Vector3(Mathf.Abs(pointA.x - pointB.x),
                                        Mathf.Abs(pointA.y - pointB.y),
                                        Mathf.Abs(pointA.z - pointB.z));
                    Vector3 center = room.worldPosition + new Vector3(0,(room.parameters.boundingBoxMaxPoint.y - room.parameters.boundingBoxMinPoint.y)/2,0);;
                    boundingBoxVisualization.transform.position = center;
                    boundingBoxVisualization.transform.localScale = size;
                }
                //spawn in room object and block unsued doors
                DungeonRoomObject roomObject = Instantiate(room.parameters.roomObject, room.worldPosition, room.worldRotation);
                room.spawnedPrefabs.Add(roomObject.gameObject);
                
                
            }
            //update door blockers and navmesh links

            //this should probably be rewritten, but it will work for now.
            foreach(DungeonDoorway door in room.doorways){
                if(door.connectingRooms.Count == 2){
                    //open door
                    if(door.spawnedBlocker != null){
                        Destroy(door.spawnedBlocker);
                    }
                    if(door.spawnedOpenDoorway == null){
                        //block off door if no blocker already
                        //bandaid fix, add 1 to y
                        GameObject openDoor = Instantiate(doorwayOpen, door.worldPosition + new Vector3(0,1,0), room.worldRotation * Quaternion.Euler(door.parameters.angle));
                        door.parentRoom.spawnedPrefabs.Add(openDoor);
                        door.spawnedOpenDoorway = openDoor;
                    }
                    //create navMeshLink if null
                    if(door.spawnedNavMeshLink == null){
                        
                        NavMeshLink link = navMeshLinkHolder.AddComponent<NavMeshLink>();
                        Vector3 posDistanceFromDoor = GetOutVectorFromAngle(door.parentRoom.worldRotation.eulerAngles + door.parameters.angle);
                        link.startPoint = door.worldPosition - posDistanceFromDoor;
                        link.endPoint = door.worldPosition + posDistanceFromDoor;
                        link.width = doorWidth;
                        link.enabled = true;
                        door.spawnedNavMeshLink = link;
                        
                    }
                }else {
                    if(door.spawnedNavMeshLink != null){
                        Destroy(door.spawnedNavMeshLink);
                    }
                    if(door.spawnedOpenDoorway != null){
                        Destroy(door.spawnedOpenDoorway);
                    }
                    if(door.spawnedBlocker == null){
                        //block off door if no blocker already
                        //add 180 because the door was backwards, bandaid fix
                        //another bandaid fix, add 1 to y
                        GameObject blocker = Instantiate(doorwayBlocker, door.worldPosition + new Vector3(0,1,0), room.worldRotation * Quaternion.Euler(door.parameters.angle + new Vector3(0,180,0)));
                        door.parentRoom.spawnedPrefabs.Add(blocker);
                        door.spawnedBlocker = blocker;
                    }
                }
                
            }

            
            
        }
    }

    //if start room, add start room,
    //else loop through all possible doorways to add onto, try all possible room parameters and all their doorways, add first one that works.
    //branch door func is used to check the
    private bool AddRoomToDungeon(){
        return AddRoomToDungeon(door => door.branchable);
    }
    private bool AddRoomToDungeon(Func<DungeonDoorway,bool> branchDoorFunc){
        bool successfullyAddedRoom = false;
        //first room
        if(rooms.Count == 0){
            //initialize start room
            DungeonRoom newRoom = InitializeStartRoom(startRoom,Vector3.zero);
            //add to roomlist
            rooms.Add(newRoom);
            //add doorways to doorwaylist
            for(int j = 0; j < newRoom.doorways.Count; j++){
                doorways.Add(newRoom.doorways[j]);
            }

            successfullyAddedRoom = true;
        }else{
            //get list of unused doors
            List<DungeonDoorway> possibleDoorsToAddFrom = GetUnusedDoors(branchDoorFunc);
            //if the door is unbranchable, its weight will be 0 in the weight function
            Func<DungeonDoorway, float> doorwayWeightFunc = door => door.branchable ? 1 : 0;
            //get queue shuffled based on weight
            Queue<DungeonDoorway> orderedDoorsToTry = WeightedOrderShuffle(possibleDoorsToAddFrom, doorwayWeightFunc);
            //loop throught all doors to try to add onto
            while(orderedDoorsToTry.Count > 0 && !successfullyAddedRoom){
                //dequeue first door in list
                DungeonDoorway choosenDoorway = orderedDoorsToTry.Dequeue();
                //set weight function for possible rooms to add, if vertical room quota is reached, vertical room wieght will be 0.
                //if we are reaching the end of the level, and don't have enough vertical rooms, force vertical rooms into list.
                Func<DungeonRoomParameters, float> roomParamWeightFunc = param => param.spawnWeight;
                if(forceLevels){
                    roomParamWeightFunc = param => (verticalRoomsOnCurrentLevel >= verticalRoomsPerLevelQuota && param.verticalRoom) ? 0 : param.spawnWeight;
                    if(verticalRoomsOnCurrentLevel < verticalRoomsPerLevelQuota && roomsToAddBeforeBlacklistingLevel - roomsAddedInLevel <= verticalRoomsPerLevelQuota - verticalRoomsOnCurrentLevel){
                        roomParamWeightFunc = param => param.verticalRoom ? 1 : 0;
                    }
                }
                
                //reset to default if dungeon verticality doesn't matter
                
                //get queue shuffled based on weight
                Queue<DungeonRoomParameters> orderedTemplatesToTry = WeightedOrderShuffle(new List<DungeonRoomParameters>(possibleRoomParameters), roomParamWeightFunc);
                //loop through all templates to try.
                while(orderedTemplatesToTry.Count > 0 && !successfullyAddedRoom){
                    //dequeue top of list
                    DungeonRoomParameters choosenParams = orderedTemplatesToTry.Dequeue();
                    // get list of door indicies to try to add at on room template
                    List<int>doorIndiciesToTry = new List<int>();
                    for(int i = 0; i < choosenParams.relativeDoorParameters.Count; i++){
                        doorIndiciesToTry.Add(i);
                    }
                    //weight all doors equally
                    Func<int, float> indexWeightFunc = index => 1;
                    //get queue shuffled based on weight
                    Queue<int> orderedIndiciesToTry = WeightedOrderShuffle(doorIndiciesToTry, indexWeightFunc);
                    //loop through indicies
                    while(orderedIndiciesToTry.Count > 0 && !successfullyAddedRoom){
                        //dequeue first index in list
                        int choosenIndex = orderedIndiciesToTry.Dequeue();
                        //attempt to add the room given the room we are trying to add onto, the room params we are trying,
                        // and the choosen door index in the room params we are attempting to connect with
                        var result = AttemptAddRoom(choosenParams, choosenDoorway,choosenIndex);
                        successfullyAddedRoom = result.successful;
                        DungeonRoom newRoom = result.dungeonRoom;
                        //if the room can be added here successfully
                        if(successfullyAddedRoom){
                            Debug.Log("FOUND SUCCESSFUL ROOM ADDING NOW");
                            //add newroom to prev doorway connections
                            choosenDoorway.connectingRooms.Add(newRoom);
                            //connect choosenIndex door to prevRoom
                            newRoom.doorways[choosenIndex].connectingRooms.Add(choosenDoorway.parentRoom);
                            //add new room to roomlist
                            rooms.Add(newRoom);
                            //add to doorway list
                            for(int j = 0; j < newRoom.doorways.Count; j++){
                                doorways.Add(newRoom.doorways[j]);
                            }

                            //check if vertical room and increment if so
                            if(newRoom.parameters.verticalRoom && forceLevels){
                                verticalRoomsOnCurrentLevel++;
                                Debug.Log("current vert rooms on level is " + verticalRoomsOnCurrentLevel);
                            }
                        }else{
                            //if not continue looping
                            //Debug.Log("INVALID ROOM");
                        }
                    }
                }

                //if nothing worked at this door, remove it from later searches
                if(!successfullyAddedRoom){
                    //Debug.Log("unbranchable!");
                    choosenDoorway.branchable = false;
                }
                
            }
            
        }
        //return true if a room was added successfully, return false if not
        return successfullyAddedRoom;
        
    }

    //normal rooms need to know their paramters, what doorway they are branching off of, and which of their doors connects to it
    //the start room only needs to know its parameters and a start position
    //version for start room
    private DungeonRoom InitializeStartRoom(DungeonRoomParameters dungeonRoomParameters, Vector3 startPosition){
        DungeonRoom newRoom;
        //initialize start room for graph
        newRoom = new DungeonRoom(dungeonRoomParameters, startPosition, Quaternion.identity, new OBB(dungeonRoomParameters,startPosition,Quaternion.identity), 0);
        for(int i = 0; i < dungeonRoomParameters.relativeDoorParameters.Count; i++){
            DungeonDoorway newDoorway = new DungeonDoorway(dungeonRoomParameters.relativeDoorParameters[i], newRoom.worldPosition + dungeonRoomParameters.relativeDoorParameters[i].relativePosition, newRoom);
            newRoom.doorways.Add(newDoorway);
        }

        return newRoom;
    }

    //version for all other rooms
    private (DungeonRoom dungeonRoom, bool successful) AttemptAddRoom(DungeonRoomParameters dungeonRoomParameters, DungeonDoorway branchFromDoor, int connectingDoorIndex){
        bool successfullyAddedRoom = true;
        DungeonRoom newRoom;
        //get the angle of the door in world space, stored in the y value
        Vector3 fromDoorOutVector = GetOutVectorFromAngle(branchFromDoor.parentRoom.worldRotation.eulerAngles + branchFromDoor.parameters.angle);
       //rotate the room parameters to be in line with the door, return the modified parameters and the rotation applied
        var result = ModifyRoomParamsToFitDoor(dungeonRoomParameters, fromDoorOutVector, branchFromDoor.parentRoom.worldRotation,connectingDoorIndex);
        DungeonRoomParameters modifiedParameters = result.returnedParams;
        Quaternion appliedRotation = result.returnedQuaternion;
        //get the distance of the new room parameters center to it door we are trying to add at.
        float centerDistanceFromDoor = Vector3.Distance(modifiedParameters.centerPoint, modifiedParameters.relativeDoorParameters[connectingDoorIndex].relativePosition);
        // set center position of new room as world position of the branch door + the difference 
        //between modified parameters choosen door's relative position in local space and the modified parameter's center in local space
        Vector3 newPosition = branchFromDoor.worldPosition;
        var startPosition = newPosition + modifiedParameters.relativeDoorParameters[connectingDoorIndex].relativePosition;
        var targetPosition = branchFromDoor.worldPosition;
        newPosition += (targetPosition - startPosition);

        //create new Dungeon Room object
        newRoom = new DungeonRoom(modifiedParameters, newPosition, appliedRotation, new OBB(modifiedParameters,newPosition, appliedRotation), branchFromDoor.parentRoom.graphDistanceFromCenter + 1);
        //if the bounding box of the new room is above the maxPossibleDungeonRoomheight, fail room add attempt
        if(forceLevels && newRoom.parameters.boundingBoxMaxPoint.y + newPosition.y > maxPossibleDungeonRoomHeight){
            successfullyAddedRoom = false;
            return (newRoom, successfullyAddedRoom);
        }
        // add new doorway to new room doorways at modified parameter's position and rotation
        for(int i = 0; i < modifiedParameters.relativeDoorParameters.Count; i++){
            Vector3 newDoorPosition = newRoom.worldPosition + modifiedParameters.relativeDoorParameters[i].relativePosition;
            DungeonDoorway newDoorway = new DungeonDoorway(modifiedParameters.relativeDoorParameters[i], newDoorPosition, newRoom);
            newRoom.doorways.Add(newDoorway);
        }

        //check validity of bounding boxes
        List<DungeonRoom> roomsToCheck = new List<DungeonRoom>(rooms);
        //don't check the parent room
        roomsToCheck.Remove(branchFromDoor.parentRoom);

        //check if bounding box overlaps with any other bounding boxes closer than prune distance
        foreach(DungeonRoom room in roomsToCheck){
            if(Vector3.Distance(room.worldPosition,newRoom.worldPosition) < boundingBoxCheckPruneDistance && AreOBBsIntersecting(newRoom.boundingBox, room.boundingBox)){
                successfullyAddedRoom = false;
            }
        }
        //return new room and if it was sucessfully added
        return (newRoom, successfullyAddedRoom);
    }

    //helper functions for initializing rooms

    //change room parameters to be in relation to the branching door
    //pass in dungeon room params, the out vector of the banch room, the world rotation of the previous room, and the attatching door index
    private (DungeonRoomParameters returnedParams, Quaternion returnedQuaternion) ModifyRoomParamsToFitDoor(DungeonRoomParameters inParams, Vector3 vectorToOppose, Quaternion prevRotation, int attachingDoorIndex){
        DungeonRoomParameters returnParameters = inParams;
        //Get outward direction of connecting door
        Vector3 currentAttatchingDoorOutVector = GetOutVectorFromAngle(inParams.relativeDoorParameters[attachingDoorIndex].angle);
        //get reverse of the branching door's outward direction, this is what we want out connection door to point towards.
        Vector3 targetVector = -vectorToOppose;
        //calulate target up (this is overkill now, it was going to be used if doors could face in angles other than only y rotation)
        Vector3 targetUp = prevRotation * Quaternion.Euler(inParams.relativeDoorParameters[attachingDoorIndex].angle)* Vector3.up;
        //get quaternion rotation needed to point room towards target vector
        Quaternion rotationToApply = GetAlignmentQuaternion(currentAttatchingDoorOutVector, targetVector);
        //rotate all door parameters on room parameter by rotationToApply.
        List<DungeonDoorParameters> newDoorParameters = new List<DungeonDoorParameters>();
        for(int i = 0; i < returnParameters.relativeDoorParameters.Count; i++){
            Vector3 rotatedPoint = RotateAround(returnParameters.relativeDoorParameters[i].relativePosition,inParams.centerPoint,rotationToApply);
            newDoorParameters.Add(new DungeonDoorParameters(rotatedPoint,returnParameters.relativeDoorParameters[i].angle));
        }
        returnParameters.relativeDoorParameters = newDoorParameters;
        return(returnParameters,rotationToApply);
    }

    //given a rotation euler vector, return the forward vector
    private Vector3 GetOutVectorFromAngle(Vector3 angle){
        Quaternion rotation = Quaternion.Euler(angle);
        return rotation *(new Vector3(0,0,1)) ;
    }

    //
    private Quaternion GetAlignmentQuaternion(Vector3 currentForward, Vector3 targetForward)
    {
        //get euler rotation needed to go from current forward to target forward
        float angleY = Vector3.SignedAngle(new Vector3(currentForward.x, 0, currentForward.z), 
                                   new Vector3(targetForward.x, 0, targetForward.z), 
                                   Vector3.up);
        //get quaternion from angle
        Quaternion rotationNeeded = Quaternion.Euler(new Vector3(0,angleY,0));
        return rotationNeeded;
    }

    //rotate point 1 around point 2 by rotation quaternion
    private Vector3 RotateAround(Vector3 point1, Vector3 point2, Quaternion rotation)
    {
        // Step 1: Translate Point1 so that Point2 is the origin (relative to Point2)
        Vector3 translatedPoint = point1 - point2;

        // Step 2: Rotate the translated point by the quaternion
        Vector3 rotatedTranslatedPoint = rotation * translatedPoint;

        // Step 3: Translate the point back to the original space relative to Point2
        Vector3 finalPoint = rotatedTranslatedPoint + point2;

        return finalPoint;
    }
    
    //this function takes in a list of type T and a function that gets the weight for each T, and returns a weighted randomized queue
    //if weight is 0, it will not be added to the que
    private Queue<T> WeightedOrderShuffle<T>(List<T> possibilities, Func<T,float> weightGetter){
        Queue<T> returnList = new Queue<T>();
        //loop through all posibilities
        while(possibilities.Count > 0){
            T choosenOne = possibilities[0];
            float totalWeight = 0f;
            //get total weight of all possibilities
            foreach(T elem in possibilities){
                totalWeight += weightGetter(elem);
            }
            //get random float
            float rand = RandomFloat(0f,totalWeight);
            float cumulativeWeight = 0f;
            bool found = false;
            //loop through list while incrementing to find the choosen value
            foreach(T elem in possibilities){
                cumulativeWeight += weightGetter(elem);
                if (rand < cumulativeWeight & found == false)
                {
                    found = true;
                    choosenOne = elem; // Return the selected element
                }
            }
            //if the weight of the choosen one doesn't equal 0, add it to the queue4
            if(weightGetter(choosenOne) > 0){
                returnList.Enqueue(choosenOne);
            }
            possibilities.Remove(choosenOne);
        }
        return returnList;
    }


    //get a list of all the unused door in the dungeon
    private List<DungeonDoorway> GetUnusedDoors(Func<DungeonDoorway,bool> usableDoorFunc){
        List<DungeonDoorway> returnList = new List<DungeonDoorway>();
        foreach (DungeonDoorway doorway in doorways)
        {
            if(doorway.connectingRooms.Count == 1 && (doorway.worldPosition.y == availableDoorheight || !forceLevels)){
                if(usableDoorFunc(doorway)){
                    returnList.Add(doorway);
                }
            }
        }
        Debug.Log("unused door count is "+returnList.Count);
        return returnList;
        
    }
    //helper functions
    //get random float
    float RandomFloat(float min, float max){
        return (float)(random.NextDouble() * (max - min) + min);
    }


    List<DungeonRoom> GetDungeonRoomNeighbors(DungeonRoom checking){
        List<DungeonRoom> returnList = new List<DungeonRoom>();
        foreach(DungeonDoorway doorway in checking.doorways){
            foreach(DungeonRoom room in doorway.connectingRooms){
                if(room.roomId != checking.roomId){
                    returnList.Add(room);
                }
            }
        }
        return returnList;
    }

    void UpdateNeighborCenterDistances(DungeonRoom currRoom, DungeonRoom prevRoom){
        //case of center room
        if(prevRoom.roomId == currRoom.roomId){
            currRoom.graphDistanceFromCenter = 0;
        }
        List<DungeonRoom> neighborList = GetDungeonRoomNeighbors(currRoom);
        //foreach(DungeonRoom room in neighborList){
        for(int i = 0; i < neighborList.Count; i++){
            if(neighborList[i].roomId != currRoom.roomId && neighborList[i].roomId != prevRoom.roomId){
                neighborList[i].graphDistanceFromCenter = currRoom.graphDistanceFromCenter + 1;
                UpdateNeighborCenterDistances(neighborList[i], currRoom);
            }
        }
    }

    //check if OBBS are intersecting
    bool AreOBBsIntersecting(OBB obb1, OBB obb2)
    {
        Vector3[] axes1 = obb1.Axes;
        Vector3[] axes2 = obb2.Axes;

        // The 15 potential separating axes
        List<Vector3> separatingAxes = new List<Vector3>();

        // Add the 3 axes of OBB1 and OBB2
        separatingAxes.AddRange(axes1);
        separatingAxes.AddRange(axes2);

        // Add the 9 cross-product axes
        for (int i = 0; i < 3; i++)
        {
            for (int j = 0; j < 3; j++)
            {
                Vector3 crossProduct = Vector3.Cross(axes1[i], axes2[j]);
                if (crossProduct.sqrMagnitude > 1e-6f) // Ignore near-zero length axes
                {
                    separatingAxes.Add(crossProduct.normalized);
                }
            }
        }

        // Test for overlap on each axis
        foreach (Vector3 axis in separatingAxes)
        {
            if (!IsOverlappingOnAxis(obb1, obb2, axis))
            {
                // Found a separating axis, OBBs do not intersect
                return false;
            }
        }

        // No separating axis found, OBBs are intersecting
        return true;
    }
    //OBB helper function
    bool IsOverlappingOnAxis(OBB obb1, OBB obb2, Vector3 axis)
    {
        // Project the center of both OBBs onto the axis
        float centerProjection1 = Vector3.Dot(obb1.Center, axis);
        float centerProjection2 = Vector3.Dot(obb2.Center, axis);

        // Project the half-extents of both OBBs onto the axis
        float projection1 = GetProjectionLength(obb1, axis);
        float projection2 = GetProjectionLength(obb2, axis);

        // Check if projections overlap
        float distanceBetweenCenters = Mathf.Abs(centerProjection1 - centerProjection2);
        float totalExtent = projection1 + projection2;

        return distanceBetweenCenters <= totalExtent;
    }

    //OBB helper function
    float GetProjectionLength(OBB obb, Vector3 axis)
    {
        // Project the half-extents of the OBB onto the axis
        float projectionLength = 0;

        for (int i = 0; i < 3; i++) // Loop over the OBB's 3 axes
        {
            projectionLength += Mathf.Abs(Vector3.Dot(obb.Axes[i], axis)) * obb.HalfExtents[i];
        }

        return projectionLength;
    }

    private bool IsPointInsideOBB(Vector3 point, OBB obb)
    {
        // Vector from the center of the OBB to the point
        Vector3 V = point - obb.Center;

        // Project the vector V onto each of the OBB's axes
        float projX = Vector3.Dot(V, obb.Axes[0]);
        float projY = Vector3.Dot(V, obb.Axes[1]);
        float projZ = Vector3.Dot(V, obb.Axes[2]);

        // Check if the projections are within the extents on each axis
        return (Math.Abs(projX) <= obb.HalfExtents[0]) &&
               (Math.Abs(projY) <= obb.HalfExtents[1]) &&
               (Math.Abs(projZ) <= obb.HalfExtents[2]);
    }

    void PlacePlayer(){
        LocomotionManager.Instance.Teleport(currentCenterRoom.worldPosition + new Vector3(0.5f,0.5f,0.5f));
    }

    void SetupEnemies()
    {
        enemySpawner.SpawnEnemies();
    }

}