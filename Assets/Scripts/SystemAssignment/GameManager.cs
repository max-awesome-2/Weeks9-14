using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using Unity.VisualScripting;
using Unity.VisualScripting.Antlr3.Runtime;
using UnityEngine;
using UnityEngine.Events;

public class GameManager : MonoBehaviour
{
    // 0 = title screen, 1 = tutorial, 2 = placement phase, 3 = wave phase
    int gameState = 2;
    private bool placingGems = false;

    // round tracking
    public int finalRound = 20;
    int roundNumber = 1;

    // player stats & upgrade tracking
    int gold = 0;
    int upgradeGold = 20;
    int upgradeLevel = 1;

    // grid stuff
    // in the int grid - 0 = empty space, 1 = path, 2 = flagstone, 3+ = waypoints
    int[][] grid;
    public int gridSize = 24;
    public GameObject tilePrefab, towerPrefab;
    public Sprite grassSprite, pathSprite, flagstoneSprite;

    // ui items
    public GameObject placeGemsButton;

    // pathfinding
    Pathfinding pathfinder;
    List<int[]> waypointCoords;

    // tower placement
    // we need a list to keep towers that have been placed this placement phase,
    // then another list to hold all kept towers - this is necessary in order to figure out opal buffs
    List<Tower> placedTowers, keptTowers;
    public GameObject gemPlacementGraphic;

    // sfx
    AudioSource audioSource;
    public AudioClip enemyDeathClip;

    // hover info box
    public GameObject hoverBox;
    public TextMeshProUGUI hoverBoxText;

    // tower sprite stuff
    public Sprite[] gemTierSprites;
    public Sprite rockSprite;
    public Color[] gemTypeColors;
    // let's say 0 is emerald, 1 is sapphire, 2 is amethyst, 3 is diamond, 4 is topaz, 5 is aquamarine, 6 is opal, 7 is ruby

    // tower stats
    public float baseTowerDamage = 5;
    public float[] gemTypeBaseDamage;
    public float[] gemTypeBaseRange;
    public float[] gemTypeBaseAtkSpeed;
    public float basePoisonDPS = 5;
    public float poisonSlowMultiplier = 0.7f;
    public float freezeSlowMultiplier = 0.6f;
    public float poisonTime = 4, freezeTime = 4;

    public float opalBaseASRatio = 1.15f;
    public float opalASRatioIncPerTier = 0.1f;

    // enemy stats
    public float baseEnemyHealth; // base enemy health
    public int enemyCount = 20;
    private List<Enemy> enemyList; // contains all spawned enemies

    // time
    private bool paused = false;
    private float gameTimeScale = 1;
    bool fastForwarding = false;

    // upgrades
    private float[] tierChances = new float[] { 100, 0, 0, 0, 0 };
    private float[][] tierChanceUpgradeLevels;

    // tiles
    public Vector3 topLeftTileCorner;
    public float tileSize;
    private Vector3 tileHoverMinBounds, tileHoverMaxBounds;
    private float oneTileScreenSize; // size of 1 2x tile in screen units

    // scaling
    // the b value in y = a (b/a)^x
    public float statScalingExponentialHeight = 6.5f;

    // multiplier for how enemy health scales in relation to tower stats - they should scale sliiightly more
    public float enemyToTowerScalingRatio = 1.1f;

    // unityevents
    private UnityEvent onRoundStart, onRoundEnd;

    // debug stuff
    private List<Vector3> pathfindingPath;

