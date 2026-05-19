using System.Text;
using TerraWine.Core;
using TerraWine.Data;
using TMPro;
using UnityEngine;

namespace TerraWine.UI
{
    public class StoragePanelController : MonoBehaviour
    {
        [SerializeField] private TMP_Text capacityText;
        [SerializeField] private TMP_Text grapesText;
        [SerializeField] private TMP_Text wineBottlesText;
        [SerializeField] private TMP_Text itemsText;

        private GameSession session;

        private void OnEnable()
        {
            Bind();
            Refresh();
        }

        private void Start()
        {
            Bind();
            Refresh();
        }

        private void OnDisable()
        {
            if (session?.StorageSystem != null)
            {
                session.StorageSystem.StorageChanged -= Refresh;
            }

            if (session?.InventorySystem != null)
            {
                session.InventorySystem.InventoryChanged -= Refresh;
            }
        }

        public void Refresh()
        {
            if (session?.StorageSystem == null)
            {
                return;
            }

            SetText(capacityText, $"{session.StorageSystem.UsedCapacity}/{session.StorageSystem.MaxCapacity}");
            SetText(grapesText, FormatStacks(InventoryItemType.Grape));
            SetText(wineBottlesText, FormatStacks(InventoryItemType.WineBottle));
            SetText(itemsText, FormatOtherStacks());
        }

        private void Bind()
        {
            if (GameBootstrap.Session == null || session == GameBootstrap.Session)
            {
                return;
            }

            OnDisable();
            session = GameBootstrap.Session;
            if (session.StorageSystem != null)
            {
                session.StorageSystem.StorageChanged += Refresh;
            }

            if (session.InventorySystem != null)
            {
                session.InventorySystem.InventoryChanged += Refresh;
            }
        }

        private string FormatStacks(InventoryItemType itemType)
        {
            StringBuilder builder = new StringBuilder();
            foreach (InventoryStackData stack in session.StorageSystem.Stacks)
            {
                if (stack.itemType != itemType)
                {
                    continue;
                }

                AppendStack(builder, stack);
            }

            return builder.Length == 0 ? "-" : builder.ToString();
        }

        private string FormatOtherStacks()
        {
            StringBuilder builder = new StringBuilder();
            foreach (InventoryStackData stack in session.StorageSystem.Stacks)
            {
                if (stack.itemType == InventoryItemType.Grape || stack.itemType == InventoryItemType.WineBottle)
                {
                    continue;
                }

                AppendStack(builder, stack);
            }

            return builder.Length == 0 ? "-" : builder.ToString();
        }

        private static void AppendStack(StringBuilder builder, InventoryStackData stack)
        {
            if (builder.Length > 0)
            {
                builder.AppendLine();
            }

            builder.Append(stack.itemId);
            builder.Append(": ");
            builder.Append(stack.amount);
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
