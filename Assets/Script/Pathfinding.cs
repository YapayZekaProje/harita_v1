using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using System.Text;
using UnityEngine;
using System.Threading.Tasks;
using Newtonsoft.Json;

public class Pathfinding : MonoBehaviour
{
    Grid grid;
    public Transform seeker, target;
    Player player;
    public bool driveable = true;
    Vector3 baslangicKonumu; // ÖNCEKİNİN ÜZERİNE EKLENDİ
    float kusUcumuMesafe;
    private bool requestSent = false; // KELVİNDEN ALINDI

    private void Awake()
    {
        grid = GetComponent<Grid>();
        player = FindObjectOfType<Player>();  // Player'ı bul

        baslangicKonumu = player.transform.position; // ÖNCEKİNİN ÜZERİNE EKLENDİ


        // KELVİNDEN ALINDI
        Debug.Log($"Astar is : {player.isAstar}");
        grid.Start(); // Grid'in oluşturulduğundan emin olun
        PrintGridInfoToFile(); // Grid detaylarını bir dosyaya yazdırın

    }
    void GoToTarget()
    {
        // ÖNCEKİNİN ÜZERİNE EKLENDİ
        kusUcumuMesafe = Vector3.Distance(player.transform.position, target.position);
        if (kusUcumuMesafe<=4f)
        {
            Debug.LogWarning("tp attim");
            player.transform.position = baslangicKonumu;

        }
        
        if (grid.path1 != null && grid.path1.Count > 0 && driveable)
        {

            Vector3 hedefNokta = grid.path1[0].WorldPosition;  // İlk path noktası 
            player.LookToTarget(hedefNokta);

            //     Debug.Log(Vector3.Distance(player.transform.position, target.position));  // hedefle kus ucumu mesafe olcer 

            player.GidilcekYer(hedefNokta);  // Hedef noktayı Player'a gönder

        }
    }



