using System.Collections;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using static UnityEditor.PlayerSettings;

public class Pathfinding : MonoBehaviour
{
    // reference to gamemanager - set in GameManager
    public GameManager gameManager;

    // list of waypoint coordinates
    public List<int[]> waypoints;

    // cache this to save a few calculations
    private int gridSize2x;

    // returns a series of Vector3 grid points if a path exists
    // otherwise, returns null
    public List<Vector3> GetPath(int[][] grid, bool isFlyingRound)
    {
        // A* PATHFINDING
        // declare output path
        List<Vector3> outputPath = new List<Vector3>();

        Node[] waypointNodes = new Node[waypoints.Count];

        // cache stuff
        gridSize2x = gameManager.gridSize * 2;


        // convert grid into a 2D array of pathfinding nodes
        Node[][] nodeGrid = new Node[grid.Length][];
        for (int x = 0; x < grid.Length; x++)
        {
            nodeGrid[x] = new Node[grid[x].Length];
            for (int y = 0; y < grid[x].Length; y++)
            {
                // in the grid 
                int n = grid[x][y];
                nodeGrid[x][y] = new Node(gameManager.Grid2xToPhysicalPos(y, x), x, y, n != -1);

                if (n > 2)
                {
                    // get the node representing this waypoint and insert it into the waypointnodes array
                    // these will be used as destination nodes for A*
                    for (int i = 0; i < waypoints.Count; i++)
                    {
                        int[] wCoords = waypoints[i];
                        if (x == wCoords[0] && y == wCoords[1])
                        {
                            waypointNodes[i] = nodeGrid[x][y];
                        }
                    }
                }
            }
        }

        List<Node> openList = new List<Node>(), closedList = new List<Node>();
        Node target = null, currentNode = null;

        //add first waypoint node location to output vector3 list
        outputPath.Add(waypointNodes[0].pos);


        //	// pseudocode here based on https://medium.com/@nicholas.w.swift/easy-a-star-pathfinding-7e6689c7f7b2
        //	for each waypoint node (except for the final one):
        for (int wpNodeIndex = 0; wpNodeIndex < waypointNodes.Length - 1; wpNodeIndex++)
        {
            Node startNode = waypointNodes[wpNodeIndex];
            Node nextNode = waypointNodes[wpNodeIndex + 1];

            // reset all nodes
            for (int x = 0; x < gridSize2x; x++)
            {
                for (int y = 0; y < gridSize2x; y++)
                {
                    nodeGrid[x][y].Reset();
                }
            }

            //		clear closedlist, openlist
            closedList.Clear();
            openList.Clear();

            //		add waypoint node to open list
            openList.Add(startNode);

            //		set target to next waypoint node
            target = nextNode;

            bool pathFound = false;

            while (!pathFound)
            {

                //	if open list is empty, no path exists – raise flag and break
                if (openList.Count == 0)
                {
                    break;
                }

                //	set currentNode to lowest F cost node in open list
                float minFCost = -Mathf.Infinity;
                Node minFCostNode = openList[0];
                foreach (Node n in openList)
                {
                    if (n.f < minFCost)
                    {
                        minFCost = n.f;
                        minFCostNode = n;
                    }
                }

                currentNode = minFCostNode;

                //	add it to the closed list
                closedList.Add(currentNode);

                // (and i guess remove it from the open list?)
                openList.Remove(currentNode);

                //	if it is the target node, path complete - break
                if (currentNode.gridX == target.gridX && currentNode.gridY == target.gridY)
                {
                    pathFound = true;
                    break;
                }

                //	for all 8 adjacent / diagonal nodes (if not off the grid):
                for (int x = currentNode.gridX - 1; x <= currentNode.gridX + 1; x++)
                {
                    for (int y = currentNode.gridY - 1; y <= currentNode.gridY + 1; y++)
                    {
                        if (x < 0 || x >= gridSize2x || y < 0 || y >= gridSize2x || (x == currentNode.gridX && y == currentNode.gridY))
                        {
                            continue;
                        }

                        Node nXY = nodeGrid[x][y];

                        //	if it is not walkable or if it’s in the closed list, ignore it
                        if (closedList.Contains(nXY) || (!nXY.isPassable && !isFlyingRound)) continue;
                        else
                        {
                            if (!openList.Contains(nXY))
                            {
                                // if it isn’t in the open list, add it to the open list, making currentNode its parent – set its f, g and h values
                                openList.Add(nXY);
                                nXY.SetParent(currentNode);
                                nXY.UpdateValues(target);
                            }
                            else
                            {
                                //	check to see if the g cost of this node is lower when going from currentNode – if so, reset the node’s parent to currentNode, and recalculate its f and g scores
                                if (nXY.g > currentNode.g + Vector3.Distance(currentNode.pos, nXY.pos))
                                {
                                    nXY.SetParent(currentNode);
                                    nXY.UpdateValues(target);
                                }
                            }
                        }

                    }
                }

            }

            //		if path does not exist flag is true, return null
            if (!pathFound) return null;

            // at this point, currentNode EQUALS the target node - we want to add all positions leading up to the target node, including the target's pos, but NOT including the very first node's pos
            List<Vector3> posList = new List<Vector3>(); // list that values will be inserted backwards into

            while (true)
            {
                if (currentNode.parent == null)
                {
                    break;
                } else
                {

                    posList.Insert(0, currentNode.pos);

                    currentNode = currentNode.parent;
                }

            }

            // add the subpath's positions to the full output path
            foreach (Vector3 v in posList)
            {
                outputPath.Add(v);
            }

        }





        //return the output vector3 list
        return outputPath;

    }
}
