using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Grid : MonoBehaviour
{
    public Transform player;
    public LayerMask UnwalkableMask;
    public Vector2 GridWroldSize;            //harita boyut
    public float NodeRadius;                 //grid yaricap
    public Node[,] grid;

    float NodeDinameter;                    //grid cap
    int GridSizeX, GridSizeY;                //grid boyut

    public List<Node> path1 = new List<Node>();


    public void Start()
    {
        NodeDinameter = NodeRadius * 2;
        GridSizeX = Mathf.RoundToInt(GridWroldSize.x / NodeDinameter);
        GridSizeY = Mathf.RoundToInt(GridWroldSize.y / NodeDinameter);
        CreateGrid();
    }

    private void Update()
    {
        CreateGrid();
    }

    public Node NodeFromWorldPoint(Vector3 worldPosition)
    {
        float percentX = (worldPosition.x + GridWroldSize.x / 2) / GridWroldSize.x;
        float percentY = (worldPosition.z + GridWroldSize.y / 2) / GridWroldSize.y;
        percentX = Mathf.Clamp01(percentX);
        percentY = Mathf.Clamp01(percentY);

        int x = Mathf.RoundToInt((GridSizeX - 1) * percentX);
        int y = Mathf.RoundToInt((GridSizeY - 1) * percentY);

        return grid[x, y];
    }



    private void CreateGrid()
    {
        grid = new Node[GridSizeX, GridSizeY];
        Vector3 worldbottomleft = transform.position - Vector3.right * GridWroldSize.x / 2 - Vector3.forward * GridWroldSize.y / 2;   //haritanin en sol alt kosesine git 

        for (int i = 0; i < GridSizeX; i++)
        {
            for (int j = 0; j < GridSizeY; j++)
            {
                Vector3 worlPoint = worldbottomleft + Vector3.right * (NodeRadius + i * NodeDinameter) + Vector3.forward * (NodeRadius + j * NodeDinameter);     //saga dogru gitme islemi 1-3-5-7 seklinde
                bool walkable = !(Physics.CheckSphere(worlPoint, NodeRadius, UnwalkableMask));  //yurunur alani ayarliyor

                grid[i, j] = new Node(walkable, worlPoint, i, j);
            }
        }
    }



    public List<Node> GetNeighbours(Node node)
    {
        List<Node> neighbours = new List<Node>();

        for (int x = -1; x <= 1; x++)
        {
            for (int y = -1; y <= 1; y++)
            {
                if (x == 0 && y == 0)
                    continue;
                if (x == -1 && y == -1)
                    continue;
                if (x == 1 && y == 1)
                    continue;
                if (x == 1 && y == -1)
                    continue;
                if (x == -1 && y == 1)
                    continue;
                int checkX = node.gridX + x;
                int checkY = node.gridY + y;

                if (checkX >= 0 && checkX < GridSizeX && checkY >= 0 && checkY < GridSizeY)     //mepin icinde olup olmadigini kontrol ediyor
                {
                    neighbours.Add(grid[checkX, checkY]);
                }
            }
        }

        return neighbours;
    }


    //void OnDrawGizmos()
    //{
    //    Gizmos.DrawWireCube(transform.position, new Vector3(GridWroldSize.x, 1, GridWroldSize.y));

    //    if (grid != null)
    //    {
    //        Node PlayerNode = NodeFromWorldPoint(player.position);

    //        foreach (Node node in grid)
    //        {
    //            Gizmos.color = node.Walkable ? Color.green : Color.red;



    //            if (PlayerNode == node)
    //            {
    //                Gizmos.color = Color.cyan;
    //            }

    //            Gizmos.DrawCube(node.WorldPosition, Vector3.one * (NodeDinameter - 0.1f));
    //        }
    //    }
    //}

    void OnDrawGizmos()
    {
         DrawPath();
       DrawOnlyPath();
    }

    void DrawPath()
    {
        if (grid != null)
        {
            Node PlayerNode = NodeFromWorldPoint(player.position);

            foreach (Node node in grid)
            {
                Gizmos.color = node.Walkable ? Color.green : Color.red;

                if (PlayerNode == node)
                {
                    Gizmos.color = Color.cyan;
                }

                Gizmos.DrawCube(node.WorldPosition, Vector3.one * (NodeDinameter - 0.1f));
            }
        }
        // Eğer path1 boş veya null değilse
        if (path1 != null && path1.Count > 0)
        {
            // path1'deki her düğüm için siyah küpler çiz
            foreach (Node node in path1)
            {
                Gizmos.color = Color.black;
                Gizmos.DrawCube(node.WorldPosition, Vector3.one * (NodeDinameter - 0.1f));
            }
        }
    }

    void DrawOnlyPath()
    {
        if (path1 != null && path1.Count > 0)
        {
            // path1'deki her düğüm için siyah küpler çiz
            foreach (Node node in path1)
            {
                Gizmos.color = Color.black;
                Gizmos.DrawCube(node.WorldPosition, Vector3.one * (NodeDinameter - 0.1f));
            }
        }
    }
}