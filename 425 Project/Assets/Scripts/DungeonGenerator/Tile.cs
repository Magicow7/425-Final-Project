using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class Tile : MonoBehaviour
{
    public enum TextureType
    {
        Floor,
        Wall,
        Door
    }

    private static readonly List<Transform> _activatedWalls = new();
    private static readonly List<Transform> _activatedFloors = new();

    public static GameObject torch;

    public static List<GameObject> decor;

    [FormerlySerializedAs("roomFloorMaterial"), SerializeField]
    private List<Material> _roomFloorMaterial;

    [FormerlySerializedAs("roomWallMaterial"), SerializeField]
    private List<Material> _roomWallMaterial;

    [FormerlySerializedAs("hallwayFloorMaterial"), SerializeField]
    private List<Material> _hallwayFloorMaterial;

    [FormerlySerializedAs("hallwayWallMaterial"), SerializeField]
    private List<Material> _hallwayWallMaterial;

    [FormerlySerializedAs("doorMaterial"), SerializeField]
    private List<Material> _doorMaterial;

    [FormerlySerializedAs("floor"), SerializeField]
    private MeshRenderer _floor;

    [FormerlySerializedAs("leftWall"), SerializeField]
    private MeshRenderer _leftWall;

    [FormerlySerializedAs("upWall"), SerializeField]
    private MeshRenderer _upWall;

    [FormerlySerializedAs("rightWall"), SerializeField]
    private MeshRenderer _rightWall;

    [FormerlySerializedAs("downWall"), SerializeField]
    private MeshRenderer _downWall;

    [FormerlySerializedAs("ceiling"), SerializeField]
    private MeshRenderer _ceiling;

    private CellType _cellType;
    private Dictionary<Vector2Int, MeshRenderer> _sides;


    public void Start()
    {
        if (_sides == null)
        {
            _sides = new Dictionary<Vector2Int, MeshRenderer>();
            _sides.Add(new Vector2Int(0, 0), _floor);
            _sides.Add(new Vector2Int(-1, 0), _leftWall);
            _sides.Add(new Vector2Int(0, 1), _upWall);
            _sides.Add(new Vector2Int(1, 0), _rightWall);
            _sides.Add(new Vector2Int(0, -1), _downWall);
            _sides.Add(new Vector2Int(1, 1), _ceiling);
        }
    }

    public static void ClearActivatedWalls()
    {
        _activatedWalls.Clear();
        _activatedFloors.Clear();
        Debug.Log(_activatedFloors.Count + "is count");
    }

    public void SetCellType(CellType cellType)
    {
        _cellType = cellType;
    }

    public CellType GetCellType()
    {
        return _cellType;
    }

    //this function will need to be rewritten maybe

    //position is used to dertermin which mesh to change, 
    public void SetTexture(Vector2Int position, TextureType type)
    {
        //Debug.Log("TextureType is " + type + "cellType is "+ cellType);
        if (_sides == null)
        {
            _sides = new Dictionary<Vector2Int, MeshRenderer>();
            _sides.Add(new Vector2Int(0, 0), _floor);
            _sides.Add(new Vector2Int(-1, 0), _leftWall);
            _sides.Add(new Vector2Int(0, 1), _upWall);
            _sides.Add(new Vector2Int(1, 0), _rightWall);
            _sides.Add(new Vector2Int(0, -1), _downWall);
            _sides.Add(new Vector2Int(1, 1), _ceiling);
        }

        var toChange = _sides[position];
        //find texture
        Material material = null;

        if (type == TextureType.Floor && _cellType == CellType.Room)
        {
            material = SetRandomMaterial(_roomFloorMaterial);
            if (position != new Vector2Int(1, 1))
            {
                _activatedFloors.Add(toChange.gameObject.transform);
            }
        }
        else if (type == TextureType.Wall && _cellType == CellType.Room)
        {
            material = SetRandomMaterial(_roomWallMaterial);
            _activatedWalls.Add(toChange.gameObject.transform);
            toChange.transform.GetComponent<Collider>().enabled = true;
        }
        else if (type == TextureType.Floor && _cellType == CellType.Hallway)
        {
            material = SetRandomMaterial(_hallwayFloorMaterial);
            if (position != new Vector2Int(1, 1))
            {
                _activatedFloors.Add(toChange.gameObject.transform);
            }
        }
        else if (type == TextureType.Wall && _cellType == CellType.Hallway)
        {
            material = SetRandomMaterial(_hallwayWallMaterial);
            _activatedWalls.Add(toChange.gameObject.transform);
            toChange.transform.GetComponent<Collider>().enabled = true;
        }
        else if (type == TextureType.Door)
        {
            material = SetRandomMaterial(_doorMaterial);
            if (_activatedWalls.Contains(toChange.gameObject.transform))
            {
                toChange.transform.GetComponent<Collider>().enabled = false;
                _activatedWalls.Remove(toChange.gameObject.transform);
            }
        }

        var newList = new Material[1];
        newList[0] = material;
        toChange.materials = newList;
    }


    public static void SpawnTorches()
    {
        var usedNums = new List<int>();
        var usedPositions = new List<Vector3>();
        for (var i = 0; i < _activatedWalls.Count / 10; i++)
        {
            if (usedNums.Count == _activatedWalls.Count)
            {
                return;
            }

            int rand = Random.Range(0, _activatedWalls.Count - 1);
            var whileBool = true;
            while (whileBool)
            {
                rand = Random.Range(0, _activatedWalls.Count - 1);
                whileBool = usedNums.Contains(rand);
                if (!whileBool)
                {
                    var temp = false;
                    foreach (var pos in usedPositions)
                    {
                        if (Vector3.Distance(pos, _activatedWalls[rand].transform.position) < 3)
                        {
                            temp = true;
                        }
                    }

                    if (temp)
                    {
                        whileBool = true;
                        usedNums.Add(rand);
                    }
                }
            }

            usedNums.Add(rand);
            usedPositions.Add(_activatedWalls[rand].position);
            var newTorch = Instantiate(torch, _activatedWalls[rand].transform.position, _activatedWalls[rand].transform.rotation);
            Generator2D.instance._spawnedThings.Add(newTorch);
        }
        /*
        for(int i = 0; i < activatedWalls.Count; i+= 10){
            GameObject newTorch = Instantiate(torch,activatedWalls[i].transform.position, activatedWalls[i].transform.rotation);
        }
        */
    }

    public static void SpawnDecor()
    {
        var usedNums = new List<int>();
        for (var i = 0; i < _activatedFloors.Count / 10; i++)
        {
            if (usedNums.Count == _activatedFloors.Count)
            {
                return;
            }

            int rand = Random.Range(0, _activatedFloors.Count - 1);
            var whileBool = true;
            while (whileBool)
            {
                rand = Random.Range(0, _activatedFloors.Count - 1);
                whileBool = usedNums.Contains(rand);
            }

            usedNums.Add(rand);
            var newDecor = Instantiate(decor[Random.Range(0, decor.Count)], _activatedFloors[rand].transform.position, Quaternion.identity);
            Generator2D.instance._spawnedThings.Add(newDecor);
            RandomizeDecorPosition(newDecor.transform, _activatedFloors[rand].transform.position + new Vector3(-0.3f, 0, -0.3f), _activatedFloors[rand].transform.position + new Vector3(0.3f, 0, 0.3f));
            RandomizeDecorRotation(newDecor.transform);
        }
    }

    private Material SetRandomMaterial(List<Material> matList)
    {
        int rand = Random.Range(0, matList.Count);
        return matList[rand];
    }

    private static void RandomizeDecorPosition(Transform toMove, Vector3 minClamp, Vector3 maxClamp)
    {
        float xShift = Mathf.Round(Random.Range(minClamp.x, maxClamp.x) * 16) / 16;
        float yShift = Mathf.Round(Random.Range(minClamp.y, maxClamp.y) * 16) / 16;
        float zShift = Mathf.Round(Random.Range(minClamp.z, maxClamp.z) * 16) / 16;
        toMove.transform.position = new Vector3(xShift, yShift, zShift);
    }

    private static void RandomizeDecorRotation(Transform toMove)
    {
        int rand = Random.Range(0, 4);
        rand *= 90;
        toMove.eulerAngles += new Vector3(0, rand, 0);
    }
}