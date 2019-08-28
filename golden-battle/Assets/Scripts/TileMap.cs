using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

[ExecuteInEditMode]
[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
[RequireComponent(typeof(MeshCollider))]
public class TileMap : MonoBehaviour{

    public int size_x = 10;
    public int size_z = 10;
    public float tileSize = 1.0f;

    public GameObject selectedUnit;

    Node[,] pathingGraph;
    bool[,] isTileWalkable;
    List<GameObject> highLightPlanes = null;

    // Start is called before the first frame update
    void Start()
    {
        BuildMesh();
        GeneratePathFindingGraph();
        PopulateWalkable();
    }

    private void PopulateWalkable() {
        isTileWalkable = new bool[size_x, size_z];

        for (int i = 0; i < size_z; i++) {
            for (int j = 0; j < size_x; j++) {
                isTileWalkable[i, j] = true;
            }
        }

        isTileWalkable[4, 4] = false;
        isTileWalkable[5, 4] = false;
    }

    private void GeneratePathFindingGraph() {
        pathingGraph = new Node[size_x, size_z];
        int[,] directions = new int[,] {
            {0,-1},
            {1,0},
            {0,1},
            {-1,0},
        };

        for (int i = 0; i < size_z; i++) {
            for (int j = 0; j < size_x; j++) {
                pathingGraph[j, i] = new Node();
                pathingGraph[j, i].z = i;
                pathingGraph[j, i].x = j;
            }
        }

        for (int i = 0; i < size_z; i++) {
            for (int j = 0; j < size_x; j++) {
                for (int dir = 0; dir < 4; dir++) {
                    if (i + directions[dir, 0] >= 0
                        && i + directions[dir, 0] < size_z
                        && j + directions[dir, 1] >= 0
                        && j + directions[dir, 1] < size_x) {
                        pathingGraph[i, j].adjacent.Add(pathingGraph[i + directions[dir, 0], j + directions[dir, 1]]);
                    }
                }
            }
        }
    }

    public void BuildMesh() {
        //Mesh data
        int vsize_x = size_x + 1;
        int vsize_z = size_z + 1;
        int numTiles = size_x * size_z;
        int numVert = numTiles * 4;
        //Generate mesh
        Vector3[] vertices = new Vector3[numVert];
        int[] triangles = new int[numTiles * 6];
        Vector3[] normals = new Vector3[numVert];
        Vector2[] uv = new Vector2[numVert];

        for (int z = 0; z < size_z; z++) {
            for (int x = 0; x < size_x; x++) {
                int squareIndex = z * size_x + x;
                int triOffset = squareIndex * 6;
                vertices[squareIndex*4 + 0] = new Vector3(x * tileSize, 0, z * tileSize);
                vertices[squareIndex*4 + 1] = new Vector3(x * tileSize, 0, (z+1) * tileSize);
                vertices[squareIndex*4 + 2] = new Vector3((x+1) * tileSize, 0, z * tileSize);
                vertices[squareIndex*4 + 3] = new Vector3((x+1) * tileSize, 0, (z+1) * tileSize);

                normals[squareIndex * 4 + 0] = normals[squareIndex * 4 + 1] = normals[squareIndex * 4 + 2] = normals[squareIndex * 4 + 3] = Vector3.up;

                uv[squareIndex * 4 + 0] = new Vector2(0, 0);
                uv[squareIndex * 4 + 1] = new Vector2(0, 1);
                uv[squareIndex * 4 + 2] = new Vector2(1, 0);
                uv[squareIndex * 4 + 3] = new Vector2(1, 1);


                triangles[triOffset + 0] = squareIndex * 4 + 0;
                triangles[triOffset + 1] = squareIndex * 4 + 1;
                triangles[triOffset + 3] = squareIndex * 4 + 2;
                triangles[triOffset + 2] = squareIndex * 4 + 2;
                triangles[triOffset + 4] = squareIndex * 4 + 1;
                triangles[triOffset + 5] = squareIndex * 4 + 3;

            }
        }
            
        //New Mesh and populate
        Mesh mesh = new Mesh();
        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.normals = normals;
        mesh.uv = uv;

        //Assign to stuff

        MeshFilter meshFilter = GetComponent<MeshFilter>();
        MeshRenderer mesh_renderer = GetComponent<MeshRenderer>();
        MeshCollider meshCollider = GetComponent<MeshCollider>();

        meshFilter.mesh = mesh;
        meshCollider.sharedMesh = mesh;

        //BuildTexture();
    }

    public void GeneratePathTo(int x, int z) {
        
        selectedUnit.GetComponent<Unit>().currentPath = null;
        Dictionary<Node, float> dist = new Dictionary<Node, float>();
        Dictionary<Node, Node> prev = new Dictionary<Node, Node>();

        //This is a Dijkstra implementaion;
        Node source = pathingGraph[
            selectedUnit.GetComponent<Unit>().tileX,
            selectedUnit.GetComponent<Unit>().tileZ
            ];

        Node target = pathingGraph[
            x,
            z
            ];

        Debug.Log(target.x+ " " + target.z);
        dist[source] = 0;
        prev[source] = null;

        List<Node> unvisited = new List<Node>();

        //Set up distance to infinity
        foreach (Node v in pathingGraph) {
            if (v != source) {
                dist[v] = Mathf.Infinity;
                prev[v] = null;
                
            }
            unvisited.Add(v);
        }

        while (unvisited.Count > 0) {

            Node u = null;

            foreach (Node possibleU in unvisited) {
                if (u == null || dist[possibleU] < dist[u]) {
                    u = possibleU;
                }
            }

            if (u == target) {
                break;
            }

            unvisited.Remove(u);

            foreach (Node v in u.adjacent) {
                //float alt = dist[u] + u.DistanceTo(v);\
                if (isTileWalkable[v.x, v.z]) {
                    float alt = dist[u] + 1;
                    if (alt < dist[v]) {
                        dist[v] = alt;
                        prev[v] = u;
                    }
                }
            }
        }

        if (prev[target] == null || (dist[target] < selectedUnit.GetComponent<Unit>().minMoveRange || dist[target] > selectedUnit.GetComponent<Unit>().maxMoveRange)) {
            return;
        }
        else {
            List<Node> currentPath = new List<Node>();
            Node curr = target;

            while (curr != null) {
                currentPath.Add(curr);
                curr = prev[curr];
            }

            currentPath.Reverse();

            selectedUnit.GetComponent<Unit>().currentPath = currentPath;
            clearHighLight();

            isTileWalkable[source.x, source.z] = true;
            isTileWalkable[x, z] = false;
       
        }
    }
    // Update is called once per frame

    public Vector3 TileCoordToWorldCoord(int x, int z) {
        return new Vector3(x+0.5f, 0.2f, z+0.5f);
    }

    public void HighlightRangeAroundUnit(int minRange, int maxRange) {
        Queue<Node> nextNodes = new Queue<Node>();
        Queue<int> levels = new Queue<int>();
        List<Node> tobeHighLighted = new List<Node>();
        bool[,] visited = new bool[size_x, size_z];

        clearHighLight();

        for (int z = 0; z < size_z; z++) {
            for (int x = 0; x < size_x; x++) {
                visited[x, z] = false;
            }
        }

        nextNodes.Enqueue(pathingGraph[selectedUnit.GetComponent<Unit>().tileX, selectedUnit.GetComponent<Unit>().tileZ]);
        levels.Enqueue(0);

        while (nextNodes.Count != 0) {
            Node currNode = nextNodes.Dequeue();
            int currLevel = levels.Dequeue();
            if (currLevel > maxRange || visited[currNode.x, currNode.z]) continue;
            visited[currNode.x, currNode.z] = true;
            if (currLevel >= minRange
                && currLevel <= maxRange) {
                tobeHighLighted.Add(currNode);
            }

            foreach (Node v in currNode.adjacent) { 
                if (isTileWalkable[v.x, v.z] && !visited[v.x,v.z]) {
                    nextNodes.Enqueue(v);
                    levels.Enqueue(currLevel + 1);
                    
                }
            }
        }
        GameObject hlPrefab = Resources.Load<GameObject>("Prefab/PinkHightLight");
 

        foreach(Node v in tobeHighLighted) {
            GameObject go = Instantiate(hlPrefab, TileCoordToWorldCoord(v.x, v.z) + new Vector3(0,-0.199f,0), Quaternion.identity);
            highLightPlanes.Add(go);
        }
    }

    private void clearHighLight() {
        if (highLightPlanes != null) {
            foreach (GameObject go in highLightPlanes) {
                Destroy(go);
            }
            highLightPlanes.Clear();
        }
        else {
            highLightPlanes = new List<GameObject>();
        }
    }

    void Update(){
        
    }
}
