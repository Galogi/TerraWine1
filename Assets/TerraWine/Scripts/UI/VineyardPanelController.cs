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
    public class VineyardPanelController : MonoBehaviour
    {
        [SerializeField] private TMP_Text messageText;
        [SerializeField] private TMP_Text tutorialText;
        [SerializeField] private Transform rowsRoot;
        [SerializeField] private VineyardPlotRowController rowPrefab;
        [SerializeField] private List<VineyardPlotRowController> staticRows = new List<VineyardPlotRowController>();

        private readonly List<VineyardPlotRowController> rows = new List<VineyardPlotRowController>();
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
            if (session?.VineyardSystem != null)
            {
                session.VineyardSystem.VineyardChanged -= Refresh;
                session.VineyardSystem.MessageRaised -= ShowMessage;
            }

            if (session?.InventorySystem != null)
            {
                session.InventorySystem.InventoryChanged -= Refresh;
            }

            if (session?.ResourceSystem != null)
            {
                session.ResourceSystem.ResourcesChanged -= Refresh;
            }

            if (session?.TutorialSystem != null)
            {
                session.TutorialSystem.TutorialChanged -= Refresh;
            }
        }

        public void Refresh()
        {
            if (session?.VineyardSystem == null)
            {
                return;
            }

            session.VineyardSystem.UpdateGrowth();
            SetText(tutorialText, session.TutorialSystem?.CurrentInstruction ?? string.Empty);
            EnsureRows(session.VineyardSystem.Plots.Count);

            for (int i = 0; i < rows.Count; i++)
            {
                if (i < session.VineyardSystem.Plots.Count)
                {
                    rows[i].gameObject.SetActive(true);
                    rows[i].Bind(session, session.VineyardSystem.Plots[i]);
                }
                else
                {
                    rows[i].gameObject.SetActive(false);
                }
            }
        }

        private void Bind()
        {
            if (session == GameBootstrap.Session || GameBootstrap.Session == null)
            {
                return;
            }

            OnDisable();
            session = GameBootstrap.Session;
            if (session.VineyardSystem != null)
            {
                session.VineyardSystem.VineyardChanged += Refresh;
                session.VineyardSystem.MessageRaised += ShowMessage;
            }

            if (session.InventorySystem != null)
            {
                session.InventorySystem.InventoryChanged += Refresh;
            }

            if (session.ResourceSystem != null)
            {
                session.ResourceSystem.ResourcesChanged += Refresh;
            }

            if (session.TutorialSystem != null)
            {
                session.TutorialSystem.TutorialChanged += Refresh;
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
