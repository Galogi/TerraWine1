using System;
using System.Collections.Generic;
using TerraWine.Core;
using TerraWine.Data;

namespace TerraWine.WorldMap
{
    public class WorldMapSystem : IGameSystem
    {
        private readonly List<WorldMapNodeRuntimeDefinition> catalog = new List<WorldMapNodeRuntimeDefinition>
        {
            new WorldMapNodeRuntimeDefinition("water_source_near", "Water Source", WorldMapNodeType.WaterSource, WorldMapDistance.Near, ResourceType.Water, 8, 1, 10),
            new WorldMapNodeRuntimeDefinition("forest_near", "Forest", WorldMapNodeType.Forest, WorldMapDistance.Near, ResourceType.Wood, 6, 1, 0),
            new WorldMapNodeRuntimeDefinition("metal_mine_mid", "Metal Mine", WorldMapNodeType.MetalMine, WorldMapDistance.Mid, ResourceType.Metal, 4, 1, 0),
            new WorldMapNodeRuntimeDefinition("old_ruins", "Old Ruins", WorldMapNodeType.Ruins, WorldMapDistance.Mid, ResourceType.Money, 0, 1, 0)
        };

        private readonly Random random = new Random();
        private GameSession session;
        private GameData data;

        public event Action WorldMapChanged;
        public event Action<string> MessageRaised;

        public IReadOnlyList<WorldMapNodeRuntimeDefinition> Catalog => catalog;
        public string LastMessage { get; private set; } = string.Empty;

        public void Initialize(GameSession gameSession, GameData gameData)
        {
            session = gameSession;
            data = gameData;
            EnsureStartingMap();
            UpdateWorldTasks();
        }

        public bool StartNodeAction(string nodeId)
        {
            WorldMapNodeRuntimeDefinition node = GetNode(nodeId);
            if (node == null)
            {
                RaiseMessage("World map node not found.");
                return false;
            }

            if (session.DailyActionSystem.ActionsRemaining < node.ActionCost)
            {
                RaiseMessage("No daily world actions remaining.");
                return false;
            }

            if (!session.DailyActionSystem.SpendAction(node.ActionCost))
            {
                RaiseMessage("No daily world actions remaining.");
                return false;
            }

            WorldMapNodeData nodeData = GetNodeData(node.NodeId);
            nodeData.timesUsed++;
            nodeData.lastUsedAtUtc = session.TimeSystem.ToSaveString(session.TimeSystem.UtcNow);

            if (node.NodeType == WorldMapNodeType.WaterSource && node.DurationSeconds > 0)
            {
                StartTimedGathering(node);
                RaiseMessage($"Started gathering from {node.DisplayName}.");
                WorldMapChanged?.Invoke();
                return true;
            }

            ApplyNodeReward(node);
            WorldMapChanged?.Invoke();
            return true;
        }

        public bool CollectCompletedWorldTask(string taskId)
        {
            UpdateWorldTasks();
            TimedTaskData task = data.timedTasks.Find(item => item.taskId == taskId && item.taskType == TimedTaskType.WorldMapGathering);
            if (task == null)
            {
                RaiseMessage("World map task not found.");
                return false;
            }

            if (!task.isComplete)
            {
                RaiseMessage("World map task is still in progress.");
                return false;
            }

            WorldMapNodeRuntimeDefinition node = GetNode(task.definitionId);
            if (node == null)
            {
                RaiseMessage("World map node data is missing.");
                return false;
            }

            session.ResourceSystem.Add(node.ResourceType, task.amount);
            data.timedTasks.Remove(task);
            RaiseMessage($"Collected {task.amount} {node.ResourceType} from {node.DisplayName}.");
            WorldMapChanged?.Invoke();
            return true;
        }

        public bool CollectFirstCompletedWorldTask()
        {
            TimedTaskData task = GetWorldTasks().Find(item => item.isComplete);
            if (task == null)
            {
                RaiseMessage("No completed world map task to collect.");
                return false;
            }

            return CollectCompletedWorldTask(task.taskId);
        }

        public void UpdateWorldTasks()
        {
            DateTime now = session.TimeSystem.UtcNow;
            bool changed = false;
            foreach (TimedTaskData task in data.timedTasks)
            {
                if (task.taskType != TimedTaskType.WorldMapGathering || task.isComplete)
                {
                    continue;
                }

                if (session.TimeSystem.TryParseSaveTime(task.completesAtUtc, out DateTime completesAt) && completesAt <= now)
                {
                    task.isComplete = true;
                    changed = true;
                }
            }

            if (changed)
            {
                WorldMapChanged?.Invoke();
            }
        }

        public List<TimedTaskData> GetWorldTasks()
        {
            return data.timedTasks.FindAll(task => task.taskType == TimedTaskType.WorldMapGathering);
        }

