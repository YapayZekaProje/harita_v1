using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;
using TMPro;
using Unity.VisualScripting;
using System.Collections;



public class Player : MonoBehaviour
{
    public TextMeshProUGUI speedText;

    public float currentSpeed;
    private float moveSpeed = 5;
    public float maxSpeed;
    public float deceleration = 0.3f; // Yava�lama h�z�
    public float acceleration = 0.5f;  //hizlanma hizi 
    public bool isSlowingDown = false;
    public bool isAccelerating = true;
    public bool isAstar;

    private Vector3 targetPosition; // Oyuncunun gitmesi gereken hedef pozisyon
    private int currentNodeIndex = 0; // �u anda �zerinde bulunulan yol d���m�n�n indeksi
    private List<Node> path; // Oyuncunun takip edece�i yol

    public float baseTurnSpeed = 50f; // Temel d�n�� h�z� (derece/saniye)
    private float turnSpeed; // D�n�� h�z�n� sabit
    private Quaternion targetRotation; // Hedef rotay� saklayan de�i�ken
    private bool isTurning = false; // D�n�� yap�l�p yap�lmad����kontrol�

    private AudioSource gazAudioSource;

    private void Start()
    {
        gazAudioSource = GetComponent<AudioSource>();
        SpeedTextUpdate();  // ba�lang��ta h�z metnini g�ncelle
    }

    // Yolun bitip bitmedi�ini kontrol eden metot
    public bool IsPathFinished()
    {
        return path == null || currentNodeIndex >= path.Count;
    }

    // Oyuncunun takip edece�i yolu ayarlayan metot
    public void SetPath(List<Node> newPath)
    {
        path = newPath; // Yeni yolu belirle
        currentNodeIndex = 0; // �lk d���mden ba�la
        if (path.Count > 0)
        {
            // D�finir la position cible en fixant Y � 1
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

        LookToTarget(hedefNoktasi); // Hedefe do�ru yava��a d�necek
    }

    // Oyuncuyu hedefe do�ru hareket ettiren metot
    public void MoveToTarget(Vector3 target)
    {
        LookToTarget(target); // Hedefe d�n
        transform.position = Vector3.MoveTowards(transform.position, target, currentSpeed * Time.deltaTime); // Hedefe do�ru hareket et
    }

    public void LookToTarget(Vector3 target)
    {
        Vector3 direction = target - transform.position;

        // E�er hedefe �ok yak�n de�ilse
        if (direction.magnitude > 0.1f)
        {
            direction.y = 0;  // y ekseninde d�nmeyi engelliyoruz

            targetRotation = Quaternion.LookRotation(direction);

            // Mevcut rotasyonu al ve yaln�zca Y eksenini ayarla
            Vector3 currentRotation = transform.rotation.eulerAngles;
            targetRotation = Quaternion.Euler(currentRotation.x, targetRotation.eulerAngles.y, currentRotation.z);

            // Arabay� sadece Y ekseninde hedefe bakacak �ekilde yumu�ak�a d�nd�r
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 2.0f);

            // D�n�� h�z�n� sabit tutal�m, d�n�� yaparken h�zla orant�l� yapal�m
            turnSpeed = Mathf.Lerp(baseTurnSpeed, baseTurnSpeed * 1.5f, currentSpeed / maxSpeed);  // H�zla orant�l� d�n��

            // E�er d�n�� hala yap�l�yorsa, rotay� yava��a hedefe do�ru d�nd�r
            if (!isTurning)
            {
                StartCoroutine(SmoothTurn()); // Yumu�ak d�n�� ba�lat
            }
        }
    }

    private IEnumerator SmoothTurn()
    {
        isTurning = true;

        // D�n�� ba�lad���nda yava��a d�n
        float timeElapsed = 30f;
        float turnDuration = 3f; // D�n�� s�resi (saniye)

        // Hedef rotaya kademeli ge�i� sa�lamak i�in
        Quaternion initialRotation = transform.rotation;

        while (timeElapsed < turnDuration)
        {
            timeElapsed += Time.deltaTime;

            // Yava��a hedef rotaya d�n�yoruz
            transform.rotation = Quaternion.Slerp(initialRotation, targetRotation, timeElapsed / turnDuration);
            yield return null;
        }

        // D�n�� tamamland�ktan sonra
        isTurning = false;
    }

    private void GameKontrol()
    {
        Vector3 newPosition = transform.position;

        // Yukar� ok tu�u z ekseninde art�rma yapar
        if (Input.GetKey(KeyCode.UpArrow))
        {
            newPosition.z += moveSpeed * Time.deltaTime;
        }

        // A�a�� ok tu�u z ekseninde azaltma yapar
        if (Input.GetKey(KeyCode.DownArrow))
        {
            newPosition.z -= moveSpeed * Time.deltaTime;
        }

        // Sa� ok tu�u x ekseninde art�rma yapar
        if (Input.GetKey(KeyCode.RightArrow))
        {
            newPosition.x += moveSpeed * Time.deltaTime;
        }

        // Sol ok tu�u x ekseninde azaltma yapar
        if (Input.GetKey(KeyCode.LeftArrow))
        {
            newPosition.x -= moveSpeed * Time.deltaTime;
        }

        // Yeni pozisyonu g�ncelle
        transform.position = newPosition;

        if (Input.GetKeyDown(KeyCode.KeypadPlus))
            ++Time.timeScale;
        if (Input.GetKeyDown(KeyCode.KeypadMinus))
            --Time.timeScale;



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
        if (isAstar)
        {


            // Oyuncunun takip edebilece�i bir yol varsa hedef pozisyona do�ru hareket et
            if (path != null && path.Count > 0)
            {
                // H�zlanma veya yava�lama durumuna g�re h�z g�ncellemesi
                if (isAccelerating)
                {
                    currentSpeed = Mathf.Min(maxSpeed, currentSpeed + acceleration * Time.deltaTime); // Maksimum h�z� a�ma
                }
                else if (isSlowingDown)
                {
                    currentSpeed = Mathf.Max(0, currentSpeed - deceleration * Time.deltaTime); // H�z� s�f�r�n alt�na d���rme
                }

                MoveToTarget(targetPosition); // Hedefe do�ru hareket et

                // Hedef d���me ula��l�p ula��lmad���n� kontrol et
                if (Vector3.Distance(transform.position, targetPosition) < 0.1f) // E�ik de�eri (mesafe kontrol�)
                {
                    currentNodeIndex++; // Sonraki d���me ge�
                    if (currentNodeIndex < path.Count)
                    {
                        // D�finir la position cible en fixant Y � 1
                        targetPosition = new Vector3(path[currentNodeIndex].WorldPosition.x, 1, path[currentNodeIndex].WorldPosition.z);

                    }
                    else
                    {
                        Debug.Log("Yolun sonuna ula��ld�!"); // Yolun bitti�ini bildir
                        path = null; // Yol bilgisini s�f�rla
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