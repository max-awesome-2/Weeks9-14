using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Node
{
    // class representing an A* pathfinding node
    public bool isPassable = false;
    public float f, g, h;
    public Node parent;

    public Vector2 pos;

    public Node(int x, int y, bool passable)
    {
        isPassable = passable;
        pos = new Vector2(x, y);
    }

    public void UpdateValues(Node targetNode)
    {
        g = parent.g + Vector3.Distance(pos, parent.pos);

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
