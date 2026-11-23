using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

// Kodla arayuz kurarken kullanilan kucuk yardimcilar (menu ve market).
public static class UiKit
{
    public static Text Text(Transform parent, string name, Font font, int size, TextAnchor anchor, Color color)
    {
        GameObject go = new GameObject(name, typeof(RectTransform), typeof(Text));
        go.transform.SetParent(parent, false);
        Text t = go.GetComponent<Text>();
        t.font = font;
        t.fontSize = size;
        t.alignment = anchor;
        t.color = color;
        t.horizontalOverflow = HorizontalWrapMode.Overflow;
        t.verticalOverflow = VerticalWrapMode.Overflow;
        t.raycastTarget = false;
        return t;
    }

    public static Image Image(Transform parent, string name, Sprite sprite, Color color, bool sliced = false)
    {
        GameObject go = new GameObject(name, typeof(RectTransform), typeof(Image));
        go.transform.SetParent(parent, false);
        Image img = go.GetComponent<Image>();
        img.sprite = sprite;
        if (sliced) img.type = UnityEngine.UI.Image.Type.Sliced;
        img.color = color;
        img.raycastTarget = false;
        return img;
    }

    public static Button Button(Transform parent, string name, string label, Font font, Sprite sprite, Color color, UnityAction onClick, int fontSize = 56)
    {
        Image img = Image(parent, name, sprite, color, true);
        img.raycastTarget = true;
        Button button = img.gameObject.AddComponent<Button>();
        button.onClick.AddListener(onClick);

        Text t = Text(img.transform, "Label", font, fontSize, TextAnchor.MiddleCenter, Color.white);
        t.text = label;
        Stretch(t.rectTransform);
        return button;
    }

    // Kose/kenar capasina gore yerlestirir (pivot = capa)
    public static void Place(RectTransform rt, Vector2 anchor, Vector2 position, Vector2 size)
    {
        rt.anchorMin = rt.anchorMax = rt.pivot = anchor;
        rt.anchoredPosition = position;
        rt.sizeDelta = size;
    }

    public static void Stretch(RectTransform rt)
    {
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = rt.offsetMax = Vector2.zero;
    }
}
