using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class Unit : MonoBehaviour {

    public int tileX;
    public int tileZ;

    public TileMap map;
    public List<Node> currentPath = null;
    private Collider coll;
    public GameManager gm;
    public event Action<int, int> OnHealthChange = delegate { };
    public Animator animator;

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
    public int currHealth;

    public bool canMove = false;
    public bool canAtk = false;


    private Node currMoveTarget = null;
    private float speed = 2.0f;


    void Start()
    {
        coll = GetComponent<CapsuleCollider>();
        currHealth = health;
        GetComponentInChildren<HealthBar>().OnHealthUpdateDone += OnHealthUpdateDone;
        animator = GetComponent<Animator>();
        DoDamage(0);
    }

    public void OnMouseUp() {
        if (!EventSystem.current.IsPointerOverGameObject()) {
            if (!gm.gameEnd) {
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                RaycastHit hitInfo;

                if (coll.Raycast(ray, out hitInfo, 100.0f)) {
                    if (gm.currTurn == team) {
                        Debug.Log("Select Unit");
                        gm.currAction = 0;
                        gm.SelectUnit(gameObject);
                    }
                    else if (gm.currAction == 1) {
                        Debug.Log("Target Unit");
                        gm.HideAttackButton();
                        map.GeneratePathTo(tileX, tileZ, true);
                    }
                }
            }
        }
    }

    // Update is called once per frame
    void Update(){
        if (currMoveTarget != null) {
            float step = speed * Time.deltaTime;
            Vector3 target = map.TileCoordToWorldCoord(currMoveTarget.x, currMoveTarget.z) + new Vector3(0, -0.2f, 0);
            transform.position = Vector3.MoveTowards(transform.position, target , step);
            transform.LookAt(target);

            if (Vector3.Distance(transform.position, map.TileCoordToWorldCoord(currMoveTarget.x, currMoveTarget.z) + new Vector3(0, -0.2f, 0)) < 0.001f) {
                currMoveTarget = null;
                if (currentPath != null) {
                    currentPath.RemoveAt(0);

                    tileX = currentPath[0].x;
                    tileZ = currentPath[0].z;

                    currMoveTarget = currentPath[0];

                    if (currentPath.Count == 1) {
                        currentPath = null;
                        //currMoveTarget = null;

                    }
                }
            }
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

    public void DoDamage(int amount) {
        currHealth -= amount;
        OnHealthChange(currHealth, health);
    }

    private void OnHealthUpdateDone() {
        if (currHealth <= 0) {
            gm.KillUnit(this);
        }
    }

    public void SetPath(List<Node> inPath) {
        currentPath = inPath;
        canMove = false;
        currMoveTarget = currentPath[0];
    }
}
