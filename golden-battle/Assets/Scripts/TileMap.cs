using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;


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

    public GameManager gameManager;

   

    // Start is called before the first frame update
    void Start()
    {
        BuildMesh();
        GeneratePathFindingGraph();
        //PopulateWalkable();
        Debug.Log("Tile Map Init Done");
    }

    public void PopulateWalkable(Unit[,] unitsMap) {
        isTileWalkable = new bool[size_x, size_z];

        for (int z = 0; z < size_z; z++) {
            for (int x = 0; x < size_x; x++) {
                isTileWalkable[x, z] = new bool();
                if (unitsMap[x,z] != null) {
                    isTileWalkable[x, z] = false;
                } else {
                    isTileWalkable[x, z] = true;
                }
            }
        }
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
                pathingGraph[j, i] = new Node {
                    z = i,
                    x = j
                };
            }
        }

        for (int z = 0; z < size_z; z++) {
            for (int x = 0; x < size_x; x++) {
                for (int dir = 0; dir < 4; dir++) {
                    if (z + directions[dir, 1] >= 0
                        && z + directions[dir, 1] < size_z
                        && x + directions[dir, 0] >= 0
                        && x + directions[dir, 0] < size_x) {
                        pathingGraph[x, z].adjacent.Add(pathingGraph[x + directions[dir, 0], z + directions[dir, 1]]);
                    }
                }
            }
        }

        Debug.Log("Pathing graph done");
        Debug.Log(pathingGraph[4, 1]);
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
        Mesh mesh = new Mesh {
            vertices = vertices,
            triangles = triangles,
            normals = normals,
            uv = uv
        };

        //Assign to stuff

        MeshFilter meshFilter = GetComponent<MeshFilter>();
        MeshRenderer mesh_renderer = GetComponent<MeshRenderer>();
        MeshCollider meshCollider = GetComponent<MeshCollider>();

        meshFilter.mesh = mesh;
        meshCollider.sharedMesh = mesh;

        //BuildTexture();
    }

    public void GeneratePathTo(int x, int z, bool isAtk) {

        Unit currUnit = selectedUnit.GetComponent<Unit>();

        if ((!isAtk && !currUnit.canMove) || (isAtk & !currUnit.canAtk)) return;
        
        currUnit.currentPath = null;
        Dictionary<Node, float> dist = new Dictionary<Node, float>();
        Dictionary<Node, Node> prev = new Dictionary<Node, Node>();

        //This is a Dijkstra implementaion;
        Node source = pathingGraph[
            currUnit.tileX,
            currUnit.tileZ
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
                if (isTileWalkable[v.x, v.z] || isAtk) {
                    float alt = dist[u] + 1;
                    if (alt < dist[v]) {
                        dist[v] = alt;
                        prev[v] = u;
                    }
                }
            }
        }

        if (!isAtk) {
            if (prev[target] == null || (dist[target] < currUnit.minMoveRange || dist[target] > currUnit.maxMoveRange)) {
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

                currUnit.SetPath(currentPath);
                ClearHighLight();


                isTileWalkable[source.x, source.z] = true;
                isTileWalkable[x, z] = false;

                gameManager.unitsMap[x, z] = gameManager.unitsMap[source.x, source.z];
                gameManager.unitsMap[source.x, source.z] = null;

                
                if (!currUnit.canAtk && !currUnit.canMove) {
                    gameManager.NotifyUnitDone();
                }
            }
        } else {
            if (prev[target] == null || (dist[target] < currUnit.minAtkRange || dist[target] > currUnit.maxAtkRange)) {
                return;
            }
            ClearHighLight();
            if (!currUnit.canAtk && !currUnit.canMove) {
                gameManager.NotifyUnitDone();
            }
            StartCoroutine(DoAttack(currUnit, gameManager.unitsMap[x, z]));
            
        }
        
    }

    private IEnumerator DoAttack(Unit Attacker, Unit Target) {
        float dmg_multiplier = 1.0f - ((0.052f * Target.armor) / (0.9f + 0.048f * Math.Abs(Target.armor)));
        int dmg = (int)Math.Floor(Attacker.damage*1.0f * dmg_multiplier);
        Debug.Log(dmg);
        Attacker.canAtk = false;
        Attacker.gameObject.transform.LookAt(Target.gameObject.transform);
        Target.gameObject.transform.LookAt(Attacker.gameObject.transform);
        Attacker.animator.SetTrigger("AttackTrigger");
        yield return new WaitForSeconds(1);
        DamagePopUp.Create(TileCoordToWorldCoord(Target.tileX, Target.tileZ) + new Vector3(0,2.1f,0), dmg);
        Target.animator.SetTrigger("HitTrigger");
        Target.DoDamage(dmg);
        gameManager.currAction = 0;
    }

    public Vector3 TileCoordToWorldCoord(int x, int z) {
        return new Vector3(x+0.5f, 0.2f, z+0.5f);
    }

    public void HighlightRangeAroundUnit(int minRange, int maxRange, string Color, bool isAtk) {
        Queue<Node> nextNodes = new Queue<Node>();
        Queue<int> levels = new Queue<int>();
        List<Node> tobeHighLighted = new List<Node>();
        bool[,] visited = new bool[size_x, size_z];

        ClearHighLight();

        for (int z = 0; z < size_z; z++) {
            for (int x = 0; x < size_x; x++) {
                visited[x, z] = false;
            }
        }

        //Debug.Log(selectedUnit.GetComponent<Unit>().tileX + " " + selectedUnit.GetComponent<Unit>().tileZ);
        Debug.Log(pathingGraph[0, 0]);

        nextNodes.Enqueue(pathingGraph[selectedUnit.GetComponent<Unit>().tileX, selectedUnit.GetComponent<Unit>().tileZ]);
        levels.Enqueue(0);

        while (nextNodes.Count != 0) {
            Node currNode = nextNodes.Dequeue();
            int currLevel = levels.Dequeue();
            if (visited[currNode.x, currNode.z]) continue;
            visited[currNode.x, currNode.z] = true;
            if (currLevel >= minRange
                && currLevel <= maxRange) {
                tobeHighLighted.Add(currNode);
            }

            foreach (Node v in currNode.adjacent) { 
                if ((isTileWalkable[v.x, v.z] || isAtk) && !visited[v.x,v.z] && currLevel + 1 <= maxRange) {
                    nextNodes.Enqueue(v);
                    levels.Enqueue(currLevel + 1);
                    
                }
            }
        }
        GameObject hlPrefab;
        if (Color.Equals("Pink")) {
             hlPrefab = Resources.Load<GameObject>("Prefab/PinkHightLight");
        } else {
             hlPrefab = Resources.Load<GameObject>("Prefab/PurpleHighLight");
        }
        
 

        foreach(Node v in tobeHighLighted) {
            GameObject go = Instantiate(hlPrefab, TileCoordToWorldCoord(v.x, v.z) + new Vector3(0,-0.199f,0), Quaternion.identity);
            highLightPlanes.Add(go);
        }
    }



    private void ClearHighLight() {
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

    public void FreeWalkable (int x, int z) {
        isTileWalkable[x, z] = true;
    }

    void Update(){
        
    }
}
