using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    // Sahnedeki tum canli dusmanlar (parcalar ve efektler fizik yerine bu listeyi tarar)
    public static readonly List<Enemy> Active = new List<Enemy>();

    [SerializeField] protected float health;
    [SerializeField] protected Rigidbody2D rb;
    [SerializeField] protected int scoreValue = 10;
    [Tooltip("Olunce ozel guc dusurme ihtimali (0-1)")]
    [SerializeField] protected float dropChance = 0.02f;
    [Tooltip("Olunce coin dusurme ihtimali (0-1)")]
    [SerializeField] protected float coinChance = 1f;
    [Tooltip("Dustugunde kac coin duser")]
    [SerializeField] protected int coinCount = 1;

    [Header("Effects")]
    [SerializeField] protected Sprite[] explosionFrames;
    [SerializeField] protected Color hitFlashColor = new Color(1f, 0.45f, 0.45f);

    protected bool isDead;
    protected SpriteRenderer sr;
    protected int killChain;          // olduren vurusun zincir derinligi (0 = lazer)
    private Color baseColor = Color.white;
    private Coroutine flashRoutine;
    private CircleCollider2D circle;

    public bool IsDead => isDead;

    // Parcalarin carpisma kontrolu icin dunya birimi cinsinden yaricap
    public float HitRadius => circle != null ? circle.radius * transform.lossyScale.x : 0.5f;

    protected virtual void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
        circle = GetComponent<CircleCollider2D>();
        if (rb == null) rb = GetComponent<Rigidbody2D>();
        ApplyTheme();
        if (sr != null) baseColor = sr.color;
        if ((explosionFrames == null || explosionFrames.Length == 0) && GameAssets.I != null)
            explosionFrames = GameAssets.I.explosionFrames;
    }

    // Bulunulan dunyaya gore gorunum. Alt siniflar sprite'i da degistirebilir.
    protected virtual void ApplyTheme()
    {
        if (sr != null) sr.color *= WorldTheme.MeteorTint;
    }

    // 2. dunyadan itibaren gemiler o dunyanin renginde cizilmis sprite'i kullanir
    protected void UseThemedSprite(Sprite[] themed)
    {
        Sprite sprite = WorldTheme.Pick(themed);
        if (sr != null && sprite != null) sr.sprite = sprite;
    }

    protected virtual void OnEnable() { Active.Add(this); }
    protected virtual void OnDisable() { Active.Remove(this); }

    // Kodla olusturulan dusmanlar icin (prefab'i olmayanlar)
    public void Configure(float hp, int score, float drop, int coins)
    {
        coinCount = coins;
        health = hp;
        scoreValue = score;
        dropChance = drop;
    }


    public void TakeDamage(float dmg)
    {
        TakeDamage(dmg, transform.position, 0);
    }

    public void TakeDamage(float dmg, Vector3 hitPoint, int chainDepth = 0)
    {
        if (isDead) return;

        health -= dmg;

        if (health <= 0)
        {
            isDead = true;
            killChain = chainDepth;
            DeathSequence();
        }
        else
        {
            Sfx.Play(GameAssets.I.hit, 0.35f, Random.Range(0.9f, 1.1f));
            HurtSequence(hitPoint);
        }

    }


    // Varsayilan: renk flasi + vurulan noktada kucuk kivilcim
    public virtual void HurtSequence(Vector3 hitPoint)
    {
        Flash();
        Effects.PlayAnimation(explosionFrames, hitPoint, 0.35f, 30f, Color.white, Material, SortingOrder + 1);
    }

    // Oyuncu tarafindan olduruldu: puan, guc dusurme, zincir parcalari, efekt
    public virtual void DeathSequence()
    {
        if (GameManager.Instance != null)
            GameManager.Instance.RegisterKill(scoreValue, killChain, transform.position);

        PowerUp.TryDrop(transform.position, dropChance);
        DropCoins();
        SpawnShards();
        DeathEffects();
        Destroy(gameObject);
    }

    // Zincirle oldurulen dusman, zincirin derinligi kadar fazladan coin dusurur (en cok 3)
    protected virtual void DropCoins()
    {
        int count = Random.value < coinChance ? coinCount : 0;
        count += Mathf.Min(3, killChain);
        if (count > 0) Coin.Spawn(transform.position, count, Levels.CoinValue(Progress.Level));
    }

    // Alt siniflar kendi patlama efektini ve parcalarini tanimlar
    protected virtual void DeathEffects() { }
    protected virtual void SpawnShards() { }

    // Oyuncuya carpinca true donen dusmanlar yok olmaz (boss)
    protected virtual bool SurvivesContact => false;

    protected void EmitShards(int count, float scale, float speed = 7f)
    {
        if (sr == null) return;
        Shard.Burst(sr.sprite, transform.position, count, scale, speed, killChain + 1, this, Material, SortingOrder + 2);
    }

    protected void OnPlayerContact()
    {
        if (isDead || PlayerStatus.Instance == null) return;

        PlayerStatus.Instance.TakeHit();

        if (!SurvivesContact)
        {
            isDead = true;
            DeathEffects();
            Destroy(gameObject);
        }
    }

    protected Material Material => sr != null ? sr.sharedMaterial : null;
    protected int SortingOrder => sr != null ? sr.sortingOrder : 5;

    protected void Flash()
    {
        if (sr == null) return;
        if (flashRoutine != null) StopCoroutine(flashRoutine);
        flashRoutine = StartCoroutine(FlashRoutine());
    }

    private IEnumerator FlashRoutine()
    {
        sr.color = hitFlashColor;
        yield return new WaitForSeconds(0.08f);
        sr.color = baseColor;
        flashRoutine = null;
    }

}
