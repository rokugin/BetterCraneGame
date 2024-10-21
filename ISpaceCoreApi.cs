using StardewValley;

namespace BetterCraneGame {
    public interface ISpaceCoreApi {

        List<string> GetVirtualCurrencyList();

        bool IsVirtualCurrencyTeamWide(string currency);

        int GetVirtualCurrencyAmount(Farmer who, string currency);

        void AddToVirtualCurrency(Farmer who, string currency, int amount);

    }
}
