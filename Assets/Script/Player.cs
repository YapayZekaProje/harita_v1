using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;
using TMPro;
using Unity.VisualScripting;



public class Player : MonoBehaviour
{
    public TextMeshProUGUI speedText;

    public float currentSpeed;
    private float moveSpeed = 5;
    public float maxSpeed;
    public float deceleration = 0.2f; // Yavaşlama hızı
    public float acceleration = 0.5f;  //hizlanma hizi 
    public bool isSlowingDown = false;
    public bool isAccelerating = true;
    public bool isAstar;

    private Vector3 targetPosition; // Oyuncunun gitmesi gereken hedef pozisyon
    private int currentNodeIndex = 0; // Şu anda üzerinde bulunulan yol düğümünün indeksi
    private List<Node> path; // Oyuncunun takip edeceği yol

    private AudioSource gazAudioSource;

    private void Start()
    {
        gazAudioSource = GetComponent<AudioSource>();

    }

    // Yolun bitip bitmediğini kontrol eden metot
    public bool IsPathFinished()
    {
        return path == null || currentNodeIndex >= path.Count;
    }

    // Oyuncunun takip edeceği yolu ayarlayan metot
    public void SetPath(List<Node> newPath)
    {
        path = newPath; // Yeni yolu belirle
        currentNodeIndex = 0; // İlk düğümden başla
        if (path.Count > 0)
        {
            // Définir la position cible en fixant Y à 1
            targetPosition = new Vector3(path[currentNodeIndex].WorldPosition.x, 1, path[currentNodeIndex].WorldPosition.z);
        }
    }

    public void GidilcekYer(Vector3 hedefNoktasi)
    {
        ++hedefNoktasi.y;
        if (isAccelerating)
        {
            isSlowingDown = false;

            currentSpeed = Mathf.Min(maxSpeed, currentSpeed + acceleration * Time.deltaTime);
        }

        else if (isSlowingDown)
        {

            currentSpeed = Mathf.Max(0, currentSpeed - deceleration * Time.deltaTime);
        }

        transform.position = Vector3.MoveTowards(transform.position, hedefNoktasi, currentSpeed * Time.deltaTime);

    }

    // Oyuncuyu hedefe doğru hareket ettiren metot
    public void MoveToTarget(Vector3 target)
    {
        LookToTarget(target); // Hedefe dön
        transform.position = Vector3.MoveTowards(transform.position, target, currentSpeed * Time.deltaTime); // Hedefe doğru hareket et
    }

    public void LookToTarget(Vector3 target)
    {
        Vector3 direction = target - transform.position;

        // Eğer hedefe çok yakın değilse
        if (direction.magnitude > 0.1f)
        {
            direction.y = 0;  // y ekseninde dönmeyi engelliyoruz

            Quaternion targetRotation = Quaternion.LookRotation(direction);

            // Mevcut rotasyonu al ve yalnızca Y eksenini ayarla
            Vector3 currentRotation = transform.rotation.eulerAngles;
            targetRotation = Quaternion.Euler(currentRotation.x, targetRotation.eulerAngles.y, currentRotation.z);

            // Arabayı sadece Y ekseninde hedefe bakacak şekilde yumuşakça döndür
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 2.0f);
        }
    }

    private void GameKontrol()
    {
        Vector3 newPosition = transform.position;

        // Yukarı ok tuşu z ekseninde artırma yapar
        if (Input.GetKey(KeyCode.UpArrow))
        {
            newPosition.z += moveSpeed * Time.deltaTime;
        }

        // Aşağı ok tuşu z ekseninde azaltma yapar
        if (Input.GetKey(KeyCode.DownArrow))
        {
            newPosition.z -= moveSpeed * Time.deltaTime;
        }

        // Sağ ok tuşu x ekseninde artırma yapar
        if (Input.GetKey(KeyCode.RightArrow))
        {
            newPosition.x += moveSpeed * Time.deltaTime;
        }

        // Sol ok tuşu x ekseninde azaltma yapar
        if (Input.GetKey(KeyCode.LeftArrow))
        {
            newPosition.x -= moveSpeed * Time.deltaTime;
        }

        // Yeni pozisyonu güncelle
        transform.position = newPosition;

        if (Input.GetKeyDown(KeyCode.KeypadPlus))
            ++Time.timeScale;
        if (Input.GetKeyDown(KeyCode.KeypadMinus))
            --Time.timeScale    ;
 


        SpeedTextUpdate();

        //if (maxSpeed == 0)
        //{
        //    isSlowingDown = false;
        //}
        if (maxSpeed == currentSpeed)
        {
            isAccelerating = false;
        }
        if (maxSpeed != currentSpeed)
        {

            if (isSlowingDown)
            {
            }
            else
            {
                isAccelerating = true;
            }
        }

        if (currentSpeed == maxSpeed)
        {
            gazAudioSource.pitch = Mathf.Lerp(gazAudioSource.pitch, Random.Range(1.5f, 1.7f), currentSpeed / maxSpeed);
        }
        else if (currentSpeed != 0)
        {
            gazAudioSource.pitch = Mathf.Lerp(0.42f, 1.7f, currentSpeed / maxSpeed);

            if (!gazAudioSource.isPlaying)
            {
                gazAudioSource.Play();
            }
        }

        else
        {
            gazAudioSource.pitch = Mathf.Lerp(0.42f, 1.7f, currentSpeed / maxSpeed);
            gazAudioSource.volume = 0.2f;

        }

    


}

    private void Update()
    {
        if(isAstar)
        {
           

            // Oyuncunun takip edebileceği bir yol varsa hedef pozisyona doğru hareket et
            if (path != null && path.Count > 0)
            {
                // Hızlanma veya yavaşlama durumuna göre hız güncellemesi
                if (isAccelerating)
                {
                    currentSpeed = Mathf.Min(maxSpeed, currentSpeed + acceleration * Time.deltaTime); // Maksimum hızı aşma
                }
                else if (isSlowingDown)
                {
                    currentSpeed = Mathf.Max(0, currentSpeed - deceleration * Time.deltaTime); // Hızı sıfırın altına düşürme
                }

                MoveToTarget(targetPosition); // Hedefe doğru hareket et

                // Hedef düğüme ulaşılıp ulaşılmadığını kontrol et
                if (Vector3.Distance(transform.position, targetPosition) < 0.1f) // Eşik değeri (mesafe kontrolü)
                {
                    currentNodeIndex++; // Sonraki düğüme geç
                    if (currentNodeIndex < path.Count)
                    {
                        // Définir la position cible en fixant Y à 1
                        targetPosition = new Vector3(path[currentNodeIndex].WorldPosition.x, 1, path[currentNodeIndex].WorldPosition.z);

                    }
                    else
                    {
                        Debug.Log("Yolun sonuna ulaşıldı!"); // Yolun bittiğini bildir
                        path = null; // Yol bilgisini sıfırla
                    }
                }
            }
        }
        GameKontrol();
    }

    private void SpeedTextUpdate()
    {

        speedText.text = (currentSpeed * 10).ToString("F1") + " Km/H";

    }
}
