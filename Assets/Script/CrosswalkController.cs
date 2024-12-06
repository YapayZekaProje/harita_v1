using System.Collections.Generic;
using UnityEngine;

public class CrosswalkController : MonoBehaviour
{
    public GameObject vehicle;             // Aracı tanımlayın
    public float detectionRadius = 10f;    // Aracın algılanacağı mesafe
    public List<PedestrianController> pedestrians; // Hareket ettirilecek yayalar
    private bool isVehicleClose = false;   // Araç yaya geçidine yakın mı?

    public int pedestrianCount = 0;       // Crosswalk'taki yaya sayısı
    public int PedestrianCount // Getter ile dışarıdan okunabilir
    {
        get { return pedestrianCount; }
    }

    void Update()
    {
        DetectVehicleDistance();
    }

    void DetectVehicleDistance()
    {
        if (vehicle == null) return;

        float distanceToVehicle = Vector3.Distance(transform.position, vehicle.transform.position);

        if (distanceToVehicle <= detectionRadius && !isVehicleClose)
        {
            isVehicleClose = true;
            TriggerPedestrianMovement(true); // Yayaları harekete geçir
        }
        else if (distanceToVehicle > detectionRadius && isVehicleClose)
        {
            isVehicleClose = false;
            TriggerPedestrianMovement(false); // Yayaları durdur
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer == 10) // Yayaların layer'ı 10
        {
            pedestrianCount++;
            gameObject.layer = 7; // Crosswalk'u aktif yap
            Debug.Log($"Yaya crosswalk'a girdi. Mevcut yaya sayısı: {pedestrianCount}");
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.layer == 10)
        {
            pedestrianCount--;
            if (pedestrianCount <= 0)
            {
                pedestrianCount = 0;
                gameObject.layer = 0; // Crosswalk'u boş yap
                Debug.Log("Crosswalk boşaldı");
            }
        }
    }

    void TriggerPedestrianMovement(bool shouldMove)
    {
        foreach (var pedestrian in pedestrians)
        {
            if (pedestrian != null)
            {
                pedestrian.SetMovement(shouldMove); // Yaya hareketini başlat/durdur
            }
        }
    }
}