        public TimeSpan GetTimeRemaining(TimedTaskData task)
        {
            if (task == null || task.isComplete || !session.TimeSystem.TryParseSaveTime(task.completesAtUtc, out DateTime completesAt))
            {
                return TimeSpan.Zero;
            }

            TimeSpan remaining = completesAt - session.TimeSystem.UtcNow;
            return remaining < TimeSpan.Zero ? TimeSpan.Zero : remaining;
        }

        public WorldMapNodeRuntimeDefinition GetNode(string nodeId)
        {
            return catalog.Find(node => node.NodeId == nodeId);
        }

        private void StartTimedGathering(WorldMapNodeRuntimeDefinition node)
        {
            DateTime now = session.TimeSystem.UtcNow;
            data.timedTasks.Add(new TimedTaskData
            {
                taskType = TimedTaskType.WorldMapGathering,
                definitionId = node.NodeId,
                ownerId = data.player.playerId,
                startedAtUtc = session.TimeSystem.ToSaveString(now),
                completesAtUtc = session.TimeSystem.ToSaveString(now.AddSeconds(GetDurationForDistance(node))),
                amount = node.BaseReward,
                isComplete = false
            });
        }

        private int GetDurationForDistance(WorldMapNodeRuntimeDefinition node)
        {
            if (node.NodeType != WorldMapNodeType.WaterSource)
            {
                return Math.Max(0, node.DurationSeconds);
            }

            return node.Distance switch
            {
                WorldMapDistance.Near => 10,
                WorldMapDistance.Mid => 20,
                WorldMapDistance.Far => 30,
                _ => Math.Max(10, node.DurationSeconds)
            };
        }

        private void ApplyNodeReward(WorldMapNodeRuntimeDefinition node)
        {
            if (node.NodeType == WorldMapNodeType.Ruins)
            {
                ApplyRuinsReward();
                return;
            }

            session.ResourceSystem.Add(node.ResourceType, node.BaseReward);
            RaiseMessage($"Gathered {node.BaseReward} {node.ResourceType} from {node.DisplayName}.");
        }

        private void ApplyRuinsReward()
        {
            int roll = random.Next(0, 5);
            switch (roll)
            {
                case 0:
                    session.ResourceSystem.Add(ResourceType.Money, 35);
                    data.worldMap.ruinsRewardHistory.Add("money:35");
                    RaiseMessage("Excavated ruins and found 35 money.");
                    break;
                case 1:
                    session.ResourceSystem.Add(ResourceType.Wood, 5);
                    data.worldMap.ruinsRewardHistory.Add("wood:5");
                    RaiseMessage("Excavated ruins and found 5 wood.");
                    break;
                case 2:
                    session.ResourceSystem.Add(ResourceType.Metal, 3);
                    data.worldMap.ruinsRewardHistory.Add("metal:3");
                    RaiseMessage("Excavated ruins and found 3 metal.");
                    break;
                case 3:
                    session.ResourceSystem.Add(ResourceType.Water, 6);
                    data.worldMap.ruinsRewardHistory.Add("water:6");
                    RaiseMessage("Excavated ruins and found 6 water.");
                    break;
                default:
                    if (session.InventorySystem.TryAddItem("rare_ruins_relic", InventoryItemType.SpecialItem, 1))
                    {
                        data.worldMap.ruinsRewardHistory.Add("item:rare_ruins_relic");
                        RaiseMessage("Excavated ruins and found a rare relic.");
                    }
                    else
                    {
                        session.ResourceSystem.Add(ResourceType.Money, 20);
                        data.worldMap.ruinsRewardHistory.Add("money:20");
                        RaiseMessage("Storage was full, so the ruins yielded 20 money.");
                    }
                    break;
            }
        }

        private void EnsureStartingMap()
        {
            data.worldMap.wineryLocation.isPlaced = true;
            foreach (WorldMapNodeRuntimeDefinition node in catalog)
            {
                if (GetNodeData(node.NodeId) == null)
                {
                    data.worldMap.nodes.Add(new WorldMapNodeData { nodeId = node.NodeId });
                }
            }
        }

        private WorldMapNodeData GetNodeData(string nodeId)
        {
            return data.worldMap.nodes.Find(node => node.nodeId == nodeId);
        }

        private void RaiseMessage(string message)
        {
            LastMessage = message;
            MessageRaised?.Invoke(message);
        }
    }

    public class WorldMapNodeRuntimeDefinition
    {
        public WorldMapNodeRuntimeDefinition(string nodeId, string displayName, WorldMapNodeType nodeType, WorldMapDistance distance, ResourceType resourceType, int baseReward, int actionCost, int durationSeconds)
        {
            NodeId = nodeId;
            DisplayName = displayName;
            NodeType = nodeType;
            Distance = distance;
            ResourceType = resourceType;
            BaseReward = baseReward;
            ActionCost = actionCost;
            DurationSeconds = durationSeconds;
        }

        public string NodeId { get; }
        public string DisplayName { get; }
        public WorldMapNodeType NodeType { get; }
        public WorldMapDistance Distance { get; }
        public ResourceType ResourceType { get; }
        public int BaseReward { get; }
        public int ActionCost { get; }
        public int DurationSeconds { get; }
    }
}
