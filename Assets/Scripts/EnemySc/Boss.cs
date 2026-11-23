using UnityEngine;

// Her asamanin sonunda gelen boss. Ustte saga sola kayar; yelpaze ve nisanli atis yapar.
// Cani azaldikca daha sik ates eder. Lazer kadar zincir parcalari da ona hasar verir.
public class Boss : Enemy
{
    private const float EnterSpeed = 2.5f;

    private float maxHealth;
    private float hoverY;
    private float age;
    private bool arrived;
    private float attackTimer = 1.5f;
    private int attackIndex;
    private int burstLeft;
    private float burstTimer;

    public float HealthFraction => Mathf.Clamp01(health / maxHealth);

    private void Start()
    {
        maxHealth = health;
        hoverY = Playfield.At(0f, 0.76f).y;
    }

    private void FixedUpdate()
    {
        Vector2 pos = rb.position;
        float dt = Time.fixedDeltaTime;

        if (!arrived)
        {
            pos.y -= EnterSpeed * dt;
            if (pos.y <= hoverY) arrived = true;
        }
        else
        {
            age += dt;
            float range = (Playfield.Right - Playfield.Left) * 0.5f - HitRadius - 0.5f;
            pos.x = Mathf.Sin(age * 0.7f) * Mathf.Max(0.2f, range);
            pos.y = hoverY + Mathf.Sin(age * 1.1f) * 0.35f;
        }

        rb.MovePosition(pos);
    }

    private void Update()
    {
        if (!arrived || PlayerStatus.Instance == null) return;

        // Nisanli seri atis devam ediyorsa
        if (burstLeft > 0)
        {
            burstTimer -= Time.deltaTime;
            if (burstTimer <= 0f)
            {
                FireAimed();
                burstLeft--;
                burstTimer = 0.18f;
            }
        }

        attackTimer -= Time.deltaTime;
        if (attackTimer > 0f) return;

        // Can azaldikca saldiri arasi 2.4 saniyeden 1.3 saniyeye iner
        attackTimer = Mathf.Lerp(1.3f, 2.4f, HealthFraction) / (Mathf.Sqrt(GameManager.SpeedMultiplier) * Levels.BossPace(Progress.Level));

        if (attackIndex % 2 == 0) FireFan();
        else { burstLeft = Levels.BossBurst(Progress.Level); burstTimer = 0f; }
        attackIndex++;
    }

    // Ilk bolumlerde mermiler de yavastir (yuzde 80'den baslar)
    private static float BulletPace => Mathf.Lerp(0.5f, 1f, Levels.BossPace(Progress.Level));

    private void FireFan()
    {
        // Mermi sayisi bolume gore artar; aralarindan gecilebilsin diye yelpaze de genisler
        int count = Levels.BossFanBullets(Progress.Level) + (HealthFraction < 0.5f ? 2 : 0);
        float spread = count > 7 ? 96f : 80f;
        for (int i = 0; i < count; i++)
        {
            float angle = (-90f - spread * 0.5f + spread * i / (count - 1)) * Mathf.Deg2Rad;
            Vector2 dir = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
            EnemyBullet.Spawn(GameAssets.I.bossBulletSprite, transform.position + Vector3.down * 1.2f, dir * 4.2f * BulletPace, 0.4f, 0.22f, 180f);
        }
        Sfx.Play(GameAssets.I.enemyShoot, 0.6f, 0.7f);
    }

    private void FireAimed()
    {
        Vector3 from = transform.position + Vector3.down * 1.2f;
        Vector2 dir = ((Vector2)(PlayerStatus.Instance.transform.position - from)).normalized;
        EnemyBullet.Spawn(GameAssets.I.enemyBulletSprite, from, dir * 5.5f * BulletPace, 0.7f, 0.2f);
        Sfx.Play(GameAssets.I.enemyShoot, 0.5f, 1.2f);
    }

    protected override bool SurvivesContact => true;

    protected override void ApplyTheme()
    {
        UseThemedSprite(GameAssets.I.themeBosses);
    }

    public override void HurtSequence(Vector3 hitPoint)
    {
        base.HurtSequence(hitPoint);
        if (GameManager.Instance != null && GameManager.Instance.Hud != null)
            GameManager.Instance.Hud.SetBossHealth(HealthFraction);
    }

    public override void DeathSequence()
    {
        // Boss olunce bolum biter; coinleri etrafa sacilir
        if (WaveDirector.Instance != null) WaveDirector.Instance.OnBossDefeated(transform.position);
        base.DeathSequence();
    }

    protected override void DeathEffects()
    {
        Effects.PlayAnimation(explosionFrames, transform.position, 3.6f, 14f, Color.white, Material, SortingOrder + 3);
        for (int i = 0; i < 5; i++)
        {
            Vector3 offset = Random.insideUnitCircle * 1.6f;
            Effects.PlayAnimation(explosionFrames, transform.position + offset, Random.Range(1.2f, 2f), Random.Range(12f, 20f), Color.white, Material, SortingOrder + 2);
        }
        Effects.SpawnDebris(sr != null ? sr.sprite : null, transform.position, 10, 0.18f, 8f, Color.white, Material, SortingOrder + 2);
        Sfx.Play(GameAssets.I.explosionBig, 1f);
        ScreenShake.Shake(0.5f, 0.8f);
    }

    protected override void SpawnShards()
    {
        EmitShards(14, 0.14f, 9f);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player")) OnPlayerContact();
    }
}
