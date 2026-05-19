using TerraWine.Core;
using TerraWine.Data;
using TerraWine.WorldMap;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace TerraWine.UI
{
    public class WorldMapNodeRowController : MonoBehaviour
    {
        [SerializeField] private TMP_Text nameText;
        [SerializeField] private TMP_Text typeText;
        [SerializeField] private TMP_Text distanceText;
        [SerializeField] private TMP_Text rewardText;
        [SerializeField] private TMP_Text actionCostText;
        [SerializeField] private Button actionButton;

        private GameSession session;
        private WorldMapNodeRuntimeDefinition node;

        private void Awake()
        {
            if (actionButton != null)
            {
                actionButton.onClick.AddListener(StartAction);
            }
        }

        public void Bind(GameSession gameSession, WorldMapNodeRuntimeDefinition nodeData)
        {
            session = gameSession;
            node = nodeData;
            Refresh();
        }

        private void Refresh()
        {
            if (node == null)
            {
                return;
            }

            SetText(nameText, node.DisplayName);
            SetText(typeText, node.NodeType.ToString());
            SetText(distanceText, node.Distance.ToString());
            SetText(rewardText, node.NodeType == WorldMapNodeType.Ruins ? "Random" : $"{node.BaseReward} {node.ResourceType}");
            SetText(actionCostText, node.ActionCost.ToString());
            if (actionButton != null)
            {
                actionButton.interactable = session?.DailyActionSystem != null && session.DailyActionSystem.ActionsRemaining >= node.ActionCost;
            }
        }

        private void StartAction()
        {
            if (node != null)
            {
                session?.WorldMapSystem.StartNodeAction(node.NodeId);
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
