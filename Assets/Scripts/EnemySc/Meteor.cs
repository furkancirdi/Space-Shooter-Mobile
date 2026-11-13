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

    // Olunce: toz bulutu + etrafa sacilan kaya parcalari
    public override void DeathSequence()
    {
        base.DeathSequence();

        Effects.PlayAnimation(explosionFrames, transform.position, 0.9f * Size, 22f, DustTint, Material, SortingOrder + 1);
        Effects.SpawnDebris(sr != null ? sr.sprite : null, transform.position, 7, 0.22f * Size, 6f, Color.white, Material, SortingOrder + 2);

        Destroy(gameObject);
    }

    private float Size => transform.localScale.x;


    private void OnTriggerEnter2D(Collider2D otherColl)
    {

        if (otherColl.CompareTag("Player"))
        {
            KillPlayer(otherColl.gameObject);
        }


    }


    private void OnBecameInvisible()
    {
        Destroy(gameObject);
    }


}
