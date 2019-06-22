using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Spawner : MonoBehaviour
{
    public SuperPosition[] spawns;

    public GameObject popup;
    public Text timeDisplay;
    public Text popupTimeDisplay;
    public bool spawning;
    public bool spawnTower;
    public bool checkWon;
    public SuperPosition player;

    public bool spawnRandomLayer;
     
    public int gridSize = 0;
    SuperPosition[][] grid;

    bool gridState = true;
    float gridStateChange = 0;
    private float timer;
    void Start() {
    }


    void Update()
    {
        const int frameWait = 5;
        if (gridSize > 0) {
            if (Input.GetKeyDown(KeyCode.R)) {
                reset();
                return;
            }
            if (grid == null) {
                return;
            }
            Vector2Int? movement = null;
            if (playerEnabled) {
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
            }
            const int death = 30;
            for (int i = 0; i< gridSize; ++i) {
                for (int j = 0; j< gridSize; ++j) {
                    if (grid[i][j] == null || grid[i][j].gameObject == null || grid[i][j].dying) {
                        grid[i][j] = null;
                        if (Time.frameCount % frameWait == 0) {
                            string keyFound = "";
                            int surroundingLiveness = 0;
                            SuperPosition lastSup = null;

                            foreachSurrounding(i,j, (sup) => {
                                if (sup == null) {
                                    return false;
                                }
                                if (!sup.dying && sup.aliveness < death*2/3) {
                                    if (lastSup == null || keyFound == sup.key) {
                                        surroundingLiveness += sup.aliveness;
                                        keyFound = sup.key;
                                        lastSup = sup;
                                    }
                                }
                                return false;
                            });
                            if (lastSup != null && surroundingLiveness > death*1/3) {
                                GameObject g = spawn(lastSup, i, j, null);
                                Vector3 localScale = g.transform.localScale;
                                localScale.y = 0.0001f;
                                g.transform.localScale = localScale;
                            }
                        }
                    } else if (grid[i][j].key == "player"){
                        if (Time.frameCount % frameWait == 0) {
                            if (movement != null) {
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
                                grid[i2][j2] = grid[i][j];
                                updatePos(i2, j2);
                                playerPos = new Vector2Int(i2, j2);
                                grid[i][j] = null;
                                movement = null;
                            }
                            bool justLost = foreachSurrounding(i,j, (sup) => {
                                return sup != null && sup.aliveness > death/3;
                            });
                            if (justLost) {
                                lost();
                                return;
                            }
                        }
                    } else {
                        if (Time.frameCount % frameWait == 0 && !grid[i][j].dying) {
                            grid[i][j].aliveness++;
                            if (grid[i][j].aliveness > death) {
                                cleanup(grid[i][j]);
                                grid[i][j].dying = true;
                                grid[i][j] = null;
                                continue;
                            }

                            foreachSurrounding(i,j, (sup) => {
                                if (sup == null) {
                                    return false;
                                }
                                if (sup.key != grid[i][j].key && sup.gameObject != null && sup.aliveness > death*2/3) {
                                    foreach (var sup2 in new SuperPosition[]{sup, grid[i][j]})
                                    {
                                        List<SuperPosition> sups = SuperPosition.superPositions[sup2.key];
                                        var supSup = sups[Random.Range(0, sups.Count)];
                                        supSup.CollapseOthers();
                                        supSup.aliveness = 0;
                                    }
                                }
                                return false;
                            });
                        }
                        float fractionToNext = Time.frameCount%frameWait/(float)frameWait;
                        grid[i][j].gameObject.transform.localScale = new Vector3(1, 0.001f + 2 * Mathf.Sin((grid[i][j].aliveness + fractionToNext)/death * Mathf.PI), 1);
                    }
                }
            }
            //gridState = !gridState;
        }

        if (playerEnabled) {
            float oldTimer = timer;
            timer += Time.deltaTime;
            // Spawn rate in seconds
            for (int i = (int)(oldTimer/spawnRate); i < (int)(timer/spawnRate); ++i) {
                spawnRandom();
            }
            timeDisplay.text = myFormat(timer);
        }
    }
    public float spawnRate = 10;

    private string myFormat(float f) {
        return "" + (int)f + "." + (int)(f*10%10) + (int)(f*100%10);
    }

    Vector2Int playerPos = new Vector2Int(50, 50);

    public static Dictionary<int, LinkedList<GameObject>> pool = new Dictionary<int, LinkedList<GameObject>>();
    private GameObject spawn(SuperPosition sup, int i, int j, string key) {
        GameObject g = sup.gameObject;
        GameObject n;
        if (pool.ContainsKey(sup.pool) && pool[sup.pool].Count > 0) {
            n = pool[sup.pool].Last.Value;
            pool[sup.pool].RemoveLast();
            n.SetActive(true);
        } else {
            n = GameObject.Instantiate(g);
        }
        n.name = g.name;
        n.transform.localScale = new Vector3(1, 0.0001f, 1f);
        grid[i][j] = n.GetComponent<SuperPosition>();
        grid[i][j].aliveness = 0;
        grid[i][j].dying = false;
        if (key != null) {
            grid[i][j].key = key;
        } else {
            grid[i][j].key = sup.key;
        }
        grid[i][j].register();
        updatePos(i, j);
        return n;
    }
    public static void cleanup(SuperPosition sup) {
        if (sup.gameObject == null)
        {
            //Debug.Log("failed to find gameobject");
            return;
        }
        sup.gameObject.SetActive(false);
        if (!Spawner.pool.ContainsKey(sup.pool))
        {
            Spawner.pool[sup.pool] = new LinkedList<GameObject>();
        }
        Spawner.pool[sup.pool].AddLast(sup.gameObject);
    }
    private void updatePos(int i, int j) {
        float offset = Mathf.Sqrt(50) / 2;
        float x = - offset * (i - gridSize /2) + offset * (j - gridSize/2);
        float z = offset * (i - gridSize /2) + offset * (j - gridSize/2); 
        grid[i][j].gameObject.transform.position = new Vector3(x, 0, z);
    }

    private void lost(){
        popup.SetActive(true);
        popupTimeDisplay.text = "Score: " + myFormat(timer) + " Seconds";
        playerEnabled = false;
    }

    bool playerEnabled = false;

    private void spawnRandom() {
        Debug.Log("spawning");
        for (int attemptCount = 0; attemptCount < 20; attemptCount++) {
            int i = Random.Range(0, gridSize);
            int j = Random.Range(0, gridSize);
            if (Mathf.Abs(i - playerPos.x) + Mathf.Abs(j - playerPos.y) > 5) {
                if (grid[i][j] == null) {
                    spawn(spawns[Random.Range(0, spawns.Length)], i, j,
                        "" + Random.Range(100, 100000)
                    );
                    Debug.Log("succeeded");
                    return;
                }
            }
        }
    }

    public void reset() {
        popup.SetActive(false);
        playerEnabled = true;
        timer = 0f;
        if (gridSize > 0) {
            if (grid != null) {
                for(int i = 0; i< gridSize; ++i) {
                    for(int j = 0; j< gridSize; ++j) {
                        if (grid[i][j] != null && grid[i][j] != player) {
                            grid[i][j].CollapseSelf();
                        }
                    }
                }
            }
            grid = new SuperPosition[gridSize][];

            for(int i = 0; i< gridSize; ++i) {
                grid[i] = new SuperPosition[gridSize];
                for(int j = 0; j< gridSize; ++j) {
                    if (Random.Range(0, 100) == 0) {
                        spawn(spawns[Random.Range(0, spawns.Length)], i, j, "" + Random.Range(100, 100000));
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
