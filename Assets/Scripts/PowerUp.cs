using UnityEngine;

// Dusmanlardan nadiren dusen, oyuncu ustunden gecince toplanan ozel guc.
public class PowerUp : MonoBehaviour
{
    public enum Kind { TripleShot, Shield, Heal }

    private const float FallSpeed = 2.2f;
    private const float PickupRadius = 0.9f;
    private const float BaseScale = 0.75f;

    private const float DropCooldown = 14f;
    private static float lastDropTime = -DropCooldown;

    private Kind kind;
    private float age;

    public static void TryDrop(Vector3 position, float chance)
    {
        if (Random.value > chance) return;
        if (GameManager.Instance == null || GameManager.Instance.IsGameOver) return;

        // Gucler nadir kalsin: iki dusme arasinda en az DropCooldown saniye
        float now = GameManager.Instance.Elapsed;
        if (now < lastDropTime) lastDropTime = -DropCooldown;   // yeni oyun basladi
        if (now - lastDropTime < DropCooldown) return;
        lastDropTime = now;

        Spawn(position, RandomKind());
    }

    public static void Spawn(Vector3 position, Kind kind)
    {
        GameAssets a = GameAssets.I;
        Sprite sprite = kind == Kind.TripleShot ? a.tripleShotSprite
                      : kind == Kind.Shield ? a.shieldPickupSprite
                      : a.healSprite;
        if (sprite == null) return;

        GameObject go = new GameObject("PowerUp_" + kind);
        go.transform.position = new Vector3(position.x, position.y, -6f);
        go.transform.localScale = Vector3.one * BaseScale;

        SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = sprite;
        if (a.spriteMaterial != null) sr.sharedMaterial = a.spriteMaterial;
        sr.sortingOrder = 8;

        go.AddComponent<PowerUp>().kind = kind;
    }

    private static Kind RandomKind()
    {
        float r = Random.value;
        if (r < 0.4f) return Kind.TripleShot;
        if (r < 0.75f) return Kind.Shield;
        return Kind.Heal;
    }

    private void Update()
    {
        age += Time.deltaTime;
        transform.position += Vector3.down * FallSpeed * Time.deltaTime;
        transform.localScale = Vector3.one * BaseScale * (1f + 0.08f * Mathf.Sin(age * 6f));

        if (transform.position.y < Playfield.Bottom - 1f)
        {
            Destroy(gameObject);
            return;
        }

        PlayerStatus player = PlayerStatus.Instance;
        if (player == null) return;

        Vector2 delta = player.transform.position - transform.position;
        if (delta.sqrMagnitude > PickupRadius * PickupRadius) return;

        Collect(player);
        Destroy(gameObject);
    }

    private void Collect(PlayerStatus player)
    {
        string label;
        switch (kind)
        {
            case Kind.TripleShot:
                player.ActivateTripleShot();
                label = "TRIPLE SHOT";
                break;
            case Kind.Shield:
                player.ActivateShield();
                label = "SHIELD";
                break;
            default:
                if (player.Heal()) label = "+1 LIFE";
                else
                {
                    // Can doluysa bonus puan
                    GameManager.Instance.AddScore(100);
                    label = "+100";
                }
                break;
        }

        Sfx.Play(GameAssets.I.powerup, 0.8f);
        if (GameManager.Instance != null && GameManager.Instance.Hud != null)
            GameManager.Instance.Hud.Popup(label, transform.position, new Color(0.5f, 1f, 0.6f), 40);
    }
}
