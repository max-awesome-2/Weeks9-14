using System.Collections;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

public class Pathfinding : MonoBehaviour
{
    // reference to gamemanager - set in GameManager
    public GameManager gameManager;

    // list of waypoint coordinates
    public List<int[]> waypoints;

    // returns a series of Vector3 grid points if a path exists
    // otherwise, returns an empty list
    public List<Vector3> GetPath(int[][] grid, bool isFlyingRound)
    {
        // A* PATHFINDING
        // declare output path
        List<Vector3> outputPath = new List<Vector3>();

        Node[] waypointNodes = new Node[waypoints.Count];


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
	
        List<Node> nodePath, openList, closedList = new List<Node>();
        Node target, currentNode;
        //add first waypoint node location to output vector3 list


        //	// pseudocode here based on https://medium.com/@nicholas.w.swift/easy-a-star-pathfinding-7e6689c7f7b2
        //	for each waypoint node (except for the final one):
        //		clear nodepath, openlist
        //		add waypoint node to open list
        //		set target to next waypoint node
        //		while (path not found):
        //			clear closed list
        //			if open list is empty, no path exists – raise flag and break
        //			set currentNode to lowest F cost node in open list
        //			add it to the closed list
        //			if it is the target node, path complete - break
        //			for all 8 adjacent / diagonal nodes (if not off the grid):
        //				if it is not walkable or if it’s in the closed list, ignore it
        //				else:
        //					if it isn’t in the open list, add it to the open list, making currentNode its parent – set its f, g and h values
        //					else : 
        //						check to see if the g cost of this node is lower when going from currentNode – if so, reset the node’s parent to currentNode, and recalculate its f and g scores
        //		if path does not exist flag is true, return empty vector3 list
			
        //		construct path list by going backwards from waypoint node and recursively adding parents to the list
        //		for each node in the list EXCEPT THE FIRST, add its Vector3 position to the output Vector3 list;
        //return the output vector3 list

			return outputPath;
		
    }
}
