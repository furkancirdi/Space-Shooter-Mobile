using UnityEngine;

public class Meteor : Enemy
{

    [SerializeField] private float minSpeed;
    [SerializeField] private float maxSpeed;
    private float speed;
    [SerializeField] private float rotateSpeed;

    // Kaya tozu rengi: patlama efektini gri/kahverengi tonlar
    private static readonly Color DustTint = new Color(0.85f, 0.75f, 0.65f);

    void Start()
    {
        speed = Random.Range(minSpeed, maxSpeed) * GameManager.SpeedMultiplier;
        rb.linearVelocity = Vector2.down * speed;

        // Boyut dayanikliligi ve puani belirler: kucuk 1 can, orta 2, buyuk 3
        health = Mathf.Max(1, Mathf.RoundToInt(health * Size * Levels.EnemyHealth(Progress.Level)));
        coinChance = 0.2f;
        scoreValue = Mathf.Max(5, Mathf.RoundToInt(scoreValue * Size / 5f) * 5);
    }

    void Update()
    {
        transform.Rotate(0, 0, rotateSpeed * Time.deltaTime);
    }


    // Vurulunca: flas + kopan kucuk kaya parcalari
    public override void HurtSequence(Vector3 hitPoint)
    {
        Flash();
        Effects.SpawnDebris(sr != null ? sr.sprite : null, hitPoint, 3, 0.12f * Size, 4f, Color.white, Material, SortingOrder + 1);
    }

    // Olunce: toz bulutu
    protected override void DeathEffects()
    {
        Effects.PlayAnimation(explosionFrames, transform.position, 0.9f * Size, 22f, DustTint, Material, SortingOrder + 1);
    }

    // Zincirleme patlama: buyuk meteor daha cok ve daha buyuk parca sacar
    protected override void SpawnShards()
    {
        EmitShards(3 + Mathf.RoundToInt(Size * 2f), 0.28f * Size);
    }

    private float Size => transform.localScale.x;


    private void OnTriggerEnter2D(Collider2D otherColl)
    {

        if (otherColl.CompareTag("Player"))
        {
            OnPlayerContact();
        }


    }


    private void OnBecameInvisible()
    {
        Destroy(gameObject);
    }


}