    void Start()
    {
        tileHoverMinBounds = Grid2xToPhysicalPos(0, 0) - Vector3.right * tileSize / 4 - Vector3.up * tileSize / 4;
        tileHoverMaxBounds = Grid2xToPhysicalPos(gridSize * 2 - 1, gridSize * 2 - 1) + Vector3.right * tileSize / 4 + Vector3.up * tileSize / 4;
        tileHoverMinBounds = Camera.main.WorldToScreenPoint(tileHoverMinBounds);
        tileHoverMaxBounds = Camera.main.WorldToScreenPoint(tileHoverMaxBounds);
        oneTileScreenSize = (tileHoverMaxBounds.x - tileHoverMinBounds.x) / (gridSize * 2);

        print("one tile screen size: " + oneTileScreenSize);

        audioSource = GetComponent<AudioSource>();
        pathfinder = GetComponent<Pathfinding>();

        // initialize upgrade level chances
        tierChanceUpgradeLevels = new float[][]
        {
            new float[]
            {
                100, 0, 0, 0, 0
            },
            new float[]
            {
                50, 50, 0, 0, 0
            },
            new float[]
            {
                20, 50, 30, 0, 0
            },
            new float[]
            {
                0, 30, 40, 30, 0
            },
            new float[]
            {
                0, 0, 0, 50, 50
            }

        };

        //initialize grid with map shape
        grid = new int[gridSize][];

        //grid[0] = new int[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
        //grid[1] = new int[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
        //grid[2] = new int[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
        //grid[3] = new int[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
        //grid[4] = new int[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
        //grid[5] = new int[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
        //grid[6] = new int[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
        //grid[7] = new int[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
        //grid[8] = new int[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
        //grid[9] = new int[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
        //grid[10] = new int[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
        //grid[11] = new int[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
        //grid[12] = new int[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
        //grid[13] = new int[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
        //grid[14] = new int[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
        //grid[15] = new int[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
        //grid[16] = new int[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
        //grid[17] = new int[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
        //grid[18] = new int[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
        //grid[19] = new int[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
        //grid[20] = new int[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
        //grid[21] = new int[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
        //grid[22] = new int[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
        //grid[23] = new int[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };

        grid[0] = new int[]   { 0, 0, 0, 0, 3, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
        grid[1] = new int[]   { 0, 0, 0, 0, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
        grid[2] = new int[]   { 0, 0, 0, 0, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
        grid[3] = new int[]   { 0, 0, 0, 0, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
        grid[4] = new int[]   { 0, 0, 0, 0, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
        grid[5] = new int[]   { 0, 0, 0, 0, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
        grid[6] = new int[]   { 0, 0, 0, 0, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
        grid[7] = new int[]   { 0, 0, 0, 0, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
        grid[8] = new int[]   { 0, 0, 0, 0, 4, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 5, 0, 0, 0, 0, 0, 0, 0, 0 };
        grid[9] = new int[]   { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 0, 0, 0, 0, 0, 0, 0, 0 };
        grid[10] = new int[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 0, 0, 0, 0, 0, 0, 0, 0 };
        grid[11] = new int[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 0, 0, 0, 0, 0, 0, 0, 0 };
        grid[12] = new int[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 0, 0, 0, 0, 0, 0, 0, 0 };
        grid[13] = new int[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 0, 0, 0, 0, 0, 0, 0, 0 };
        grid[14] = new int[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 7, 1, 1, 1, 1, 1, 6, 0, 0, 0, 0, 0, 0, 0, 0 };
        grid[15] = new int[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
        grid[16] = new int[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
        grid[17] = new int[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
        grid[18] = new int[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
        grid[19] = new int[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 8, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
        grid[20] = new int[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
        grid[21] = new int[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
        grid[22] = new int[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
        grid[23] = new int[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };

        // loop through grid and spawn tiles at each grid location

        int[][] newGrid = new int[gridSize * 2][];

        // gets the list of top-left waypoint coordinates in the gridsize * 2 system
        List<int[]> waypointCoords = new List<int[]>();

        // use for loops to expand each cell into a 2x2 square, doubling grid size(towers take up a 2x2 square, but this allows them to be placed at offsets of 0.5 spaces)
        // also scan through grid and add all waypoint coordinates(in order) to waypointCoords
        for (int x = 0; x < gridSize; x++)
        {
            Vector3 rowPos = topLeftTileCorner + Vector3.down * x * tileSize;

            newGrid[x * 2] = new int[gridSize * 2];
            newGrid[x * 2 + 1] = new int[gridSize * 2];


            for (int y = 0; y < gridSize; y++)
            {
                int cornerVal = grid[x][y];
                int val = cornerVal;

                Sprite tileSprite;

                Color tileColor;

                if (cornerVal >= 2)
                {
                    tileSprite = flagstoneSprite;
                    tileColor = Color.black;
                }
                else if (cornerVal == 0)
                {
                    tileSprite = grassSprite;
                    tileColor = Color.green;
                }
                else
                {
                    tileSprite = pathSprite;
                    tileColor = Color.gray;
                }

                // instantiate tile and set sprite
                Vector3 gridPos = rowPos + Vector3.right * tileSize * y;
                GameObject newTile = Instantiate(tilePrefab, gridPos, Quaternion.identity);
                newTile.transform.localScale = Vector3.one * tileSize;
                //newTile.GetComponent<SpriteRenderer>().sprite = tileSprite;
                newTile.GetComponent<SpriteRenderer>().color = tileColor; // REMOVE THIS


                if (cornerVal > 2)
                {
                    int waypointIndex = cornerVal - 3;

                    // extend waypoint list to the current waypoint if it's not long enough yet to accomodate it
                    if (waypointCoords.Count < waypointIndex + 1)
                    {
                        int increaseBy = waypointIndex + 1 - waypointCoords.Count;
                        for (int i = 0; i < increaseBy; i++)
                        {
                            // extend it with dummy value
                            waypointCoords.Add(null);
                        }
                    }

                    // set coords of waypoint in list
                    waypointCoords[waypointIndex] = new int[] { x * 2, y * 2 };

                    val = 2;
                }

                // replace waypoints with flagstone except for the topleft corner

                newGrid[x * 2][y * 2] = cornerVal;
                newGrid[x * 2 + 1][y * 2] = val;

                newGrid[x * 2][y * 2 + 1] = val;
                newGrid[x * 2 + 1][y * 2 + 1] = val;
            }
        }

        // overwrite the original grid with the expanded grid
        grid = newGrid;

        pathfinder.waypoints = waypointCoords;

        pathfindingPath = pathfinder.GetPath(grid, false);
    }

    public void OnEnemyKilled()
    {
        //gold += (gold etc.)
        gold += 3 + Mathf.FloorToInt(roundNumber * 1.5f);
        audioSource.PlayOneShot(enemyDeathClip);
    }

    public void OnPlaceGemsButtonClicked()
    {
        placingGems = true;
        placeGemsButton.SetActive(false);
    }

    // called when the gamestate / phase changes, handles all logic that happens at the beginning of a phase
    private void ChangePhase(int phase)
    {
        gameState = phase;
        if (phase == 0)
        {
            placeGemsButton.SetActive(true);

            placedTowers.Clear();


        }
        else
        {
            //if roundNumber % 5 == 0, it’s a flying enemy round – communicate this via bool parameter in the path generation function call
            bool flyingRound = roundNumber % 5 == 0;

            //use pathfinder to generate enemy path based on grid and waypointCoords
            List<Vector3> path = pathfinder.GetPath(grid, flyingRound);

            // cache it for debug
            pathfindingPath = path;


        }
    }

    public Vector3 GridToPhysicalPos(int x, int y)
    {
        return topLeftTileCorner + Vector3.right * x * tileSize / 2 - Vector3.up * y * tileSize / 2;
    }

    public Vector3 Grid2xToPhysicalPos(int x, int y)
    {
        return topLeftTileCorner + Vector3.right * (x * tileSize / 2 - tileSize/4) - Vector3.up * (y * tileSize / 2 - tileSize / 4);
    }

    private int[] GetGridAtMousePos()
    {
        int[] returnCoords = new int[] { -1, -1 };

        Vector3 mouseGridCoords = Input.mousePosition;

        mouseGridCoords = mouseGridCoords - new Vector3(tileHoverMinBounds.x, tileHoverMaxBounds.y, 0);

        returnCoords[0] = Mathf.FloorToInt(mouseGridCoords.x / oneTileScreenSize);
        returnCoords[1] = gridSize * 2 - 1 - Mathf.CeilToInt(mouseGridCoords.y / oneTileScreenSize);

        if (returnCoords[0] < 0 || returnCoords[0] >= gridSize * 2 || returnCoords[1] < 0 || returnCoords[1] >= gridSize * 2) return null;

        return returnCoords;
    }

    private void PlaceTower()
    {
        //use getgridatmouse() to get the grid the mouse is hovering over
        // (this is the top left corner of where the tower will be placed, so make sure to check the other three squares to make sure we can put the tower here)
        int[] grCoords = GetGridAtMousePos();

        if (grCoords == null)
        {
            print("invalid coords - off grid");
            return;
        }

        // create modified grid
        int[][] modifiedGrid = new int[gridSize * 2][];

        for (int x = 0; x < gridSize * 2; x++)
        {
            for (int y = 0; y < gridSize * 2; y++)
            {
                modifiedGrid[x][y] = grid[x][y];
            }
        }

        // check for invalid coordinates, i.e. flagstone or other towers
        // also set the modified (hypothetical) grid's values for where the new tower will be placed to impassable tiles
        // the hypothetical grid will then be used to check if the path is blocked by the new tower
        for (int x = grCoords[0]; x < grCoords[0] + 2; x++)
        {
            for (int y = grCoords[1]; y < grCoords[1] + 2; y++)
            {
                if (x < 0 || y < 0 || x >= gridSize || y >= gridSize)
                {
                    print("invalid coords - goes off screen");
                    return;
                }

                int valAtCoords = grid[x][y];
                if (valAtCoords >= 2)
                {
                    print("invalid coords - placed on flagstone");
                }
                else if (valAtCoords == -1)
                {
                    print("invalid coords - placed on other tower");
                }

                // set hypothetical grid value
                modifiedGrid[x][y] = -1;
            }
        }

        //check grid to see if it is a valid location for placement
        //first, check if it’s blocked by another tower or flagstone
        //then, if it’s not, use Pathfinder to attempt to generate a path between all waypoints

        List<Vector3> pathPoints = pathfinder.GetPath(modifiedGrid, false);

        if (pathPoints == null)
        {
            print("invalid coords: blocks enemies!");
            return;
        }

        //if placement is not valid, spawn an instance of FloatingText at the mouse

        //if it’s blocked by another tower or the player tried to place on flagstone, set the floatingtext’s text to ‘cannot place here!’
        //if the placement would block the enemy’s path, change the text to ‘must leave path for enemies!’

        //instantiate tower from prefab
        GameObject newTower = Instantiate(towerPrefab);

        //add instantiated tower’s Tower script to placedTowers
        Tower t = newTower.GetComponent<Tower>();
        placedTowers.Add(t);

        //block off the new tower’s grid positions in grid
        for (int x = grCoords[0]; x < grCoords[0] + 2; x++)
        {
            for (int y = grCoords[1]; y < grCoords[1] + 2; y++)
            {
                grid[x][y] = -1;
            }
        }

        //get a randomly generated type and tier to assign to the new gem
        int gemType = Random.Range(0, gemTypeColors.Length);
        float gemTierVal = Random.value * 100f;

        float chanceTotal = 0;
        int gemTier = -1;
        // to get gem tier:
        for (int i = 0; i < tierChances.Length; i++)
        {
            chanceTotal += tierChances[i];

            if (gemTierVal <= chanceTotal)
            {
                gemTier = i;
                break;
            }
        }

        t.x = grCoords[0];
        t.y = grCoords[1];

        // set tower's stats
        SetTowerStats(t, gemTier, gemType);

    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            int[] smda = GetGridAtMousePos();
            if (smda != null) print("grid at mouse: " + smda[0] + ", " + smda[1]);
        }

        if (gameState == 0 && placingGems && placedTowers.Count == 5)
        {


            if (placedTowers.Count == 5)
            {
                if (Input.GetMouseButtonDown(1))
                {
                    int[] coords = GetGridAtMousePos();

                    print("right click during placement");
                    if (coords != null)
                    {

                        //loop through placedTowers and check if the grid position matches any of their grid positions
                        for (int i = 0; i < placedTowers.Count; i++)
                        {
                            Tower t = placedTowers[i];

                            // tower's stored grid position is its top left corner, so make sure to check intersection with its other 3 spaces
                            int gX = t.x, gY = t.y;
                            if (coords[0] >= gX && coords[0] <= (gX + 1) && coords[1] >= gY && coords[1] <= (gY + 1))
                            {
                                // if it matches one, call OnKeepTower(index of the tower)
                                OnKeepTower(i);
                                print("KEEPING TOWER AT INDEX " + i);
                            }
                        }

                    }
                    else
                    {
                        // nothing happens
                    }
                }

            }
            else
            {
                // move the tower placement graphic to the mouse pos
                int[] coords = GetGridAtMousePos();
                if (coords != null)
                    gemPlacementGraphic.transform.position = Grid2xToPhysicalPos(coords[0], coords[1]);

            }
        }

        if (hoverBox.activeSelf)
        {
            //set hoverbox position to mouse position
            hoverBox.transform.position = Input.mousePosition;
        }
    }

    /// <summary>
    /// Sets a tower's stats based on its gem type and tier. Called once when a tower is initially generated, and again if it is combine upgraded when kept.
    /// </summary>
    private void SetTowerStats(Tower t, int gemTier, int gemType)
    {
        // calculate the gem’s stats based on its type and tier
        float dmg = baseTowerDamage * gemTypeBaseDamage[gemType];
        float range = 1 * gemTypeBaseRange[gemType];
        float atkSpeed = 1 * gemTypeBaseAtkSpeed[gemType];

        float tierRatio = GetTierStatRatio(gemTier);
        dmg *= tierRatio;
        // TO CONSIDER: make range and attack speed scale linearly

        //range *= tierRatio; 
        //atkSpeed *= tierRatio;

        // initialize the gem with its stats
        t.InitTower(gemType, gemTier, dmg, range, atkSpeed);

        if (gemType == 0)
        {
            t.SetPoisonStats(basePoisonDPS * tierRatio, poisonTime, poisonSlowMultiplier);
        }
        else if (gemType == 1)
        {
            t.SetFreezeStats(freezeSlowMultiplier, freezeTime);
        }
        else if (gemType == 6)
        {
            t.SetOpalStats(opalBaseASRatio + opalASRatioIncPerTier * gemTier);
        }

        //use getcomponent to get new tower’s spriterenderer
        SpriteRenderer towerRen = t.GetComponent<SpriteRenderer>();

        // set the spriterenderer’s sprite and color according to the tier and color of the gem
        towerRen.color = gemTypeColors[gemType];
        towerRen.sprite = gemTierSprites[gemTier];
    }

    void OnKeepTower(int keptIndex)
    {

        //     for towers in placedTowers:
        for (int i = 0; i < placedTowers.Count; i++)
        {
            Tower t = placedTowers[i];

            if (i != keptIndex)
            {
                //         set tower’s sprite to rock sprite and tower’s spriterenderer color to white
                t.OnNotKept();
                SpriteRenderer sr = t.GetComponent<SpriteRenderer>();
                sr.color = Color.white;
                sr.sprite = rockSprite;
            }
            else
            {
                bool combineUpgrade = false;

                // first, look through to see if it's combineable
                for (int j = 0; j < placedTowers.Count; j++)
                {
                    Tower t2 = placedTowers[j];
                    if (j != i && t2.gemType == t.gemType && t2.gemTier == t.gemTier && t.gemTier != gemTierSprites.Length - 1)
                    {
                        // COMBINEABLE - upgrade tier of the new one
                        combineUpgrade = true;
                        break;
                    }
                }

                // if the gem is comineable, upgrade its tier by 1 and reset its stats
                if (combineUpgrade)
                    SetTowerStats(t, t.gemTier + 1, t.gemType);

                // initialize the tower, passing in the gamemanager’s onroundstart and onroundend events
                t.OnKept(onRoundStart, onRoundEnd, enemyList);
                keptTowers.Add(t);

                // apply opal bonuses from all existing opal towers that this new tower is in range of
                for (int j = 0; j < keptTowers.Count; j++)
                {
                    Tower t2 = keptTowers[j];
                    if (t2.gemType == 6 && Vector3.Distance(t.transform.position, t2.transform.position) <= t2.range)
                    {
                        t.AddOpalRatio(t2.opalBonus);
                    } 

                    // make sure that, if this new tower is an opal tower, also add our opal bonus to the existing towers that are in range
                    if (t.gemType == 6 && Vector3.Distance(t.transform.position, t2.transform.position) <= t.range)
                    {
                        t2.AddOpalRatio(t.opalBonus);
                    }
                }
            }
        }

        ChangePhase(1);
    }

    // manage hover info box
    public void OnHoverObjectOfInterest(string hoverInfo)
    {
        hoverBox.SetActive(true);
        hoverBoxText.text = hoverInfo;
    }

    public void OnMouseExitObjectOfInterest()
    {
        hoverBox.SetActive(false);
    }

    public void OnPauseButtonPressed()
    {
        paused = !paused;
        gameTimeScale = paused ? 0 : (fastForwarding ? 3 : 1);
    }

    public void UpgradeButtonPressed()
    {
        if (gold < upgradeGold) return;
        gold -= upgradeGold;
        upgradeLevel++;

        for (int i = 0; i < 5; i++)
        {
            tierChances[i] = tierChanceUpgradeLevels[i][upgradeLevel];
        }
    }

    private float GetTierStatRatio(int tier)
    {
        // calculate x value to evaluate the exponential curve at
        int maxTier = gemTierSprites.Length;
        float getPointAtX = (1f / (maxTier - 1)) * tier;

        return GetScaleRatio(statScalingExponentialHeight, getPointAtX);
    }

    private float GetEnemyStatRatio(int round)
    {
        float roundRatio = round * (1f / (finalRound - 1));

        return GetScaleRatio(statScalingExponentialHeight * enemyToTowerScalingRatio, roundRatio);
    }

    private float GetScaleRatio(float xAtOneIntercept, float evaluationX)
    {
        float bonusRatio = 0.1f * Mathf.Pow(xAtOneIntercept / 0.1f, evaluationX);

        return 1 + bonusRatio;
    }

    // gizmos for debug stuff
    private void OnDrawGizmos()
    {
        if (pathfindingPath != null)
        {
            Vector3 last = pathfindingPath[0];
            
            for (int i = 1; i < pathfindingPath.Count; i++)
            {
                Vector3 next = pathfindingPath[i];

                Gizmos.color = Color.red;
                Gizmos.DrawLine(last, next);

                last = next;
            }
        }

        Gizmos.color = Color.blue;
        Gizmos.DrawSphere(Camera.main.ScreenToWorldPoint(tileHoverMinBounds), 0.05f);
        Gizmos.color = Color.green;
        Gizmos.DrawSphere(Camera.main.ScreenToWorldPoint(tileHoverMaxBounds), 0.05f);

    }
}
