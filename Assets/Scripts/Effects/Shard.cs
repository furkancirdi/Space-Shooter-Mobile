using UnityEngine;

// Patlayan dusmandan sacilan, baska dusmanlara hasar veren parca.
// Zincirleme patlamanin temeli: her parca oldurdugu dusmanin zincir derinligini bir artirir.
public class Shard : MonoBehaviour
{
    private const float Damage = 1f;

    private Vector2 velocity;
    private float spin;
    private float lifetime;
    private float age;
    private float radius;
    private int chainDepth;
    private Enemy source;
    private SpriteRenderer sr;
    private Vector3 startScale;

    public static void Burst(Sprite sprite, Vector3 position, int count, float scale, float speed,
                             int chainDepth, Enemy source, Material material, int sortingOrder)
    {
        if (sprite == null) return;

        float offset = Random.Range(0f, 360f);
        for (int i = 0; i < count; i++)
        {
            // Parcalar esit aralikli yonlere dagilir, biraz rastgelelikle
            float angle = (offset + 360f * i / count + Random.Range(-15f, 15f)) * Mathf.Deg2Rad;
            Vector2 dir = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));

            GameObject go = new GameObject("Shard");
            go.transform.position = new Vector3(position.x, position.y, -6f);
            go.transform.rotation = Quaternion.Euler(0, 0, Random.Range(0f, 360f));
            go.transform.localScale = Vector3.one * scale * Random.Range(0.8f, 1.15f);

            SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = sprite;
            if (material != null) sr.sharedMaterial = material;
            sr.sortingOrder = sortingOrder;
            sr.color = new Color(1f, 0.85f, 0.6f);

            Shard shard = go.AddComponent<Shard>();
            shard.sr = sr;
            shard.velocity = dir * speed * Random.Range(0.85f, 1.15f);
            shard.spin = Random.Range(-540f, 540f);
            shard.lifetime = Random.Range(0.5f, 0.65f);
            shard.radius = Mathf.Max(0.12f, sprite.bounds.extents.x * go.transform.localScale.x);
            shard.chainDepth = chainDepth;
            shard.source = source;
            shard.startScale = go.transform.localScale;
        }
    }

    private void Update()
    {
        age += Time.deltaTime;
        float t = age / lifetime;
        if (t >= 1f)
        {
            Destroy(gameObject);
            return;
        }

        velocity *= 1f - 1.2f * Time.deltaTime;
        transform.position += (Vector3)(velocity * Time.deltaTime);
        transform.Rotate(0, 0, spin * Time.deltaTime);

        // Son ucte birinde kuculerek solar
        if (t > 0.66f)
        {
            float f = 1f - (t - 0.66f) / 0.34f;
            transform.localScale = startScale * (0.5f + 0.5f * f);
            Color c = sr.color;
            c.a = f;
            sr.color = c;
        }

        Vector2 pos = transform.position;
        for (int i = 0; i < Enemy.Active.Count; i++)
        {
            Enemy e = Enemy.Active[i];
            if (e == source || e.IsDead) continue;

            float reach = e.HitRadius + radius;
            if (((Vector2)e.transform.position - pos).sqrMagnitude > reach * reach) continue;

            e.TakeDamage(Damage, transform.position, chainDepth);
            Destroy(gameObject);
            return;
        }
    }
}