    private void Update()
    {
        // KELVİNDEN ALINDI
        if (player.isAstar)
        {
            Debug.Log("Update metodu çağrıldı.");
            RequestPathFromServer(seeker.position, target.position);
        }
        else
        {
            FindPath(seeker.position, target.position);
            GoToTarget();
        }
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

            if (currentNode == targetNode)
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

                // Kavşak kontrolü
                if (!currentNode.kavsak && !neighbour.kavsak)
                {
                    // Yön kontrolü
                    if (currentNode.gridY < neighbour.gridY && !currentNode.right) // Yukarı hareket (right == true)
                    {
                        continue;
                    }
                    if (currentNode.gridX < neighbour.gridX && !currentNode.right) // Sağa hareket (right == true)
                    {
                        continue;
                    }
                    if (currentNode.gridY > neighbour.gridY && !currentNode.left) // Aşağı hareket (left == true)
                    {
                        continue;
                    }
                    if (currentNode.gridX > neighbour.gridX && !currentNode.left) // Sola hareket (left == true)
                    {
                        continue;
                    }
                }

                // Right'tan direkt Left'e veya Left'ten direkt Right'a geçişi engelle
                if (currentNode.right && neighbour.left && !neighbour.kavsak)
                {
                    continue;
                }
                if (currentNode.left && neighbour.right && !neighbour.kavsak)
                {
                    continue;
                }

                // A* METODU MU ????

                // Hareket maliyetini hesapla ve komşuyu ekle
                //int newMovementCostToNeighbour = currentNode.gCost + GetDistance(currentNode, neighbour);
                //if (newMovementCostToNeighbour < neighbour.gCost || !openSet.Contains(neighbour))
                //{
                //    neighbour.gCost = newMovementCostToNeighbour;
                //    neighbour.hCost = GetDistance(neighbour, targetNode);
                //    neighbour.parent = currentNode;

                //    if (!openSet.Contains(neighbour))
                //        openSet.Add(neighbour);
                //}
            }
        }

        // Yol bulunamadıysa hata mesajı
        Debug.LogWarning("Path not found!");
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

    // Grid bilgisini bir dosyaya yazdıran metod
    void PrintGridInfoToFile()
    {
        if (grid.grid == null)
        {
            Debug.LogError("Grid henüz başlatılmadı.");
            return;
        }

        string filePath = @"C:\Users\ylmzh\OneDrive\Desktop\ornek_proje-main\ornek_proje-main\Assets\Script\GridInfo.json";
        List<Dictionary<string, object>> gridInfoList = new List<Dictionary<string, object>>();

        float nodeRadius = grid.NodeRadius; // Node yarıçapını alın

        // Grid üzerindeki her bir düğümü dolaşarak bilgilerini kaydedin
        for (int x = 0; x < grid.grid.GetLength(0); x++)
        {
            for (int y = 0; y < grid.grid.GetLength(1); y++)
            {
                Node node = grid.grid[x, y];
                var nodeInfo = new Dictionary<string, object>
                {
                    { "gridX", node.gridX },
                    { "gridY", node.gridY },
                    { "worldX", node.WorldPosition.x },
                    { "worldZ", node.WorldPosition.z },
                    { "walkable", node.Walkable }
                };
                gridInfoList.Add(nodeInfo);
            }
        }

        // JSON yapısını oluşturun ve dosyaya yazdırın
        var finalJsonStructure = new
        {
            nodeRadius = nodeRadius,
            gridSizeX = grid.grid.GetLength(0),
            gridSizeY = grid.grid.GetLength(1),
            nodes = gridInfoList
        };

        string json = JsonConvert.SerializeObject(finalJsonStructure, Formatting.Indented);
        File.WriteAllText(filePath, json);
        Debug.Log($"Grid bilgisi şu dosyaya kaydedildi: {filePath}");
    }

    // Server'dan yol isteği gönderen metod
    void RequestPathFromServer(Vector3 startPoz, Vector3 targetPoz)
    {
        if (requestSent)
        {
            Debug.Log("İstek zaten gönderildi, yanıt bekleniyor...");
            return;
        }

        string serverIP = "127.0.0.1"; // Sunucunun IP adresi
        int port = 8089; // Python sunucusuyla eşleşen port numarası

        try
        {
            using (TcpClient client = new TcpClient(serverIP, port))
            using (NetworkStream stream = client.GetStream())
            {
                // Başlangıç ve hedef pozisyonlarını gönderin
                string request = $"{startPoz.x},{startPoz.y},{startPoz.z};{targetPoz.x},{targetPoz.y},{targetPoz.z}";
                Debug.Log($"Sunucuya istek gönderiliyor: {request}");
                byte[] data = Encoding.UTF8.GetBytes(request);
                stream.Write(data, 0, data.Length);

                // Yanıt alın
                byte[] buffer = new byte[1024];
                int bytesRead = stream.Read(buffer, 0, buffer.Length);
                string response = Encoding.UTF8.GetString(buffer, 0, bytesRead);

                Debug.Log($"Sunucudan gelen yanıt: {response}");

                ParsePathResponse(response);
            }
        }
        catch (SocketException ex)
        {
            Debug.LogError($"SocketException: {ex.Message}");
        }

        requestSent = true; // Yeni istek gönderilmesini engelle
    }

    // Sunucudan gelen yolu çözümleyen metod
    void ParsePathResponse(string response)
    {
        string[] pathNodes = response.Split(';');
        List<Node> path = new List<Node>();

        foreach (string nodeData in pathNodes)
        {
            if (string.IsNullOrEmpty(nodeData)) continue;

            string[] nodeCoordinates = nodeData.Split(',');
            if (nodeCoordinates.Length == 3)
            {
                // Grid koordinatlarını kullanarak düğümü bulun
                int gridX = int.Parse(nodeCoordinates[0]);
                int gridY = int.Parse(nodeCoordinates[1]);

                Node node = grid.grid[gridX, gridY]; // Grid üzerinden düğümü alın
                if (node != null)
                {
                    path.Add(node);
                }
            }
        }

        // Oyuncuya yolu ayarla
        player.SetPath(path);

    }

}
