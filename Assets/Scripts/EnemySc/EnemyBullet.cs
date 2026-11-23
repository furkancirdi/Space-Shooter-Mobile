using UnityEngine;

// Dusman ve boss mermisi. Oyuncuya carpisma mesafe kontroluyle yapilir.
public class EnemyBullet : MonoBehaviour
{
    private Vector2 velocity;
    private float radius;
    private float spin;

    public static void Spawn(Sprite sprite, Vector3 position, Vector2 velocity, float scale, float radius, float spin = 0f)
    {
        if (sprite == null) return;

        GameObject go = new GameObject("EnemyBullet");
        go.transform.position = new Vector3(position.x, position.y, -5.5f);
        go.transform.localScale = Vector3.one * scale;

        SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = sprite;
        if (GameAssets.I.spriteMaterial != null) sr.sharedMaterial = GameAssets.I.spriteMaterial;
        sr.sortingOrder = 7;

        EnemyBullet bullet = go.AddComponent<EnemyBullet>();
        bullet.velocity = velocity;
        bullet.radius = radius;
        bullet.spin = spin;
    }

    private void Update()
    {
        transform.position += (Vector3)(velocity * Time.deltaTime);
        if (spin != 0f) transform.Rotate(0, 0, spin * Time.deltaTime);

        if (Playfield.IsOutside(transform.position, 1f))
        {
            Destroy(gameObject);
            return;
        }

        PlayerStatus player = PlayerStatus.Instance;
        if (player == null) return;

        float reach = radius + PlayerStatus.HitRadius;
        Vector2 delta = player.transform.position - transform.position;
        if (delta.sqrMagnitude > reach * reach) return;

        player.TakeHit();
        Destroy(gameObject);
    }
}
