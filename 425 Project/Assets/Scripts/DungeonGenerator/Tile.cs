using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tile : MonoBehaviour
{

    public enum TextureType{
        Floor,
        Wall,
        Door
    }

    private static List<Transform> activatedWalls = new List<Transform>();
    private static List<Transform> activatedFloors = new List<Transform>();

    public static void clearActivatedWalls(){
        activatedWalls.Clear();
        activatedFloors.Clear();
        Debug.Log(activatedFloors.Count + "is count");
    }

    public static GameObject torch;

    public static List<GameObject> decor;

    [SerializeField]
    List<Material> roomFloorMaterial;
    [SerializeField]
    List<Material> roomWallMaterial;
    [SerializeField]
    List<Material> hallwayFloorMaterial;
    [SerializeField]
    List<Material> hallwayWallMaterial;
    [SerializeField]
    List<Material> doorMaterial;
    [SerializeField]
    MeshRenderer floor;
    [SerializeField]
    MeshRenderer leftWall;
    [SerializeField]
    MeshRenderer upWall;
    [SerializeField]
    MeshRenderer rightWall;
    [SerializeField]
    MeshRenderer downWall;
    
    [SerializeField]
    MeshRenderer ceiling;
    Dictionary<Vector2Int,MeshRenderer> sides;

    CellType cellType;


    public void Start(){
        if(sides == null){
            sides = new Dictionary<Vector2Int,MeshRenderer>();
            sides.Add(new Vector2Int(0,0),floor);
            sides.Add(new Vector2Int(-1,0),leftWall);
            sides.Add(new Vector2Int(0,1),upWall);
            sides.Add(new Vector2Int(1,0),rightWall);
            sides.Add(new Vector2Int(0,-1),downWall);
            sides.Add(new Vector2Int(1,1),ceiling);
        }
    }

    public void setCellType(CellType cellType){
        this.cellType = cellType;
    }

    public CellType getCellType(){
        return cellType;
    }
    
    //this function will need to be rewritten maybe

    //position is used to dertermin which mesh to change, 
    public void setTexture(Vector2Int position, TextureType type){
        
        //Debug.Log("TextureType is " + type + "cellType is "+ cellType);
        if(sides == null){
            sides = new Dictionary<Vector2Int,MeshRenderer>();
            sides.Add(new Vector2Int(0,0),floor);
            sides.Add(new Vector2Int(-1,0),leftWall);
            sides.Add(new Vector2Int(0,1),upWall);
            sides.Add(new Vector2Int(1,0),rightWall);
            sides.Add(new Vector2Int(0,-1),downWall);
            sides.Add(new Vector2Int(1,1),ceiling);
        }
        MeshRenderer toChange = sides[position];
        //find texture
        Material material = null;

        if(type == TextureType.Floor && cellType == CellType.Room){
            material = SetRandomMaterial(roomFloorMaterial);
            if(position != new Vector2Int(1,1)){
                activatedFloors.Add(toChange.gameObject.transform);
            }
        }else if(type == TextureType.Wall && cellType == CellType.Room){
            material = SetRandomMaterial(roomWallMaterial);
            activatedWalls.Add(toChange.gameObject.transform);
            toChange.transform.GetComponent<Collider>().enabled = true;
        }else if(type == TextureType.Floor && cellType == CellType.Hallway){
            material = SetRandomMaterial(hallwayFloorMaterial);
            if(position != new Vector2Int(1,1)){
                activatedFloors.Add(toChange.gameObject.transform);
            }
        }else if(type == TextureType.Wall && cellType == CellType.Hallway){
            material = SetRandomMaterial(hallwayWallMaterial);
            activatedWalls.Add(toChange.gameObject.transform);
            toChange.transform.GetComponent<Collider>().enabled = true;
        }else if(type == TextureType.Door){
            material = SetRandomMaterial(doorMaterial);
            if(activatedWalls.Contains(toChange.gameObject.transform)){
                toChange.transform.GetComponent<Collider>().enabled = false;
                activatedWalls.Remove(toChange.gameObject.transform);
            }
        }
        Material[] newList = new Material[1];
        newList[0] = material;
        toChange.materials = newList;
        
        
    }


    public static void spawnTorches(){
        List<int> usedNums = new List<int>();
        List<Vector3> usedPositions = new List<Vector3>();
        for(int i = 0; i < activatedWalls.Count/10; i++){
            if(usedNums.Count == activatedWalls.Count){
                return;
            }
            int rand = Random.Range(0,activatedWalls.Count-1);
            bool whileBool = true;
            while(whileBool){
                rand = Random.Range(0,activatedWalls.Count-1);
                whileBool = usedNums.Contains(rand);
                if(!whileBool){
                    bool temp = false;
                    foreach(Vector3 pos in usedPositions){
                        if(Vector3.Distance(pos,activatedWalls[rand].transform.position) < 3){
                            temp = true;
                        }
                    }
                    if(temp){
                        whileBool = true;
                        usedNums.Add(rand);
                    }
                }

            }
            usedNums.Add(rand);
            usedPositions.Add(activatedWalls[rand].position);
            GameObject newTorch = Instantiate(torch,activatedWalls[rand].transform.position, activatedWalls[rand].transform.rotation);
            Generator2D.instance.spawnedThings.Add(newTorch);
        }
        /*
        for(int i = 0; i < activatedWalls.Count; i+= 10){
            GameObject newTorch = Instantiate(torch,activatedWalls[i].transform.position, activatedWalls[i].transform.rotation);
        }
        */
        

    }

    public static void spawnDecor(){
        
    
        List<int> usedNums = new List<int>();
        for(int i = 0; i < activatedFloors.Count/10; i++){
            if(usedNums.Count == activatedFloors.Count){
                return;
            }
            int rand = Random.Range(0,activatedFloors.Count-1);
            bool whileBool = true;
            while(whileBool){
                rand = Random.Range(0,activatedFloors.Count-1);
                whileBool = usedNums.Contains(rand);

            }
            usedNums.Add(rand);
            GameObject newDecor = Instantiate(decor[Random.Range(0,decor.Count)],activatedFloors[rand].transform.position, Quaternion.identity);
            Generator2D.instance.spawnedThings.Add(newDecor);
            randomizeDecorPosition(newDecor.transform, activatedFloors[rand].transform.position + new Vector3(-0.3f,0,-0.3f),activatedFloors[rand].transform.position + new Vector3(0.3f,0,0.3f));
            randomizeDecorRotation(newDecor.transform);
        }
        
        
    }

    private Material SetRandomMaterial(List<Material> matList){
        int rand = Random.Range(0,matList.Count);
        return matList[rand];
    }

    private static void randomizeDecorPosition(Transform toMove, Vector3 minClamp, Vector3 maxClamp){
        float xShift = Mathf.Round((Random.Range(minClamp.x,maxClamp.x))*16)/16;
        float yShift = Mathf.Round((Random.Range(minClamp.y,maxClamp.y))*16)/16;
        float zShift = Mathf.Round((Random.Range(minClamp.z,maxClamp.z))*16)/16;
        toMove.transform.position = new Vector3(xShift,yShift,zShift);
    }

    private static void randomizeDecorRotation(Transform toMove){
        int rand = Random.Range(0,4);
        rand *= 90;
        toMove.eulerAngles += new Vector3(0,rand,0);
    }

    
}
