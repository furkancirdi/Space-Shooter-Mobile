using UnityEngine;

public class SpriteAnimationFx : MonoBehaviour
{
    private Sprite[] frames;
    private float frameTime;
    private float timer;
    private int index;
    private SpriteRenderer sr;

    public void Init(Sprite[] animationFrames, float fps)
    {
        frames = animationFrames;
        frameTime = 1f / Mathf.Max(1f, fps);
        sr = GetComponent<SpriteRenderer>();
        sr.sprite = frames[0];
    }

    private void Update()
    {
        timer += Time.deltaTime;
        while (timer >= frameTime)
        {
            timer -= frameTime;
            index++;
            if (index >= frames.Length)
            {
                Destroy(gameObject);
                return;
            }
            sr.sprite = frames[index];
        }
    }
}
