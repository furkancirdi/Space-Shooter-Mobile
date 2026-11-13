using UnityEngine;

// Asagi inerken saga sola zikzak yapan basit dusman gemisi.
public class EnemyShip : Enemy
{
    [SerializeField] private float speed = 3f;
    [SerializeField] private float zigzagAmplitude = 1.5f;
    [SerializeField] private float zigzagFrequency = 2f;

    private float startX;
    private float phase;

    private void Start()
    {
        speed *= GameManager.SpeedMultiplier;
        startX = transform.position.x;
        phase = Random.Range(0f, Mathf.PI * 2f);
        transform.rotation = Quaternion.identity;
    }

    private void FixedUpdate()
    {
        Vector2 pos = rb.position;
        pos.y -= speed * Time.fixedDeltaTime;
        pos.x = startX + Mathf.Sin(Time.time * zigzagFrequency + phase) * zigzagAmplitude;
        rb.MovePosition(pos);
    }

    // Olunce: buyuk ates patlamasi + gemi parcalari
    public override void DeathSequence()
    {
        base.DeathSequence();

        Effects.PlayAnimation(explosionFrames, transform.position, 1.6f, 20f, Color.white, Material, SortingOrder + 1);
        Effects.SpawnDebris(sr != null ? sr.sprite : null, transform.position, 5, 0.25f, 5f, new Color(1f, 0.6f, 0.3f), Material, SortingOrder + 2);

        Destroy(gameObject);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player")) KillPlayer(other.gameObject);
    }

    private void OnBecameInvisible()
    {
        Destroy(gameObject);
    }
}
