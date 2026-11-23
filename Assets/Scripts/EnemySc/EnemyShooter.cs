using UnityEngine;

// Ekranin ust kismina inip saga sola kayan ve oyuncuya nisan alarak ates eden dusman.
// Bir sure sonra asagi dalip ekrandan cikar.
public class EnemyShooter : Enemy
{
    private const float EnterSpeed = 3.5f;
    private const float LeaveSpeed = 5f;
    private const float StayTime = 11f;
    private const float BulletSpeed = 5f;

    private const float SwayAmplitude = 2f;

    private float hoverY;
    private float baseX;
    private float direction;
    private float hoverAge;
    private float fireTimer = 1f;
    private float fireInterval;
    private bool hovering;
    private bool leaving;

    private void Start()
    {
        // Salinimin merkezi kenarlardan uzak tutulur; boylece gemi kenara yapisip kalmaz
        float margin = SwayAmplitude + 0.8f;
        baseX = Mathf.Clamp(transform.position.x, Playfield.Left + margin, Playfield.Right - margin);
        direction = Random.value < 0.5f ? -1f : 1f;
        hoverY = Playfield.At(0f, Random.Range(0.64f, 0.82f)).y;
        fireInterval = 2.4f / GameManager.SpeedMultiplier;
    }

    private void FixedUpdate()
    {
        Vector2 pos = rb.position;
        float dt = Time.fixedDeltaTime;

        if (leaving)
        {
            pos.y -= LeaveSpeed * GameManager.SpeedMultiplier * dt;
        }
        else
        {
            // Hedef yukseklige yaklastikca yavaslar; birden durmaz
            float remaining = pos.y - hoverY;
            float speed = Mathf.Clamp(remaining * 2.5f, 0.4f, EnterSpeed);
            pos.y = Mathf.MoveTowards(pos.y, hoverY, speed * dt);
            if (remaining < 1.2f) hovering = true;

            if (hovering)
            {
                // Salinim sifirdan baslar ve ilk saniyede yumusakca hizlanir: gemi sicramaz
                hoverAge += dt;
                float ease = Mathf.SmoothStep(0f, 1f, hoverAge);
                float targetX = baseX + Mathf.Sin(hoverAge * 1.3f) * SwayAmplitude * direction;
                pos.x = Mathf.Lerp(pos.x, targetX, ease);
                if (hoverAge >= StayTime) leaving = true;
            }
            else
            {
                // Inerken merkez cizgisine dogru hafifce kayar
                pos.x = Mathf.MoveTowards(pos.x, baseX, 1.5f * dt);
            }
        }

        rb.MovePosition(pos);

        if (pos.y < Playfield.Bottom - 1.5f) Destroy(gameObject);
    }

    private void Update()
    {
        if (!hovering || leaving) return;

        fireTimer -= Time.deltaTime;
        if (fireTimer > 0f) return;
        fireTimer = fireInterval;

        PlayerStatus player = PlayerStatus.Instance;
        if (player == null) return;

        Vector2 dir = ((Vector2)(player.transform.position - transform.position)).normalized;
        EnemyBullet.Spawn(GameAssets.I.enemyBulletSprite, transform.position + Vector3.down * 0.4f, dir * BulletSpeed, 0.6f, 0.18f);
        Sfx.Play(GameAssets.I.enemyShoot, 0.45f);
    }

    protected override void DeathEffects()
    {
        Effects.PlayAnimation(explosionFrames, transform.position, 1.7f, 20f, new Color(1f, 0.8f, 1f), Material, SortingOrder + 1);
    }

    protected override void ApplyTheme()
    {
        UseThemedSprite(GameAssets.I.themeShooters);
    }

    protected override void SpawnShards()
    {
        EmitShards(5, 0.25f);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player")) OnPlayerContact();
    }
}
