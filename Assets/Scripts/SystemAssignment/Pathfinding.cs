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
                nodeGrid[x][y] = new Node(gameManager.Grid2xToPhysicalPos(x, y), x, y, n != -1);

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

        List<Node> nodePath = new List<Node>(), openList = new List<Node>(), closedList = new List<Node>();
        Node target, currentNode;
        //add first waypoint node location to output vector3 list


        //	// pseudocode here based on https://medium.com/@nicholas.w.swift/easy-a-star-pathfinding-7e6689c7f7b2
        //	for each waypoint node (except for the final one):
        for (int wpNodeIndex = 0; wpNodeIndex < waypointNodes.Length - 1; wpNodeIndex++)
        {
            Node startNode = waypointNodes[wpNodeIndex];
            Node nextNode = waypointNodes[wpNodeIndex + 1];

            //		clear nodepath, openlist
            nodePath.Clear();
            openList.Clear();

            //		add waypoint node to open list
            openList.Add(startNode);

            //		set target to next waypoint node
            target = nextNode;

            bool pathFound = false;

            while (!pathFound)
            {
                //	clear closed list
                closedList.Clear();

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
                        if (x < 0 || x >= gridSize2x || y < 0 || y >= gridSize2x)
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
        }


        //		construct path list by going backwards from waypoint node and recursively adding parents to the list
        //		for each node in the list EXCEPT THE FIRST, add its Vector3 position to the output Vector3 list;
        //return the output vector3 list

        return outputPath;

    }
}
