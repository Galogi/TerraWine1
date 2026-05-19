using System;
using System.Collections.Generic;
using TerraWine.Core;
using TerraWine.Data;

namespace TerraWine.Inventory
{
    public class InventorySystem : IGameSystem
    {
        private GameSession session;
        private GameData data;

        public event Action InventoryChanged;

        public IReadOnlyList<InventoryStackData> Stacks => data.inventory.stacks;

        public void Initialize(GameSession gameSession, GameData gameData)
        {
            session = gameSession;
            data = gameData;
        }

        public int GetAmount(string itemId)
        {
            InventoryStackData stack = FindStack(itemId);
            return stack == null ? 0 : stack.amount;
        }

        public void AddItem(string itemId, InventoryItemType itemType, int amount)
        {
            TryAddItem(itemId, itemType, amount);
        }

        public bool TryAddItem(string itemId, InventoryItemType itemType, int amount)
        {
            if (string.IsNullOrWhiteSpace(itemId) || amount <= 0)
            {
                return false;
            }

            if (session?.StorageSystem != null && !session.StorageSystem.HasSpace(amount))
            {
                return false;
            }

            InventoryStackData stack = FindStack(itemId);
            if (stack == null)
            {
                stack = new InventoryStackData
                {
                    itemId = itemId,
                    itemType = itemType,
                    amount = 0
                };
                data.inventory.stacks.Add(stack);
            }

            stack.amount += amount;
            data.storage.usedCapacity += amount;
            InventoryChanged?.Invoke();
            session?.StorageSystem?.NotifyStorageChanged();
            return true;
        }

        public bool RemoveItem(string itemId, int amount)
        {
            InventoryStackData stack = FindStack(itemId);
            if (stack == null || amount <= 0 || stack.amount < amount)
            {
                return false;
            }

            stack.amount -= amount;
            data.storage.usedCapacity = Math.Max(0, data.storage.usedCapacity - amount);
            if (stack.amount == 0)
            {
                data.inventory.stacks.Remove(stack);
            }

            InventoryChanged?.Invoke();
            session?.StorageSystem?.NotifyStorageChanged();
            return true;
        }

        private InventoryStackData FindStack(string itemId)
        {
            return data.inventory.stacks.Find(stack => stack.itemId == itemId);
        }
    }
}
