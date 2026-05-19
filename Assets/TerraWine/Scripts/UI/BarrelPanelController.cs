using System;
using System.Collections.Generic;
using TerraWine.Core;
using TerraWine.Data;
using TerraWine.Winery;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace TerraWine.UI
{
    public class BarrelPanelController : MonoBehaviour
    {
        [SerializeField] private TMP_Text messageText;
        [SerializeField] private Transform rowsRoot;
        [SerializeField] private BarrelRowController rowPrefab;
        [SerializeField] private List<BarrelRowController> staticRows = new List<BarrelRowController>();
        [SerializeField] private float timerRefreshSeconds = 1f;

        private readonly List<BarrelRowController> rows = new List<BarrelRowController>();
        private GameSession session;
        private float nextRefreshTime;

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
            if (session?.BarrelSystem != null)
            {
                session.BarrelSystem.BarrelsChanged -= Refresh;
                session.BarrelSystem.MessageRaised -= ShowMessage;
            }

            if (session?.InventorySystem != null)
            {
                session.InventorySystem.InventoryChanged -= Refresh;
            }
        }

        private void Update()
        {
            if (session?.BarrelSystem == null || Time.unscaledTime < nextRefreshTime)
            {
                return;
            }

            nextRefreshTime = Time.unscaledTime + timerRefreshSeconds;
            Refresh();
        }

        public void Refresh()
        {
            if (session?.BarrelSystem == null)
            {
                return;
            }

            session.BarrelSystem.UpdateAging();
            EnsureRows(session.BarrelSystem.Barrels.Count);

            for (int i = 0; i < rows.Count; i++)
            {
                bool active = i < session.BarrelSystem.Barrels.Count;
                rows[i].gameObject.SetActive(active);
                if (active)
                {
                    rows[i].Bind(session, session.BarrelSystem.Barrels[i]);
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
            if (session.BarrelSystem != null)
            {
                session.BarrelSystem.BarrelsChanged += Refresh;
                session.BarrelSystem.MessageRaised += ShowMessage;
            }

            if (session.InventorySystem != null)
            {
                session.InventorySystem.InventoryChanged += Refresh;
            }
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
