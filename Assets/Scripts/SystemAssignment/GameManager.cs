using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using Unity.VisualScripting.Antlr3.Runtime;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    // 0 = title screen, 1 = tutorial, 2 = placement phase, 3 = wave phase
    int gameState = 2;
    private bool placingGems = false;

    // round tracking
    int roundNumber = 1;

    // player stats & upgrade tracking
    int gold = 0;
    int round = 1;
    int upgradeGold = 20;
    int upgradeLevel = 1;


    // grid stuff
    // in the int grid - 0 = empty space, 1 = path, 2 = flagstone, 3+ = waypoints
    int[][] grid;
    int gridSize = 24;
    public GameObject tilePrefab, towerPrefab;
    public Sprite grassSprite, pathSprite, flagstoneSprite;

    // ui items
    public GameObject placeGemsButton;

    // pathfinding
    Pathfinding pathfinder;
    List<int[]> waypointCoords;

    // tower placement
    List<Tower> placedTowers;
    public GameObject gemPlacementGraphic;

    // sfx
    AudioSource audioSource;
    public AudioClip enemyDeathClip;

    // hover info box
    public GameObject hoverBox;
    public TextMeshProUGUI hoverBoxText;

    // tower sprite stuff
    public Sprite[] gemTierSprites;
    public Color[] gemTypeColors;
    // let's say 0 is emerald, 1 is sapphire, 2 is amethyst, 3 is diamond, 4 is topaz, 5 is aquamarine, 6 is opal

    // tower stats
    public float[] gemTypeBaseDamage;
    public float[] gemTypeBaseRange;
    public float[] gemTypeBaseAtkSpeed;
    public float basePoisonDPS = 5;
    public float poisonSlowMultiplier = 0.7f;
    public float freezeSlowMultiplier = 0.6f;
    public float poisonTime = 4, freezeTime = 4;


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


    void Start()
    {
        tileHoverMinBounds = topLeftTileCorner - (Vector3.right * tileSize / 2 - Vector3.up * gridSize * tileSize) - (Vector3.up * tileSize / 2);
        tileHoverMaxBounds = topLeftTileCorner + (Vector3.up * tileSize / 2 + Vector3.right * gridSize * tileSize) + (Vector3.right * tileSize / 2);
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

        grid[0] = new int[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
        grid[1] = new int[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
        grid[2] = new int[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
        grid[3] = new int[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
        grid[4] = new int[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
        grid[5] = new int[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
        grid[6] = new int[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
        grid[7] = new int[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
        grid[8] = new int[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
        grid[9] = new int[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
        grid[10] = new int[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
        grid[11] = new int[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
        grid[12] = new int[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
        grid[13] = new int[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
        grid[14] = new int[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
        grid[15] = new int[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
        grid[16] = new int[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
        grid[17] = new int[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
        grid[18] = new int[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
        grid[19] = new int[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
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
                if (cornerVal >= 2) tileSprite = flagstoneSprite;
                else if (cornerVal == 0) tileSprite = grassSprite;
                else tileSprite = pathSprite;

                // instantiate tile and set sprite
                Vector3 gridPos = rowPos + Vector3.right * tileSize * y;
                GameObject newTile = Instantiate(tilePrefab, gridPos, Quaternion.identity);
                newTile.GetComponent<SpriteRenderer>().sprite = tileSprite;


                if (cornerVal > 2)
                {
                    int waypointIndex = cornerVal - 3;

                    // extend waypoint list to the current waypoint if it's not long enough yet to accomodate it
                    if (waypointCoords.Count < waypointIndex + 1)
                    {
                        for (int i = 0; i < waypointIndex + 1 - waypointCoords.Count; i++)
                        {
                            // extend it with dummy value
                            waypointCoords.Add(new int[2]);
                        }
                    }

                    // set coords of waypoint in list
                    waypointCoords[waypointIndex] = new int[] { x, y };

                    val = 2;
                }

                // replace waypoints with flagstone except for the topleft corner

                newGrid[x * 2][y * 2] = cornerVal;
                newGrid[x * 2 + 1][y * 2] = val;

                newGrid[x * 2][y * 2 + 1] = val;
                newGrid[x * 2 + 1][y * 2 + 1] = val;
            }
        }


        pathfinder.waypoints = waypointCoords;


    }

    public void OnEnemyKilled()
    {
        //gold += (gold etc.)
        gold += 3 + Mathf.FloorToInt(round * 1.5f);
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


        }
    }

    private Vector3 Grid2xToPhysicalPos(int x, int y)
    {
        return topLeftTileCorner + Vector3.right * x * tileSize - Vector3.up * y * tileSize;
    }

    private int[] GetGridAtMousePos()
    {
        int[] returnCoords = new int[] { -1, -1 };

        Vector2 mouseGridCoords = Input.mousePosition - tileHoverMinBounds;

        returnCoords[0] = Mathf.FloorToInt(mouseGridCoords.x / oneTileScreenSize);
        returnCoords[1] = 1 - Mathf.CeilToInt(mouseGridCoords.y / oneTileScreenSize);

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

        List<Vector3> pathPoints = pathfinder.GetPath(modifiedGrid);

        if (pathPoints.Count == 0)
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

        // get stats from stat formula
        float dmg = 1;
        float range = 1;
        float atkSpeed = 1;

        t.InitTower(grCoords[0], grCoords[1], gemType, 1, 1, 1);

        if (gemType == 0)
        {
            t.SetPoisonStats(basePoisonDPS, poisonTime, poisonSlowMultiplier);
        }
        else if (gemType == 1)
        {
            t.SetFreezeStats(freezeSlowMultiplier, freezeTime);
        }

        //use getcomponent to get new tower’s spriterenderer
        SpriteRenderer towerRen = newTower.GetComponent<SpriteRenderer>();

        // set the spriterenderer’s sprite and color according to the tier and color of the gem
        towerRen.color = gemTypeColors[gemType];
        towerRen.sprite = gemTierSprites[gemTier];

        // calculate the gem’s stats based on its type and tier


        // initialize the gem with its stats
    }

    void Update()
    {
        if (gameState == 0 && placingGems && placedTowers.Count == 5)
        {
            // if 
            if (Input.GetMouseButtonDown(1))
            {
                //get grid position at mouse
                //loop through placedTowers and check if the grid position matches any of their grid positions
                foreach (Tower t in placedTowers)
                {

                }

                // if it matches one, call OnKeepTower(index of the tower)


            }

            // move the tower placement graphic to the mouse pos
            int[] coords = GetGridAtMousePos();
            if (coords != null)
                gemPlacementGraphic.transform.position = Grid2xToPhysicalPos(coords[0], coords[1]);
        }

        if (hoverBox.activeSelf)
        {
            //set hoverbox position to mouse position
            hoverBox.transform.position = Input.mousePosition;
        }
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
            }
            else
            {

            }
            //     if index is not keptIndex:
            //tower.NotKept()


            //         set tower’s sprite to rock sprite and tower’s spriterenderer color to white

            //     else:
            //note the kept tower’s type and tier


            //         run through the placedTowers list and see if there are any other gems of the same type and tier


            //         if so, combine it with the kept tower, raising the tier of the kept tower by one and changing its sprite to match the new tier, then recalculate its stats


            //         initialize the tower, passing in the gamemanager’s onroundstart and onroundend events
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

}
