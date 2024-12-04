using UnityEngine;

public class RaycastDistance : MonoBehaviour
{
    public float maxDistance;
    public Transform rayOrigin;
    RaycastHit hit;
    Pathfinding pathfinding;
    Player player;

    private void Start()
    {
        pathfinding = FindObjectOfType<Pathfinding>();
        player = FindAnyObjectByType<Player>();
    }

    private void Update()
    {
        StartRaycast();
    }

    private void StartRaycast()
    {
        Ray ray = new Ray(rayOrigin.position, rayOrigin.forward); // Ray'i başlat
        RaycastHit[] hits = Physics.RaycastAll(ray, maxDistance, ~0, QueryTriggerInteraction.Collide);

        bool shouldSlowDown = false;

        foreach (RaycastHit hit in hits)
        {
            float distance = hit.distance;
            GameObject hitObject = hit.collider.gameObject;

            // Crosswalk kontrolü
            CrosswalkController crosswalk = hitObject.GetComponent<CrosswalkController>();
            if (crosswalk != null)
            {
                if (crosswalk.PedestrianCount > 0 && distance < 8.25f) // Eğer crosswalk'ta yaya varsa
                {
                    Debug.Log("Yaya var, yavaşlıyorum");
                    shouldSlowDown = true;
                    break;
                }

                else
                {
                    Debug.Log("Crosswalk boş");
                }
            }

            // Diğer kontroller (örneğin ışık sistemi)
            LightSystemSC lightSystem = hitObject.GetComponent<LightSystemSC>();
            if (lightSystem != null && lightSystem.red)
            {
                if (distance < 8.25f)
                {
                    Debug.Log("Kırmızı ışık, yavaşlıyorum");
                    shouldSlowDown = true;
                    break;
                }
            }
        }

        // Hız kontrolü
        if (shouldSlowDown)
        {
            player.isSlowingDown = true;
            player.isAccelerating = false;
        }
        else
        {
            player.isSlowingDown = false;
            player.isAccelerating = true;
        }

        Debug.DrawRay(rayOrigin.position, rayOrigin.forward * maxDistance, Color.green);
    }
}
