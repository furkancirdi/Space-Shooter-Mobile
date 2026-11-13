using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem.EnhancedTouch;

public class MeteorSpawner : MonoBehaviour
{

    [SerializeField] private GameObject [] meteor;
    [SerializeField] private float spawnTime;
    [SerializeField] private bool randomRotationAndSize = true;
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

        // Zorluk arttikca spawn araligi kisalir
        if (timer > spawnTime / GameManager.Difficulty)
        {
            i = Random.Range(0, meteor.Length);
            Vector3 spawnPos = new Vector3(Random.Range(maxLeft, maxRight), yPos, -5);

            if (randomRotationAndSize)
            {
                GameObject obj = Instantiate(meteor[i], spawnPos, Quaternion.Euler(0,0,Random.Range(0,360)));
                float size = Random.Range(0.5f, 1.5f);
                obj.transform.localScale = new Vector3(size, size, 1);
            }
            else
            {
                Instantiate(meteor[i], spawnPos, meteor[i].transform.rotation);
            }
            timer = 0;
        }

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
