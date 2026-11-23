using UnityEngine;

public class PlayerShooting : MonoBehaviour
{

    [SerializeField] private GameObject laserBullet;
    [SerializeField] private Transform basicShootPoint;

    [SerializeField] private float shootingInterval;
    private float intervalReset;


    void Start()
    {
        // Marketten alinan atis hizi yukseltmesi araligi kisaltir
        intervalReset = shootingInterval / Progress.FireRateMultiplier;
    }


    void Update()
    {
        shootingInterval -= Time.deltaTime;

        if (shootingInterval <= 0)
        { 
            Shoot();
            shootingInterval = intervalReset;
        }   
    
    }


    private void Shoot()
    {
        Instantiate(laserBullet, basicShootPoint.position, Quaternion.identity);

        // Uclu atis gucu aktifken iki yana da acili mermi
        if (PlayerStatus.Instance != null && PlayerStatus.Instance.TripleShot)
        {
            Instantiate(laserBullet, basicShootPoint.position, Quaternion.Euler(0, 0, 14f));
            Instantiate(laserBullet, basicShootPoint.position, Quaternion.Euler(0, 0, -14f));
        }

        Sfx.Play(GameAssets.I.laser, 0.22f, Random.Range(0.95f, 1.05f));
    }


}
