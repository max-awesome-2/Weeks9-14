using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting.Antlr3.Runtime;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    // 0 = title screen, 1 = tutorial, 2 = placement phase, 3 = wave phase
    int gameState = 2;
    private bool placingGems = false;

    // round tracking
    int roundNumber = 1;

    // player stats
    int gold = 0;
    int round = 1;

    // grid stuff
    int[][] grid;
    int gridsize = 48;
    public GameObject tilePrefabGround, tilePrefabPath, tilePrefabFlagstone, towerPrefab;

    // ui items
    public GameObject placeGemsButton;

    // pathfinding
    Pathfinding pathfinder;
    List<int[]> waypointCoords;

    // tower placement
    List<Tower> placedTowers;

    // sfx
    AudioSource audioSource;
    public AudioClip enemyDeathClip;

    // hover info box
    public GameObject hoverBox;
    public TextMeshProUGUI hoverBoxText;

    // tower sprite stuff
    public Sprite[] gemTierSprites;
    public Color[] gemTypeColors;

    // time
    private bool paused = false;
    private float gameTimeScale = 1;

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        pathfinder = GetComponent<Pathfinding>();

        //initialize grid with map shape
        grid = new int[gridsize][];

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

        // use for loops to expand each cell into a 2x2 square, doubling grid size(towers take up a 2x2 square, but this allows them to be placed at offsets of 0.5 spaces)


        // scan through grid and add all waypoint coordinates(in order) to waypointCoords



    }

    public void onenemykilled()
    {
        //gold += (gold etc.)
        gold += 1;
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
            //use pathfinder to generate enemy path based on grid and waypointCoords

            //if roundNumber % 5 == 0, it’s a flying enemy round – communicate this via bool parameter in the path generation function call
            bool flyingRound = roundNumber % 5 == 0;

        }
    }

    private void PlaceTower()
    {
        //use getgridatmouse() to get the grid the mouse is hovering over


        //check grid to see if it is a valid location for placement
        //first, check if it’s blocked by another tower or flagstone
        //then, if it’s not, use Pathfinder to attempt to generate a path between all waypoints


        //if placement is not valid, spawn an instance of FloatingText at the mouse


        //if it’s blocked by another tower or the player tried to place on flagstone, set the floatingtext’s text to ‘cannot place here!’
		//if the placement would block the enemy’s path, change the text to ‘must leave path for enemies!’

        //if placement is valid, instantiate tower from prefab
        //add instantiated tower’s Tower script to placedTowers
        //block off the new tower’s grid positions in grid
        //get a randomly generated type and tier to assign to the new gem


        //use getcomponent to get new tower’s spriterenderer


        // set the spriterenderer’s sprite and color according to the tier and color of the gem
        // calculate the gem’s stats based on its type and tier


        // initialize the gem with its stats
    }

    void update()
    {
        if (gameState == 0 && placingGems && placedTowers.Count == 5)
        {
            // if 
            if (Input.GetMouseButtonDown(1))
            {
                //get grid position at mouse
                //loop through placedTowers and check if the grid position matches any of their grid positions


                // if it matches one, call OnKeepTower(index of the tower)



            }
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

            } else
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
        hoverBoxText.text = hoverInfo
    }

    public void OnMouseExitObjectOfInterest()
    {
        hoverBox.SetActive(false);
    }

    public void OnPauseButtonPressed()
    {
        paused = !paused;
        gameTimeScale = paused ? 0 : (fastforwarding ? 3 : 1);
    }

    public void UpgradeButtonPressed()
    {
        if (gold < upgradegold) return;
        gold -= upgradegold;
        upgradelevel++;
        for (int i = 0; i < 5; i++)
        {
            tierchances[i] = tierChanceUpgradeLevels[i][upgradeLevel];
        }
    }

}
