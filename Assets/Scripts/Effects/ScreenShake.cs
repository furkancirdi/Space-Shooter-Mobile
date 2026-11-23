using UnityEngine;

// Kamerayi kisa sureli sarsar. Cinemachine kamerayi her karede yerine koydugu icin
// sarsinti onun ustune, karenin en sonunda eklenir.
[DefaultExecutionOrder(10000)]
public class ScreenShake : MonoBehaviour
{
    private static ScreenShake instance;

    private Transform cam;
    private float amplitude;
    private float duration;
    private float timeLeft;
    private Vector3 appliedOffset;
    private Vector3 positionAfterApply;

    public static void Shake(float amplitude, float duration)
    {
        if (instance == null)
        {
            if (Camera.main == null) return;
            instance = new GameObject("ScreenShake").AddComponent<ScreenShake>();
            instance.cam = Camera.main.transform;
        }

        // Daha guclu sarsinti devam ediyorsa onu bozma
        if (instance.timeLeft > 0f && instance.amplitude * (instance.timeLeft / instance.duration) > amplitude) return;

        instance.amplitude = amplitude;
        instance.duration = duration;
        instance.timeLeft = duration;
    }

    private void LateUpdate()
    {
        if (cam == null) return;

        // Kamerayi bu karede kimse yerine koymadiysa onceki sarsintiyi geri al
        if (appliedOffset != Vector3.zero && cam.position == positionAfterApply)
            cam.position -= appliedOffset;
        appliedOffset = Vector3.zero;

        if (timeLeft <= 0f) return;

        timeLeft -= Time.unscaledDeltaTime;
        float strength = amplitude * Mathf.Clamp01(timeLeft / duration);
        appliedOffset = (Vector3)(Random.insideUnitCircle * strength);
        cam.position += appliedOffset;
        positionAfterApply = cam.position;
    }
}
