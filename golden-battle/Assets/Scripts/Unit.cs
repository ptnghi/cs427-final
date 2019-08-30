using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Unit : MonoBehaviour
{

    public int tileX;
    public int tileZ;

    public TileMap map;
    public List<Node> currentPath = null;
    private Collider coll;
    public GameManager gm;

    float tempTime;
    const float sendRate = 0.1f;

    public int minMoveRange = 2;
    public int maxMoveRange = 4;
    public int minAtkRange;
    public int maxAtkRange;

    public int health;
    public int armor;
    public int damage;
    public int team;

    public bool canMove = false;
    public bool canAtk = false;

    void Start()
    {
        coll = GetComponent<BoxCollider>();
    }

    public void OnMouseUp() {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hitInfo;

        if (coll.Raycast(ray, out hitInfo, 100.0f)) {
            if (gm.currTurn == team) {
                Debug.Log("Select Unit");
                gm.SelectUnit(gameObject);
            }
            else if (gm.currAction == 1) {
                Debug.Log("Target Unit");
                map.GeneratePathTo(tileX, tileZ, true);
            }
        }
    }

    // Update is called once per frame
    void Update(){
        if (currentPath != null) {
            int currNode = 0;

            while (currNode < currentPath.Count - 1) {
                Vector3 start = map.TileCoordToWorldCoord(currentPath[currNode].x, currentPath[currNode].z)
                    + new Vector3(0, 0.2f, 0);
                Vector3 end = map.TileCoordToWorldCoord(currentPath[currNode + 1].x, currentPath[currNode + 1].z)
                    + new Vector3(0, 0.2f, 0); ;


                Debug.DrawLine(start, end, Color.red);

                currNode++;
            }
        }
        tempTime += Time.deltaTime;
        if (tempTime > sendRate) {
            tempTime -= sendRate;
            MoveFollowPath();
        }

    }

    public void MoveFollowPath() {
        if (currentPath == null) {
            return;
        }

        currentPath.RemoveAt(0);

        tileX = currentPath[0].x;
        tileZ = currentPath[0].z;
        transform.position = map.TileCoordToWorldCoord(tileX, tileZ);

        if (currentPath.Count == 1) {
            currentPath = null;
        }
    }
}
