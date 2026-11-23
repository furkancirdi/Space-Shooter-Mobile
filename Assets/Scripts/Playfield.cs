using UnityEngine;

// Kameranin gordugu alanin dunya koordinatlari.
public static class Playfield
{
    private static Camera cam;

    private static Camera Cam
    {
        get
        {
            if (cam == null) cam = Camera.main;
            return cam;
        }
    }

    public static float Left => At(0f, 0f).x;
    public static float Right => At(1f, 1f).x;
    public static float Bottom => At(0f, 0f).y;
    public static float Top => At(1f, 1f).y;

    // Viewport (0-1) koordinatini dunya konumuna cevirir
    public static Vector2 At(float vx, float vy)
    {
        return Cam.ViewportToWorldPoint(new Vector3(vx, vy, 0f));
    }

    public static bool IsOutside(Vector2 pos, float margin)
    {
        return pos.x < Left - margin || pos.x > Right + margin || pos.y < Bottom - margin || pos.y > Top + margin;
    }
}
