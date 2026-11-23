using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

// Market ekrani: coin ile kalici yukseltme alinir. Urunler ve fiyatlar Progress.Upgrades'ten gelir.
// Ana menude kodla kurulur.
public class Shop : MonoBehaviour
{
    private class Row
    {
        public Progress.Upgrade upgrade;
        public Text tier;
        public Text price;
        public Image button;
    }

    private static readonly Color CanBuy = new Color(0.2f, 0.7f, 0.35f);
    private static readonly Color CannotBuy = new Color(0.35f, 0.38f, 0.5f);
    private static readonly Color Gold = new Color(1f, 0.85f, 0.25f);

    private Row[] rows;
    private Text coinText;
    private UnityAction onChanged;

    public static Shop Create(Canvas canvas, Font font, Sprite boxSprite, UnityAction onChanged)
    {
        // Karartma; arkadaki butonlara tiklanmasini da engeller
        Image dim = UiKit.Image(canvas.transform, "Shop", null, new Color(0f, 0f, 0f, 0.8f));
        dim.raycastTarget = true;
        UiKit.Stretch(dim.rectTransform);

        Shop shop = dim.gameObject.AddComponent<Shop>();
        shop.onChanged = onChanged;
        shop.Build(font, boxSprite);
        dim.gameObject.SetActive(false);
        return shop;
    }

    private void Build(Font font, Sprite boxSprite)
    {
        Image panel = UiKit.Image(transform, "Panel", boxSprite, new Color(0.08f, 0.1f, 0.25f, 0.97f), true);
        UiKit.Place(panel.rectTransform, new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(980, 1560));
        Transform p = panel.transform;

        Text title = UiKit.Text(p, "Title", font, 72, TextAnchor.MiddleCenter, new Color(0.55f, 0.85f, 1f));
        title.text = "SHOP";
        UiKit.Place(title.rectTransform, new Vector2(0.5f, 1), new Vector2(0, -50), new Vector2(600, 100));

        Image coinIcon = UiKit.Image(p, "CoinIcon", GameAssets.I.coinSprite, Color.white);
        UiKit.Place(coinIcon.rectTransform, new Vector2(0.5f, 1), new Vector2(-90, -170), new Vector2(56, 56));
        coinText = UiKit.Text(p, "CoinText", font, 48, TextAnchor.MiddleLeft, Gold);
        UiKit.Place(coinText.rectTransform, new Vector2(0.5f, 1), new Vector2(110, -170), new Vector2(300, 56));

        rows = new Row[Progress.Upgrades.Length];
        for (int i = 0; i < rows.Length; i++)
        {
            Progress.Upgrade upgrade = (Progress.Upgrade)i;
            Progress.UpgradeInfo info = Progress.Upgrades[i];
            float y = -270 - i * 205;

            Image box = UiKit.Image(p, "Row" + i, boxSprite, new Color(1f, 1f, 1f, 0.07f), true);
            UiKit.Place(box.rectTransform, new Vector2(0.5f, 1), new Vector2(0, y), new Vector2(900, 185));
            Transform r = box.transform;

            Text name = UiKit.Text(r, "Name", font, 38, TextAnchor.MiddleLeft, Color.white);
            name.text = info.title;
            UiKit.Place(name.rectTransform, new Vector2(0, 1), new Vector2(30, -18), new Vector2(560, 50));

            Text desc = UiKit.Text(r, "Desc", font, 24, TextAnchor.MiddleLeft, new Color(0.7f, 0.85f, 1f));
            desc.text = info.description;
            UiKit.Place(desc.rectTransform, new Vector2(0, 1), new Vector2(30, -74), new Vector2(560, 36));

            Row row = new Row { upgrade = upgrade };
            row.tier = UiKit.Text(r, "Tier", font, 26, TextAnchor.MiddleLeft, Gold);
            UiKit.Place(row.tier.rectTransform, new Vector2(0, 1), new Vector2(30, -122), new Vector2(560, 40));

            Button buy = UiKit.Button(r, "Buy", "", font, boxSprite, CanBuy, () => Buy(upgrade), 40);
            UiKit.Place((RectTransform)buy.transform, new Vector2(1, 0.5f), new Vector2(-25, 0), new Vector2(250, 125));
            row.button = buy.image;
            row.price = buy.GetComponentInChildren<Text>();
            rows[i] = row;
        }

        Button close = UiKit.Button(p, "Close", "BACK", font, boxSprite, new Color(0.25f, 0.5f, 0.95f), Close, 48);
        UiKit.Place((RectTransform)close.transform, new Vector2(0.5f, 0), new Vector2(0, 45), new Vector2(480, 130));
    }

    public void Open()
    {
        gameObject.SetActive(true);
        transform.SetAsLastSibling();
        Refresh();
    }

    private void Close()
    {
        Sfx.Play(GameAssets.I.uiClick, 0.7f);
        gameObject.SetActive(false);
    }

    private void Buy(Progress.Upgrade upgrade)
    {
        if (Progress.TryBuy(upgrade)) Sfx.Play(GameAssets.I.buy, 0.8f);
        else Sfx.Play(GameAssets.I.shieldBlock, 0.4f, 0.7f);    // yetersiz coin ya da en yuksek kademe

        Refresh();
        if (onChanged != null) onChanged();
    }

    private void Refresh()
    {
        int coins = Progress.Coins;
        coinText.text = coins.ToString();

        foreach (Row row in rows)
        {
            int tier = Progress.Tier(row.upgrade);
            int max = Progress.MaxTier(row.upgrade);
            int cost = Progress.NextCost(row.upgrade);

            row.tier.text = "LEVEL " + tier + " / " + max;
            row.price.text = cost < 0 ? "MAX" : cost.ToString();
            row.button.color = cost >= 0 && coins >= cost ? CanBuy : CannotBuy;
        }
    }
}
