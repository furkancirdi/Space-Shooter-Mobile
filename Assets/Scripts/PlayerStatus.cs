using UnityEngine;

// Oyuncunun cani, hasar sonrasi dokunulmazligi ve sureli gucleri (kalkan, uclu atis).
// GameManager tarafindan oyun basinda Player nesnesine eklenir.
public class PlayerStatus : MonoBehaviour
{
    public const int StartLives = 3;
    public const int MaxLives = 5;
    public const float ShieldDuration = 6f;
    public const float TripleShotDuration = 8f;
    private const float InvulnerableAfterHit = 2f;
    public const float HitRadius = 0.45f;

    public static PlayerStatus Instance { get; private set; }

    public int Lives { get; private set; } = StartLives;
    private bool safe;      // bolum bitti: artik hasar almaz
    public float ShieldTime { get; private set; }
    public float TripleShotTime { get; private set; }
    public bool TripleShot => TripleShotTime > 0f;

    private float invulnerableTime;
    private SpriteRenderer sr;
    private Color baseColor;
    private SpriteRenderer bubble;

    private void Awake()
    {
        Instance = this;
        Lives = Mathf.Min(MaxLives, StartLives + Progress.ExtraLives);
        sr = GetComponent<SpriteRenderer>();
        if (sr != null) baseColor = sr.color;
        CreateBubble();
    }

    private void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }

    private void Update()
    {
        if (TripleShotTime > 0f) TripleShotTime -= Time.deltaTime;
        if (ShieldTime > 0f) ShieldTime -= Time.deltaTime;

        if (invulnerableTime > 0f)
        {
            invulnerableTime -= Time.deltaTime;
            // Hasar sonrasi yanip sonme
            if (sr != null)
            {
                Color c = baseColor;
                c.a = invulnerableTime > 0f && Mathf.Repeat(Time.time * 10f, 1f) < 0.5f ? 0.25f : 1f;
                sr.color = c;
            }
        }

        if (bubble != null)
        {
            bool on = ShieldTime > 0f;
            // Bitmeye yakin kalkan yanip soner
            if (on && ShieldTime < 1.5f) on = Mathf.Repeat(Time.time * 8f, 1f) < 0.6f;
            bubble.enabled = on;
            bubble.transform.Rotate(0, 0, 60f * Time.deltaTime);
        }
    }

    // Dusman, dusman mermisi veya boss carptiginda cagrilir
    public void TakeHit()
    {
        if (invulnerableTime > 0f || safe) return;
        if (GameManager.Instance != null && GameManager.Instance.IsGameOver) return;

        if (ShieldTime > 0f)
        {
            Sfx.Play(GameAssets.I.shieldBlock, 0.7f);
            ScreenShake.Shake(0.07f, 0.12f);
            return;
        }

        Lives--;
        Sfx.Play(GameAssets.I.playerHit, 0.9f);
        ScreenShake.Shake(0.3f, 0.35f);
        Vibrate();

        if (Lives <= 0)
        {
            Die();
            return;
        }

        invulnerableTime = InvulnerableAfterHit;
    }

    public void ActivateShield() { ShieldTime = ShieldDuration * Progress.PowerTimeMultiplier; }
    public void ActivateTripleShot() { TripleShotTime = TripleShotDuration * Progress.PowerTimeMultiplier; }
    public void MakeSafe() { safe = true; }

    // Can doluysa false doner (cagiran bonus puan verir)
    public bool Heal()
    {
        if (Lives >= MaxLives) return false;
        Lives++;
        return true;
    }

    private void Die()
    {
        GameAssets a = GameAssets.I;
        Material mat = sr != null ? sr.sharedMaterial : a.spriteMaterial;
        Effects.PlayAnimation(a.explosionFrames, transform.position, 2.2f, 16f, Color.white, mat, 20);
        if (sr != null) Effects.SpawnDebris(sr.sprite, transform.position, 8, 0.2f, 7f, Color.white, mat, 21);
        Sfx.Play(a.explosionBig, 0.9f);
        ScreenShake.Shake(0.45f, 0.6f);

        if (GameManager.Instance != null) GameManager.Instance.PlayerDied();
        Destroy(gameObject);
    }

    private void CreateBubble()
    {
        GameAssets a = GameAssets.I;
        if (a == null || a.shieldBubbleSprite == null) return;

        GameObject go = new GameObject("ShieldBubble");
        go.transform.SetParent(transform, false);
        go.transform.localScale = Vector3.one * 1.25f;
        bubble = go.AddComponent<SpriteRenderer>();
        bubble.sprite = a.shieldBubbleSprite;
        if (sr != null)
        {
            bubble.sharedMaterial = sr.sharedMaterial;
            bubble.sortingLayerID = sr.sortingLayerID;
            bubble.sortingOrder = sr.sortingOrder + 1;
        }
        bubble.color = new Color(1f, 1f, 1f, 0.75f);
        bubble.enabled = false;
    }

    private static void Vibrate()
    {
#if (UNITY_ANDROID || UNITY_IOS) && !UNITY_EDITOR
        Handheld.Vibrate();
#endif
    }
}
