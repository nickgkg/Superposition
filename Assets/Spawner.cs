﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spawner : MonoBehaviour
{
    public GameObject[] spawns;

    public GameObject popup;
    public bool spawning;
    public bool spawnTower;
    public bool checkWon;
    public SuperPosition player;

    public bool spawnRandomLayer;
     
    public int gridSize = 0;
    SuperPosition[][] grid;

    bool gridState = true;
    float gridStateChange = 0;

    void Start() {
        if (spawnRandomLayer) {
            float width = 15f;
            for (int i = 0; i < 100; i++) {
                int rand = Random.Range(0, spawns.Length - 1);
                GameObject n = GameObject.Instantiate(spawns[rand],
                        gameObject.transform.position +
                        new Vector3(
                            Random.Range(-width,width),
                            Random.Range(0, 5 * width),
                            Random.Range(-width, width)
                        ),
                        gameObject.transform.rotation);
                n.transform.localScale = new Vector3(
                    Random.Range(0.25f, 2f),
                    Random.Range(0.25f, 2f),
                    Random.Range(0.25f, 2f));
                n.name = spawns[rand].name;
            }
        }
        reset();
    }


    void Update()
    {
        for (int i = 0; i < spawns.Length; i++) {
            string key = "" + (i + 1);
            if (spawning && Random.Range(0, 1000) == 0) {
                Debug.Log("spawning");
                if (!SuperPosition.superPositions.ContainsKey(key)
                        || SuperPosition.superPositions[key].Count == 0) {
                    Debug.Log("spawning " + key);
                    GameObject n = GameObject.Instantiate(spawns[i], gameObject.transform.position, gameObject.transform.rotation);
                    n.name = spawns[i].name;
                    break;
                }
            }
            if (checkWon) {
                bool haveWon = true;
                if (SuperPosition.superPositions.ContainsKey(key)
                        && SuperPosition.superPositions[key].Count > 1) {
                    haveWon = false;
                }
                if (haveWon) {
                    Debug.Log("You Have Won!!");
                }
            }
        }
        const int frameWait = 5;
        if (gridSize > 0) {
            Vector2Int? movement = null;
            if (Input.GetKey(KeyCode.S)) {
                movement = new Vector2Int(0, -1);
            }
            if (Input.GetKey(KeyCode.D)) {
                movement = new Vector2Int(-1, 0);
            }
            if (Input.GetKey(KeyCode.A)) {
                movement = new Vector2Int(1, 0);
            }
            if (Input.GetKey(KeyCode.W)) {
                movement = new Vector2Int(0, 1);
            }
            const int death = 30;
            for (int i = 0; i< gridSize; ++i) {
                for (int j = 0; j< gridSize; ++j) {
                    if (grid[i][j] == null || grid[i][j].gameObject == null) {
                        grid[i][j] = null;
                        if (Time.frameCount % frameWait == 0) {
                            foreachSurrounding(i,j, (sup) => {
                                if (sup == null) {
                                    return false;
                                }
                                if (sup.aliveness > death*1/3 && sup.aliveness < death*2/3) {
                                    GameObject g = spawn(sup.gameObject, i, j);
                                    Vector3 localScale = g.transform.localScale;
                                    localScale.y = 1f;
                                    g.transform.localScale = localScale;
                                    return true;
                                }
                                return false;
                            });
                        }
                    } else if (grid[i][j].key == "player"){
                        if (movement != null && Time.frameCount % frameWait == 0) {
                            Vector2Int v = movement.Value;
                            int i2 = i + v.x;
                            int j2 = j + v.y;     
                            if (i2 < 0 || i2 >= gridSize || j2 < 0 || j2 >= gridSize) {
                                continue;
                            }
                            if (grid[i2][j2] != null) {
                                lost();
                                return;
                            }
                            bool justLost = foreachSurrounding(i,j, (sup) => {
                                return sup != null;
                            });
                            if (justLost) {
                                lost();
                                return;
                            }
                            grid[i2][j2] = grid[i][j];
                            updatePos(i2, j2);
                            grid[i][j] = null;
                            movement = null;
                        }
                    } else {
                        if (Time.frameCount % frameWait == 0 && !grid[i][j].dying) {
                            grid[i][j].aliveness++;
                            if (grid[i][j].aliveness > death) {
                                GameObject.Destroy(grid[i][j].gameObject);
                                grid[i][j] = null;
                                continue;
                            }

                            foreachSurrounding(i,j, (sup) => {
                                if (sup == null) {
                                    return false;
                                }
                                if (sup.key != grid[i][j].key) {
                                    foreach (var sup2 in new SuperPosition[]{sup, grid[i][j]})
                                    {
                                        List<SuperPosition> sups = SuperPosition.superPositions[sup2.key];
                                        var supSup = sups[Random.Range(0, sups.Count)];
                                        supSup.CollapseOthers();
                                    }
                                }
                                return false;
                            });
                        }
                        float fractionToNext = Time.frameCount%frameWait/(float)frameWait;
                        grid[i][j].gameObject.transform.localScale = new Vector3(1, 0.5f + Mathf.Sin((grid[i][j].aliveness + fractionToNext)/death * Mathf.PI), 1);
                    }
                }
            }
            //gridState = !gridState;
        }

    }

    private GameObject spawn(GameObject g, int i, int j) {
        GameObject n = GameObject.Instantiate(g);
        n.name = g.name;
        grid[i][j] = n.GetComponent<SuperPosition>();
        grid[i][j].aliveness = 0;
        updatePos(i, j);
        return n;
    }

    private void updatePos(int i, int j) {
        float offset = Mathf.Sqrt(50) / 2;
        float x = - offset * (i - gridSize /2) + offset * (j - gridSize/2);
        float z = offset * (i - gridSize /2) + offset * (j - gridSize/2); 
        grid[i][j].gameObject.transform.position = new Vector3(x, 0, z);
    }

    private void lost(){
        //popup.SetActive(true);
    }

    private void reset() {
        if (gridSize > 0) {
            if (grid != null) {
                for(int i = 0; i< gridSize; ++i) {
                    for(int j = 0; j< gridSize; ++j) {
                        if (grid[i][j] != null) {
                            GameObject.Destroy(grid[i][j].gameObject);
                        }
                    }
                }
            }
            grid = new SuperPosition[gridSize][];

            for(int i = 0; i< gridSize; ++i) {
                grid[i] = new SuperPosition[gridSize];
                for(int j = 0; j< gridSize; ++j) {
                    if (Random.Range(0, 100) == 0) {
                        spawn(spawns[Random.Range(0, spawns.Length)], i, j);
                        grid[i][j].key = "" + Random.Range(100, 100000);
                    }
                }
            }

            grid[gridSize/2][gridSize/2] = player;
            updatePos(gridSize/2,gridSize/2);
        }
    }

    private bool foreachSurrounding(int i, int j, System.Func<SuperPosition, bool> f) {
        foreach(Vector2Int offset in new Vector2Int[]{
                new Vector2Int(0, -1),
                new Vector2Int(0, 1),
                new Vector2Int(1, 0),
                new Vector2Int(-1, 0)}){
            int i2 = i + offset.x;
            int j2 = j + offset.y;
            if (i2 < 0 || i2 >= gridSize || j2 < 0 || j2 >= gridSize) {
                continue;
            }
            if (f(grid[i2][j2])) {
                return true;
            }
        }
        return false;
    }

}
