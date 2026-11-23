using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem.EnhancedTouch;

public class MeteorSpawner : MonoBehaviour
{

    [SerializeField] private GameObject [] meteor;
    [SerializeField] private float spawnTime;
    [SerializeField] private bool randomRotationAndSize = true;
    [Tooltip("Tek meteor yerine kume gelme ihtimali (0-1)")]
    [SerializeField] private float clusterChance = 0.3f;
    private bool ready;
    private float timer = 0f;
    private int i;

    private Camera mainCam;
    private float maxLeft;
    private float maxRight;
    private float yPos;

    void Start()
    {
        mainCam = Camera.main;
        StartCoroutine(SetBoundaries());
    }

    void Update()
    {
        if (!ready) return;

        timer += Time.deltaTime;

        // Zorluk arttikca spawn araligi kisalir; boss varken seyreklesir
        if (timer > spawnTime * WaveDirector.SpawnIntervalFactor / GameManager.Difficulty)
        {
            timer = 0;

            i = Random.Range(0, meteor.Length);
            if (!WaveDirector.CanSpawn(meteor[i].GetComponent<Enemy>())) return;

            Vector3 spawnPos = new Vector3(Random.Range(maxLeft, maxRight), yPos, -5);

            if (randomRotationAndSize)
            {
                SpawnMeteor(meteor[i], spawnPos);

                // Ara sira yan yana bir meteor kumesi gelir: zincirleme patlama firsati
                if (Random.value < clusterChance)
                {
                    int extra = Random.Range(2, 4);
                    for (int n = 0; n < extra; n++)
                    {
                        Vector3 offset = new Vector3(Random.Range(-1.4f, 1.4f), Random.Range(0.6f, 2.6f), 0);
                        Vector3 pos = spawnPos + offset;
                        pos.x = Mathf.Clamp(pos.x, maxLeft, maxRight);
                        SpawnMeteor(meteor[Random.Range(0, meteor.Length)], pos);
                    }
                }
            }
            else
            {
                Instantiate(meteor[i], spawnPos, meteor[i].transform.rotation);
            }
        }

    }

    private void SpawnMeteor(GameObject prefab, Vector3 position)
    {
        GameObject obj = Instantiate(prefab, position, Quaternion.Euler(0,0,Random.Range(0,360)));
        float size = Random.Range(0.5f, 1.5f);
        obj.transform.localScale = new Vector3(size, size, 1);
    }

    private IEnumerator SetBoundaries()
    {
        yield return new WaitForSeconds(0.4f);

        maxLeft = mainCam.ViewportToWorldPoint(new Vector2(0.15f, 0)).x;
        maxRight = mainCam.ViewportToWorldPoint(new Vector2(0.85f, 0)).x;

        yPos = mainCam.ViewportToWorldPoint(new Vector2(0, 1.1f)).y;
        ready = true;
    }

}
