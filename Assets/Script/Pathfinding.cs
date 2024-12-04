using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pathfinding : MonoBehaviour
{
    Grid grid;
    public Transform seeker, target;
    Player player;
    public bool driveable = true;

    private void Awake()
    {
        grid = GetComponent<Grid>();
        player = FindObjectOfType<Player>();  // Player'ı bul

    }
    void GoToTarget()
    {
        if (grid.path1 != null && grid.path1.Count > 0 && driveable)
        {

            Vector3 hedefNokta = grid.path1[0].WorldPosition;  // İlk path noktası 
            player.LookToTarget(hedefNokta);
            player.GidilcekYer(hedefNokta);  // Hedef noktayı Player'a gönder
        }
    }



    private void Update()
    {
        FindPath(seeker.position, target.position);
        GoToTarget();
    }

    void FindPath(Vector3 startPoz, Vector3 targetPoz)
    {
        Node startNode = grid.NodeFromWorldPoint(startPoz);
        Node targetNode = grid.NodeFromWorldPoint(targetPoz);

        List<Node> openSet = new List<Node>();
        List<Node> closedSet = new List<Node>();

        openSet.Add(startNode);
        while (openSet.Count > 0)
        {
            Node currentNode = openSet[0];
            for (int i = 1; i < openSet.Count; i++)
            {
                if (currentNode.fCost > openSet[i].fCost || currentNode.fCost == openSet[i].fCost)
                {
                    currentNode = openSet[i];
                }
            }

            openSet.Remove(currentNode);
            closedSet.Add(currentNode);
            if (targetNode == currentNode)
            {
                RetracePath(startNode, targetNode);
                return;
            }
            foreach (Node neighbour in grid.GetNeighbours(currentNode))
            {
                if (!neighbour.Walkable || closedSet.Contains(neighbour))
                {
                    continue;
                }
                int newMovementCostToNeighbour = currentNode.gCost + GetDistance(currentNode, neighbour);
                if (newMovementCostToNeighbour < neighbour.gCost || !openSet.Contains(neighbour))
                {
                    neighbour.gCost = newMovementCostToNeighbour;
                    neighbour.hCost = GetDistance(neighbour, targetNode);
                    neighbour.parent = currentNode;
                    if (!openSet.Contains(neighbour))
                        openSet.Add(neighbour);


                }

            }

        }

    }



    void RetracePath(Node startNode, Node endNode)
    {
        List<Node> path = new List<Node>();
        Node currentNode = endNode;

        while (currentNode != startNode)
        {
            path.Add(currentNode);
            currentNode = currentNode.parent;
        }
        path.Reverse(); 
        grid.path1 = path;

    }


    int GetDistance(Node nodeA, Node nodeB)
    {
        int dstx = Mathf.Abs(nodeA.gridX - nodeB.gridX);
        int dsty = Mathf.Abs(nodeA.gridY - nodeB.gridY);

        if (dstx > dsty)
            return 14 * dsty + 10 * dstx;
        return 14 * dstx + 10 * (dsty - dstx);
    }


}
