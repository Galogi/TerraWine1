using System.Collections.Generic;
using TerraWine.Core;
using TerraWine.Economy;
using TMPro;
using UnityEngine;

namespace TerraWine.UI
{
    public class ShopPanelController : MonoBehaviour
    {
        [SerializeField] private TMP_Text moneyText;
        [SerializeField] private TMP_Text messageText;
        [SerializeField] private Transform rowsRoot;
        [SerializeField] private ShopItemRowController rowPrefab;
        [SerializeField] private List<ShopItemRowController> staticRows = new List<ShopItemRowController>();

        private readonly List<ShopItemRowController> rows = new List<ShopItemRowController>();
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
            if (session?.ShopSystem != null)
            {
                session.ShopSystem.ShopChanged -= Refresh;
                session.ShopSystem.MessageRaised -= ShowMessage;
            }

            if (session?.ResourceSystem != null)
            {
                session.ResourceSystem.ResourcesChanged -= Refresh;
            }
        }

        public void Refresh()
        {
            if (session?.ShopSystem == null)
            {
                return;
            }

            SetText(moneyText, $"Money: {session.ResourceSystem.Money}");
            EnsureRows(session.ShopSystem.Catalog.Count);

            for (int i = 0; i < rows.Count; i++)
            {
                bool active = i < session.ShopSystem.Catalog.Count;
                rows[i].gameObject.SetActive(active);
                if (active)
                {
                    rows[i].Bind(session, session.ShopSystem.Catalog[i]);
                }
            }
        }

        private void Bind()
        {
            if (GameBootstrap.Session == null || session == GameBootstrap.Session)
            {
                return;
            }

            OnDisable();
            session = GameBootstrap.Session;
            session.ShopSystem.ShopChanged += Refresh;
            session.ShopSystem.MessageRaised += ShowMessage;
            session.ResourceSystem.ResourcesChanged += Refresh;
        }

        private void EnsureRows(int count)
        {
            if (rows.Count == 0)
            {
                rows.AddRange(staticRows.FindAll(row => row != null));
            }

            if (rowPrefab == null || rowsRoot == null)
            {
                return;
            }

            while (rows.Count < count)
            {
                rows.Add(Instantiate(rowPrefab, rowsRoot));
            }
        }

        private void ShowMessage(string message)
        {
            SetText(messageText, message);
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
