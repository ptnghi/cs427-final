using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour{

    public TileMap map;
    List<Unit>[] units;
    public Unit[,] unitsMap;
    Dictionary<string, string> unitPrefabPaths = new Dictionary<string, string>() {
        {"Archer", "Prefab/Archer"},
        {"General", "Prefab/General" },
        { "Warrior", "Prefab/Warrior"}
    };

    public GameObject attackButton;
       
    Unit selectedUnit;  

    public int currTurn;
    public int currAction; //0 = Move, 1 = attack
    int unitsDone;

    // Start is called before the first frame update
    void Start(){
        CreateMap();
        CreateUnits();
        currTurn = 1;
        attackButton.SetActive(false);
        NexTurn();
    }

    public void NotifyUnitDone() {
        unitsDone++;
        if (unitsDone == units[currTurn].Count) {
            NexTurn();
        }
    }

    public void EndTurn() {
        foreach (Unit unit in units[currTurn]) {
            unit.canMove = false;
            unit.canAtk = false;
        }
        NexTurn();
    }

    private void NexTurn() {
        unitsDone = 0;
        currTurn = (currTurn + 1) % 2;
        foreach(Unit unit in units[currTurn]) {
            unit.canMove = true;
            unit.canAtk = true;
        }
    }

    internal void SelectUnit(GameObject gameObject) {
        map.selectedUnit = gameObject;
        selectedUnit = gameObject.GetComponent<Unit>();
        map.HighlightRangeAroundUnit(selectedUnit.minMoveRange, selectedUnit.maxMoveRange, "Blue", false);
        attackButton.SetActive(true);
    }

    public void CreateUnits() {
        //We'll implement a unit picking and init position later
        //NOw we hard code lol
        units = new List<Unit>[2];
        units[0] = new List<Unit>();
        units[1] = new List<Unit>();
        unitsMap = new Unit[map.size_x, map.size_z];

        for (int z = 0; z < map.size_z; z++) {
            for (int x = 0; x < map.size_x; x++) {
                unitsMap[x, z] = null;
            }
        }

        List<PieceInfo> team1Board = new List<PieceInfo>() {
            new PieceInfo{pieceName = "Archer", posX = 3, posZ = 1 },
            new PieceInfo{pieceName = "Archer", posX = 5, posZ = 1 },
            new PieceInfo{pieceName = "Warrior", posX = 2, posZ = 2 },
            new PieceInfo{pieceName = "Warrior", posX = 6, posZ = 2 },
            new PieceInfo{pieceName = "General", posX = 4, posZ = 0 }
        };

        List<PieceInfo> team2Board = new List<PieceInfo>() {
            new PieceInfo{pieceName = "Archer", posX = 3, posZ = 8 },
            new PieceInfo{pieceName = "Archer", posX = 5, posZ = 8 },
            new PieceInfo{pieceName = "Warrior", posX = 2, posZ = 7 },
            new PieceInfo{pieceName = "Warrior", posX = 6, posZ = 7 },
            new PieceInfo{pieceName = "General", posX = 4, posZ = 9 }
        };

       

        foreach(PieceInfo piece in team1Board) {
            GameObject go = Instantiate(Resources.Load<GameObject>(unitPrefabPaths[piece.pieceName]), map.TileCoordToWorldCoord(piece.posX, piece.posZ), Quaternion.identity);
            Unit unit = go.GetComponent<Unit>();
            unit.team = 0;
            unit.map = map;
            unit.tileX = piece.posX;
            unit.tileZ = piece.posZ;
            unit.gm = this;
            units[0].Add(unit);
            unitsMap[unit.tileX, unit.tileZ] = unit;
        }

        foreach (PieceInfo piece in team2Board) {
            GameObject go = Instantiate(Resources.Load<GameObject>(unitPrefabPaths[piece.pieceName]), map.TileCoordToWorldCoord(piece.posX, piece.posZ), Quaternion.identity);
            Unit unit = go.GetComponent<Unit>();
            unit.team = 1;
            unit.map = map;
            unit.tileX = piece.posX;
            unit.tileZ = piece.posZ;
            unit.gm = this;
            units[1].Add(unit);
            unitsMap[unit.tileX, unit.tileZ] = unit;
        }

        map.PopulateWalkable(unitsMap);

    }

    public void CreateMap() {
        GameObject go = Instantiate(Resources.Load<GameObject>("Prefab/TileMap"), Vector3.zero, Quaternion.identity);
        map = go.GetComponent<TileMap>();
        map.gameManager = this;
    }

    public void AttackMode() {
        currAction = 1;
        map.HighlightRangeAroundUnit(selectedUnit.minAtkRange, selectedUnit.maxAtkRange, "Pink", true);
    }

    // Update is called once per frame
    void Update(){
        
    }
}
