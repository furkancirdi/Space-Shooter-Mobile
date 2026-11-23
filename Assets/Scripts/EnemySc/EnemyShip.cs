using UnityEngine;

// Asagi inerken saga sola zikzak yapan basit dusman gemisi.
public class EnemyShip : Enemy
{
    [SerializeField] private float speed = 3f;
    [SerializeField] private float zigzagAmplitude = 1.5f;
    [SerializeField] private float zigzagFrequency = 2f;

    private float startX;
    private float direction;
    private float age;

    private void Start()
    {
        speed *= GameManager.SpeedMultiplier;
        health *= Levels.EnemyHealth(Progress.Level);
        transform.rotation = Quaternion.identity;

        // Zikzak ekranin disina tasmasin diye merkez cizgisi kenarlardan uzak tutulur.
        // Gemi bu sirada ekranin ustunde oldugu icin kaydirma gorunmez.
        float margin = zigzagAmplitude + 0.5f;
        startX = Mathf.Clamp(transform.position.x, Playfield.Left + margin, Playfield.Right - margin);
        direction = Random.value < 0.5f ? -1f : 1f;

        Vector3 p = transform.position;
        p.x = startX;
        transform.position = p;
        rb.position = p;
    }

    private void FixedUpdate()
    {
        // Salinim sifirdan baslar: gemi oldugu yerden yumusakca zikzaga girer, sicramaz
        age += Time.fixedDeltaTime;
        Vector2 pos = rb.position;
        pos.y -= speed * Time.fixedDeltaTime;
        pos.x = startX + Mathf.Sin(age * zigzagFrequency) * zigzagAmplitude * direction;
        rb.MovePosition(pos);
    }

    protected override void ApplyTheme()
    {
        UseThemedSprite(GameAssets.I.themeShips);
    }

    // Olunce: buyuk ates patlamasi
    protected override void DeathEffects()
    {
        Effects.PlayAnimation(explosionFrames, transform.position, 1.6f, 20f, Color.white, Material, SortingOrder + 1);
    }

    // Gemi parcalari da zincire katilir
    protected override void SpawnShards()
    {
        EmitShards(5, 0.25f);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player")) OnPlayerContact();
    }

    private void OnBecameInvisible()
    {
        Destroy(gameObject);
    }
}
