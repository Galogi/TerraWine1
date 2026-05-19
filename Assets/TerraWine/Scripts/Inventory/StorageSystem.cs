using System;
using System.Collections.Generic;
using TerraWine.Core;
using TerraWine.Data;

namespace TerraWine.Inventory
{
    public class StorageSystem : IGameSystem
    {
        private GameData data;

        public event Action StorageChanged;

        public int UsedCapacity => data.storage.usedCapacity;
        public int MaxCapacity => data.storage.maxCapacity;
        public int FreeCapacity => Math.Max(0, MaxCapacity - UsedCapacity);
        public IReadOnlyList<InventoryStackData> Stacks => data.inventory.stacks;

        public void Initialize(GameSession session, GameData gameData)
        {
            data = gameData;
            RecalculateUsedCapacity();
        }

        public bool HasSpace(int amount)
        {
            return amount <= FreeCapacity;
        }

        public void IncreaseCapacity(int amount)
        {
            if (amount <= 0)
            {
                return;
            }

            data.storage.maxCapacity += amount;
            StorageChanged?.Invoke();
        }

        public void NotifyStorageChanged()
        {
            StorageChanged?.Invoke();
        }

        private void RecalculateUsedCapacity()
        {
            int total = 0;
            foreach (InventoryStackData stack in data.inventory.stacks)
            {
                total += Math.Max(0, stack.amount);
            }

            data.storage.usedCapacity = total;
        }
    }
}
