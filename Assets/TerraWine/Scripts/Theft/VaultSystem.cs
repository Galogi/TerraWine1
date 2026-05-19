using TerraWine.Core;
using TerraWine.Data;

namespace TerraWine.Theft
{
    public class VaultSystem : IGameSystem
    {
        private GameData data;

        public int PlayerVaultDigits => data.winery.vaultDigits;
        public int PlayerVaultLevel => data.winery.vaultLevel;

        public void Initialize(GameSession session, GameData gameData)
        {
            data = gameData;
            if (data.winery.vaultDigits < 3)
            {
                data.winery.vaultDigits = 3;
            }

            if (data.winery.vaultLevel < 1)
            {
                data.winery.vaultLevel = 1;
            }

            if (string.IsNullOrWhiteSpace(data.winery.vaultPassword))
            {
                data.winery.vaultPassword = "123";
            }
        }

        public bool ValidatePlayerPassword(string guess)
        {
            return ValidatePassword(guess, data.winery.vaultPassword, data.winery.vaultDigits);
        }

        public bool ValidateBotPassword(BotWineryData bot, string guess)
        {
            return bot != null && ValidatePassword(guess, bot.vaultPassword, bot.vaultDigits);
        }

        public bool UpgradePlayerVault(int targetDigits)
        {
            if (targetDigits < 3 || targetDigits > 5 || targetDigits <= data.winery.vaultDigits)
            {
                return false;
            }

            data.winery.vaultDigits = targetDigits;
            data.winery.vaultLevel = targetDigits - 2;
            if (data.winery.vaultPassword.Length != targetDigits)
            {
                data.winery.vaultPassword = targetDigits == 4 ? "1234" : "12345";
            }

            return true;
        }

        private static bool ValidatePassword(string guess, string password, int digits)
        {
            return !string.IsNullOrWhiteSpace(guess)
                && !string.IsNullOrWhiteSpace(password)
                && guess.Length == digits
                && guess == password;
        }
    }
}
