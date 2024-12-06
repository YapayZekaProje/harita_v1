using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UniformCostSearch : MonoBehaviour
{
    public Grid grid; // Grid scriptine referans (kare harita için)
    public Transform startPoint; // Başlangıç pozisyonu
    public Transform targetPoint; // Hedef pozisyon

    private void Start()
    {
        FindPath(); // Yolu bulmak için metodu çağır
    }

    private void FindPath()
    {
        // Başlangıç ve hedef pozisyonlarına karşılık gelen düğümleri al
        Node startNode = grid.NodeFromWorldPoint(startPoint.position);
        Node targetNode = grid.NodeFromWorldPoint(targetPoint.position);

        // Öncelikli Kuyruk (Uniform Cost Search için SortedSet kullanılıyor)
        PriorityQueue<Node> openSet = new PriorityQueue<Node>(); // Calismasi icin PriorityQueue.cs projeye eklenmesi gerekir
        HashSet<Node> closedSet = new HashSet<Node>();

        //Enqueue, Dequeue, ve Contains PriorityQueue.cs'da belirttim

        startNode.gCost = 0; // Başlangıç düğümünün maliyeti 0
        openSet.Enqueue(startNode, startNode.gCost); // Başlangıç düğümünü kuyruğa ekle 

        while (openSet.Count > 0) // Kuyruk boş değilse devam et
        {
            // En düşük maliyete sahip düğümü al
            Node currentNode = openSet.Dequeue();
            closedSet.Add(currentNode); // Değerlendirilmiş düğümleri kapalı listeye ekle

            // Hedef düğüme ulaşıldıysa, yolu çiz ve çık
            if (currentNode == targetNode)
            {
                RetracePath(startNode, targetNode);
                return;
            }

            // Komşu düğümleri kontrol et
            foreach (Node neighbor in grid.GetNeighbours(currentNode))
            {
                // Yürünemeyen ya da zaten kapalı listede olan düğümleri atla
                if (!neighbor.Walkable || closedSet.Contains(neighbor))
                {
                    continue;
                }

                // Komşuya yeni maliyeti hesapla
                int newCostToNeighbor = currentNode.gCost + GetDistance(currentNode, neighbor);

                // Daha düşük maliyet bulunursa ya da düğüm açık listede değilse
                if (newCostToNeighbor < neighbor.gCost || !openSet.Contains(neighbor))
                {
                    neighbor.gCost = newCostToNeighbor; // Yeni maliyeti ata
                    neighbor.parent = currentNode; // Geri izleme için ebeveyn düğümü güncelle

                    // Eğer düğüm açık listede değilse, kuyruğa ekle
                    if (!openSet.Contains(neighbor))
                    {
                        openSet.Enqueue(neighbor, neighbor.gCost);
                    }
                }
            }
        }
    }

    private void RetracePath(Node startNode, Node endNode)
    {
        List<Node> path = new List<Node>(); // Yolu saklamak için liste
        Node currentNode = endNode;

        // Başlangıç düğümüne ulaşana kadar geriye doğru ilerle
        while (currentNode != startNode)
        {
            path.Add(currentNode);
            currentNode = currentNode.parent;
        }

        path.Reverse(); // Listeyi ters çevirerek yolu başlangıçtan hedefe doğru sırala
        grid.path1 = path; // Yolu görselleştirme için grid'e ata
    }

    private int GetDistance(Node nodeA, Node nodeB)
    {
        // İki düğüm arasındaki Manhattan mesafesini hesapla
        int distX = Mathf.Abs(nodeA.gridX - nodeB.gridX);
        int distY = Mathf.Abs(nodeA.gridY - nodeB.gridY);

        return distX + distY; // Manhattan mesafesi (ızgara tabanlı hareket için)
    }
}
