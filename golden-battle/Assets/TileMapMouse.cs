﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(TileMap))]
public class TileMapMouse : MonoBehaviour {
    TileMap tileMap;
    Collider col;
    Vector3 currentTileCoord;

    // Start is called before the first frame update
    void Start() {
        tileMap = GetComponent<TileMap>();
        col = GetComponent<MeshCollider>();
    }

    public void OnMouseUp() {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hitInfo;

        //Debug.DrawRay(ray.origin, ray.direction * 10, Color.red,5);
        if (col.Raycast(ray, out hitInfo, 100f)) {
            int x = Mathf.FloorToInt(hitInfo.point.x / tileMap.tileSize);
            int z = Mathf.FloorToInt(hitInfo.point.z / tileMap.tileSize);

            Debug.Log("Tile:" + x + "," + z);

            tileMap.GeneratePathTo(x, z);

            currentTileCoord.x = x;
            currentTileCoord.z = z;
        }
    }

    // Update is called once per frame
    void Update() {
        

        // Use these coord to move unit to for example
    }
}