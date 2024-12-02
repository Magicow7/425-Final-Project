using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DijkstrasGraph<TE> : MonoBehaviour
{
    private readonly Dictionary<string, Dictionary<string, int>> _adjacencyMap;
    private readonly Dictionary<string, TE> _dataMap;

    public DijkstrasGraph() {
		_adjacencyMap = new Dictionary<string, Dictionary<string, int>>();
		_dataMap = new Dictionary<string, TE>();
	}

    public void AddVertex(string vertexName, TE data){
        if(_dataMap.ContainsKey(vertexName)){
            Debug.Log("vertex by this key already stored in graph");
        }
        _adjacencyMap.Add(vertexName, new Dictionary<string,int>());
        _dataMap.Add(vertexName, data);

        
    }

    public void AddEdge(string vertex1Name, string vertex2Name, int cost){
        if(!_dataMap.ContainsKey(vertex1Name) || !_dataMap.ContainsKey(vertex2Name)) {
			Debug.Log("Vertex not found");
		}
        _adjacencyMap[vertex1Name].Add(vertex2Name, cost);
        _adjacencyMap[vertex2Name].Add(vertex1Name, cost);
    }

    //toString not added, refer to the Dijkstras graph project if you want that, if you are not silas: lol good luck.

    public Dictionary<string,int> GetAdjacentVertices(string vertexName){
		return _adjacencyMap[vertexName];
	}

    public int GetCost(string vertex1Name, string vertex2Name){
        if(!_dataMap.ContainsKey(vertex1Name) || !_dataMap.ContainsKey(vertex2Name)) {
			Debug.Log("Vertex not found");
		}
        int oneToTwo = _adjacencyMap[vertex1Name][vertex2Name];
        int twoToOne = _adjacencyMap[vertex2Name][vertex1Name];
        if(oneToTwo != twoToOne){
            Debug.Log("Cost mismatch");
        }
        return oneToTwo;
    }

    public void SetCost(string vertex1Name, string vertex2Name, int value){
        if(!_dataMap.ContainsKey(vertex1Name) || !_dataMap.ContainsKey(vertex2Name)) {
			Debug.Log("Vertex not found");
		}
        _adjacencyMap[vertex1Name][vertex2Name] = value;
        _adjacencyMap[vertex2Name][vertex1Name] = value;
    }

    public List<string> GetVetricies(){
        return new List<string>(this._dataMap.Keys);
    }
    
    public TE GetData(string vertex){
        if(!_dataMap.ContainsKey(vertex)) {
			Debug.Log("Vertex not found");
		}
        return _dataMap[vertex];
    }

    public List<string> DoBreadthFirstSearch(string startVertexName){
        if(!_dataMap.ContainsKey(startVertexName)) {
			Debug.Log("Vertex not found");
		}
        List<string> queue = new List<string>();
        List<string> visited = new List<string>();
        string currentVert;
        queue.Add(startVertexName);
        while(queue.Count > 0){
            currentVert = queue[0];
            queue.Remove(currentVert);
            visited.Add(currentVert);
            Dictionary<string,int> vertAdjs = _adjacencyMap[currentVert];
            foreach(string key in vertAdjs.Keys){
                if(!visited.Contains(key) && !queue.Contains(key)){
                    queue.Add(key);
                }
            }
            //queue.RemoveAt(0);
        }
        return visited;
    }

    // depth first searches not implemented
    public List<string> DoDijkstras(string startVertexName, string endVertexName){
    
        if(!_dataMap.ContainsKey(startVertexName) || !_dataMap.ContainsKey(endVertexName)) {
			Debug.Log("Vertex not found");
		}

        List<string> shortestPath = new List<string>();
        Dictionary<string, int> distances = new Dictionary<string,int>();
        Dictionary<string, List<string>> paths = new Dictionary<string, List<string>>();
        List<string> queue = new List<string>();
        List<string> visited = new List<string>();

        foreach(string key in _dataMap.Keys){
            distances.Add(key, int.MaxValue);
            List<string> putList = new List<string>();
            putList.Add("None");
            paths.Add(key,putList);
        }

        distances[startVertexName] = 0;
        List<string> putList2 = new List<string>();
        putList2.Add(startVertexName);
        paths[startVertexName] = putList2;
        queue.Add(startVertexName);

        string current = startVertexName;


        while(visited.Count < _dataMap.Count){
            Dictionary<string,int> adjs = _adjacencyMap[current];
            if(adjs != null){
                foreach(string key in adjs.Keys){
                    if(!visited.Contains(key) && !queue.Contains(key)){
                        queue.Add(key);
                    }
                    if(distances[key] > adjs[key] + distances[current]){
                        distances[key] = adjs[key] + distances[current];
                        List<string> previousPath = paths[current];
                        List<string> pathCopy = new List<string>();
                        foreach(string str in previousPath){
                            pathCopy.Add(new string(str));
                        }
                        pathCopy.Add(key);
                        paths[key] = pathCopy;
                    }
                }
            }
            visited.Add(current);
            queue.Remove(current);
            string currentSmallestName = "empty";
            int currentSmallestCost = int.MaxValue;
            foreach(string str in queue){
                int stringCost = distances[str];
                if(stringCost < currentSmallestCost){
                    currentSmallestName = str;
                    currentSmallestCost = stringCost;
                }
            }
            current = currentSmallestName;
        }

        int returnCost = distances[endVertexName];
        List<string> returnList = paths[endVertexName];

        if(returnList[0].Equals("None")){
            returnCost = -1;
        }
        foreach(string str in returnList){
            shortestPath.Add(str);
        }
        return shortestPath;
    }

}
