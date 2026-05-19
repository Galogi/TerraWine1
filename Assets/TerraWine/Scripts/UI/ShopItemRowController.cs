using TerraWine.Core;
using TerraWine.Economy;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace TerraWine.UI
{
    public class ShopItemRowController : MonoBehaviour
    {
        [SerializeField] private TMP_Text nameText;
        [SerializeField] private TMP_Text categoryText;
        [SerializeField] private TMP_Text priceText;
        [SerializeField] private TMP_Text descriptionText;
        [SerializeField] private Button buyButton;

        private GameSession session;
        private ShopItemRuntimeDefinition item;

        private void Awake()
        {
            if (buyButton != null)
            {
                buyButton.onClick.AddListener(Buy);
            }
        }

        public void Bind(GameSession gameSession, ShopItemRuntimeDefinition shopItem)
        {
            session = gameSession;
            item = shopItem;
            Refresh();
        }

        private void Refresh()
        {
            if (item == null)
            {
                return;
            }

            SetText(nameText, item.DisplayName);
            SetText(categoryText, item.Category.ToString());
            SetText(priceText, $"${item.Price}");
            SetText(descriptionText, item.Description);
            if (buyButton != null)
            {
                buyButton.interactable = session?.ResourceSystem != null && session.ResourceSystem.Money >= item.Price;
            }
        }

        private void Buy()
        {
            if (item != null)
            {
                session?.ShopSystem.Buy(item.ItemId);
            }
        }

        private static void SetText(TMP_Text target, string value)
        {
            if (target != null)
            {
                target.text = value;
            }
        }
    }
}
