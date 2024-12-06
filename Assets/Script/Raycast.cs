using UnityEngine;

public class RaycastDistance : MonoBehaviour
{
    public float maxDistance;
    public Transform rayOrigin;
    RaycastHit hit;
    Pathfinding pathfinding;
    Player player;

    float nullTimer = 0f; // null olan script için bir zamanlayıcı
    float nullThreshold = 8f; // "null" süresi  saniye olarak ayarlanir

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
        Debug.DrawRay(rayOrigin.position, rayOrigin.forward * maxDistance, Color.green);

        RaycastHit[] hits = Physics.RaycastAll(ray, maxDistance, ~0, QueryTriggerInteraction.Collide);

        bool shouldSlowDown = false;
        
        if (hits.Length == 0)
        {

            nullTimer += Time.deltaTime; // null olduğunda zamanlayıcıyı artır

            if (nullTimer >= nullThreshold)
            {

                Debug.LogWarning("yaya yok (null bekleme süresi doldu)");
                player.isAccelerating = true; // hızlan
                nullTimer = 0f; // zamanlayıcıyı sıfırla

            }

        }

        foreach (RaycastHit hit in hits)
        {
            float distance = hit.distance;
            int hitLayer = hit.collider.gameObject.layer;
            GameObject hitObject = hit.collider.gameObject;
            LightSystemSC lamba = hitObject.GetComponent<LightSystemSC>();

            CrosswalkController script = hitObject.GetComponent<CrosswalkController>();

            if (lamba != null)
            {
                if (lamba.red == false)
                {
                    Debug.Log(" yesil");

                }
                else
                {
                    Debug.Log("kirmizi");
                    if (distance < 10f)
                    {
                        player.isAccelerating = false;
                        player.isSlowingDown = true;

                    }
                    break;
                }
            }

            if (script != null)
            {
                if (script.pedestrianCount == 0)
                {
                    Debug.Log("yaya yok ");
                }
                else
                {
                    if (distance < 10f)
                    {
                        Debug.Log("yaya var");
                        player.isAccelerating = false;
                        player.isSlowingDown = true;
                        break;
                    }
                }
            }

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
