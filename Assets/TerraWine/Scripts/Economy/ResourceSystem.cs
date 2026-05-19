using System;
using TerraWine.Core;
using TerraWine.Data;

namespace TerraWine.Economy
{
    public class ResourceSystem : IGameSystem
    {
        private GameData data;

        public event Action ResourcesChanged;

        public int Money => data.resources.money;
        public int Water => data.resources.water;
        public int Wood => data.resources.wood;
        public int Metal => data.resources.metal;

        public void Initialize(GameSession session, GameData gameData)
        {
            data = gameData;
        }

        public int Get(ResourceType resourceType)
        {
            return resourceType switch
            {
                ResourceType.Money => data.resources.money,
                ResourceType.Water => data.resources.water,
                ResourceType.Wood => data.resources.wood,
                ResourceType.Metal => data.resources.metal,
                _ => 0
            };
        }

        public void Add(ResourceType resourceType, int amount)
        {
            if (amount <= 0)
            {
                return;
            }

            Set(resourceType, Get(resourceType) + amount);
        }

        public bool Spend(ResourceType resourceType, int amount)
        {
            if (amount <= 0 || Get(resourceType) < amount)
            {
                return false;
            }

            Set(resourceType, Get(resourceType) - amount);
            return true;
        }

        private void Set(ResourceType resourceType, int amount)
        {
            switch (resourceType)
            {
                case ResourceType.Money:
                    data.resources.money = amount;
                    break;
                case ResourceType.Water:
                    data.resources.water = amount;
                    break;
                case ResourceType.Wood:
                    data.resources.wood = amount;
                    break;
                case ResourceType.Metal:
                    data.resources.metal = amount;
                    break;
            }

            ResourcesChanged?.Invoke();
        }
    }
}
