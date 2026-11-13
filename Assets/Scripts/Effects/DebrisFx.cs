using UnityEngine;

public class DebrisFx : MonoBehaviour
{
    private Vector2 velocity;
    private float spin;
    private float lifetime;
    private float age;
    private SpriteRenderer sr;
    private Color startColor;
    private Vector3 startScale;

    public void Init(Vector2 startVelocity, float angularSpeed, float life)
    {
        velocity = startVelocity;
        spin = angularSpeed;
        lifetime = life;
        sr = GetComponent<SpriteRenderer>();
        startColor = sr.color;
        startScale = transform.localScale;
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

        velocity *= 1f - 3f * Time.deltaTime;
        transform.position += (Vector3)(velocity * Time.deltaTime);
        transform.Rotate(0, 0, spin * Time.deltaTime);
        transform.localScale = startScale * (1f - t * 0.5f);

        Color c = startColor;
        c.a = startColor.a * (1f - t);
        sr.color = c;
    }
}
