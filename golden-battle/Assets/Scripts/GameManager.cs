using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameManager : MonoBehaviour{

    public TileMap map;
    List<Unit>[] units;
    public Unit[,] unitsMap;
    Dictionary<string, string> unitPrefabPaths = new Dictionary<string, string>() {
        {"Archer", "Prefab/ArcherModel"},
        {"General", "Prefab/GeneralModel" },
        { "Warrior", "Prefab/WarriorModel"}
    };

    public GameObject attackButton;
    public GameObject nextTurnText;
    public GameObject menuButtons;
       
    Unit selectedUnit;  

    public int currTurn;
    public int currAction; //0 = Move, 1 = attack
    int unitsDone;
    public bool gameEnd;

    // Start is called before the first frame update
    void Start(){
        gameEnd = false;
        CreateMap();
        CreateUnits();
        currTurn = 1;
        attackButton.SetActive(false);
        StartCoroutine(NexTurn());
    }

    public void NotifyUnitDone() {
        unitsDone++;
        if (unitsDone == units[currTurn].Count) {
            StartCoroutine(NexTurn());
        }
    }

    public void EndTurn() {
        foreach (Unit unit in units[currTurn]) {
            unit.canMove = false;
            unit.canAtk = false;
        }
        StartCoroutine(NexTurn());
    }

    private IEnumerator NexTurn() {
        unitsDone = 0;
        currTurn = (currTurn + 1) % 2;
        string text = "Player's " + (currTurn + 1).ToString() + " turn!";
        nextTurnText.GetComponent<Text>().text = text;
        nextTurnText.SetActive(true);
        foreach(Unit unit in units[currTurn]) {
            unit.canMove = true;
            unit.canAtk = true;
        }
        yield return new WaitForSeconds(2);
        nextTurnText.SetActive(false);
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
            GameObject go = Instantiate(Resources.Load<GameObject>(unitPrefabPaths[piece.pieceName]), map.TileCoordToWorldCoord(piece.posX, piece.posZ) + new Vector3(0,-0.2f,0), Quaternion.identity);
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
            GameObject go = Instantiate(Resources.Load<GameObject>(unitPrefabPaths[piece.pieceName]), map.TileCoordToWorldCoord(piece.posX, piece.posZ) + new Vector3(0, -0.2f, 0), Quaternion.identity);
            go.transform.Rotate(0, 180, 0);
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
        Debug.Log("Attack CLick");
        currAction = 1;
        map.HighlightRangeAroundUnit(selectedUnit.minAtkRange, selectedUnit.maxAtkRange, "Pink", true);
        
    }

    public void KillUnit( Unit victim) {
        int dyingTeam = victim.team;
        unitsMap[victim.tileX, victim.tileZ] = null;
        units[dyingTeam].Remove(victim);
        map.FreeWalkable(victim.tileX, victim.tileZ);
        Destroy(victim.gameObject);

        if (units[dyingTeam].Count <= 0) {
            GameEnd();
        }
    }

    private void GameEnd() {
        string text = "Player's " + (currTurn + 1).ToString() + " victory!";
        nextTurnText.GetComponent<Text>().text = text;
        nextTurnText.SetActive(true);
        gameEnd = true;
        menuButtons.SetActive(true);
    }

    public void HideAttackButton() {
        attackButton.SetActive(false);
    }

    public void Replay() {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void Quit() {
        SceneManager.LoadScene("MainMenu");
    }
}
