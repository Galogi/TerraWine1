using TerraWine.Data;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace TerraWine.UI
{
    public class TheftBotRowController : MonoBehaviour
    {
        [SerializeField] private TMP_Text nameText;
        [SerializeField] private TMP_Text vaultText;
        [SerializeField] private TMP_Text recipesText;
        [SerializeField] private TMP_Text defenseText;
        [SerializeField] private Button selectButton;

        private TheftPanelController panel;
        private BotWineryData bot;

        private void Awake()
        {
            if (selectButton != null)
            {
                selectButton.onClick.AddListener(Select);
            }
        }

        public void Bind(TheftPanelController owner, BotWineryData botData, bool selected)
        {
            panel = owner;
            bot = botData;
            SetText(nameText, selected ? $"> {bot.displayName}" : bot.displayName);
            SetText(vaultText, $"{bot.vaultDigits} digits");
            SetText(recipesText, bot.ownedRecipeIds.Count == 0 ? "-" : string.Join(", ", bot.ownedRecipeIds));
            SetText(defenseText, $"Defense {bot.defenseLevel}");
        }

        private void Select()
        {
            if (bot != null)
            {
                panel?.SelectBot(bot);
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
