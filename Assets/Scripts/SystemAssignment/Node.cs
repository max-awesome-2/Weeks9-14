using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Node
{
    // class representing an A* pathfinding node
    public bool isPassable = false;
    public float f, g, h;
    private Node parent;

    public Vector3 pos;
    public int gridX, gridY;

    public Node(Vector3 worldPos, int x, int y, bool passable)
    {
        isPassable = passable;
        pos = worldPos;

        gridX = x;
        gridY = y;
    }

    public void SetParent(Node n)
    {
        parent = n;
    }

    public void UpdateValues(Node targetNode)
    {
        // when we call UpdateValues on the start node, it doesn't have a parent, so just calculate its h and f
        if (parent != null) g = parent.g + Vector3.Distance(pos, parent.pos);

        h = Vector3.Distance(pos, targetNode.pos);

        f = g + h;
    }

    public void Reset()
    {
        parent = null;
        f = 0;
        g = 0;
        h = 0;
    }



}
