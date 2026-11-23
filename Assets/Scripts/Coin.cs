using UnityEngine;

// Dusmanlardan dusen coin. Once etrafa sacilir, sonra yavasca duser;
// oyuncu yaklasinca ona dogru cekilir. Toplaninca kalici bakiyeye eklenir.
public class Coin : MonoBehaviour
{
    private const float FallSpeed = 1.8f;
    private const float BaseMagnetRadius = 1.7f;
    private const float PickupRadius = 0.55f;
    private const float PullSpeed = 13f;
    private const float Scale = 0.55f;

    // Boss olunce sahnedeki butun coinler oyuncuya ucar
    public static bool AttractAll;

    private Vector2 velocity;
    private float age;
    private int value;
    private bool pulled;

    public static void Spawn(Vector3 position, int count, int value)
    {
        GameAssets a = GameAssets.I;
        if (a == null || a.coinSprite == null) return;

        for (int i = 0; i < count; i++)
        {
            GameObject go = new GameObject("Coin");
            go.transform.position = new Vector3(position.x, position.y, -6f);
            go.transform.localScale = Vector3.one * Scale;

            SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = a.coinSprite;
            if (a.spriteMaterial != null) sr.sharedMaterial = a.spriteMaterial;
            sr.sortingOrder = 8;

            Coin coin = go.AddComponent<Coin>();
            coin.value = value;
            coin.velocity = Random.insideUnitCircle * (count > 1 ? 4.5f : 1.5f);
        }
    }

    private void Update()
    {
        float dt = Time.deltaTime;
        age += dt;

        PlayerStatus player = PlayerStatus.Instance;
        Vector3 pos = transform.position;

        if (player != null && age > 0.25f)
        {
            Vector2 delta = player.transform.position - pos;
            float radius = BaseMagnetRadius * Progress.MagnetMultiplier;
            if (AttractAll || delta.sqrMagnitude < radius * radius) pulled = true;

            if (pulled)
            {
                if (delta.sqrMagnitude < PickupRadius * PickupRadius)
                {
                    Collect();
                    return;
                }
                pos += (Vector3)(delta.normalized * Mathf.Min(delta.magnitude, PullSpeed * dt));
                transform.position = pos;
                return;
            }
        }

        // Sacilma hizi soner, yerine sabit dusus gelir
        velocity = Vector2.Lerp(velocity, Vector2.down * FallSpeed, 4f * dt);
        pos += (Vector3)(velocity * dt);
        pos.x = Mathf.Clamp(pos.x, Playfield.Left + 0.3f, Playfield.Right - 0.3f);
        transform.position = pos;

        // Hafif "donme" hissi
        Vector3 s = Vector3.one * Scale;
        s.x *= 0.55f + 0.45f * Mathf.Abs(Mathf.Cos(age * 5f));
        transform.localScale = s;

        if (pos.y < Playfield.Bottom - 1f) Destroy(gameObject);
    }

    private void Collect()
    {
        Progress.AddCoins(value);
        if (GameManager.Instance != null) GameManager.Instance.CoinCollected(value);
        Sfx.Play(GameAssets.I.coin, 0.4f, Random.Range(0.95f, 1.15f));
        Destroy(gameObject);
    }
}
