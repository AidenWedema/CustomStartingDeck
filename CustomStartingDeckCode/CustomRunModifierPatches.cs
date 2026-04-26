// Credits for this script to Pikcube in the sts2 modding discord
using HarmonyLib;
using MegaCrit.Sts2.Core.Models;

namespace CustomStartingDeck.CustomStartingDeckCode;

public static class CustomRunModifierPatches
{
    [HarmonyPatch(typeof(ModelDb), nameof(ModelDb.GoodModifiers), MethodType.Getter)]
    public static class GoodModifierPatches
    {
        // [UsedImplicitly]
        public static IReadOnlyList<ModifierModel> Postfix(IReadOnlyList<ModifierModel> __result)
        {
            List<ModifierModel> allGood = [.. __result, .. CustomRunManager.GetGoodModifiers()];
            return allGood.AsReadOnly();
        }
    }

    [HarmonyPatch(typeof(ModelDb), nameof(ModelDb.BadModifiers), MethodType.Getter)]
    public static class BadModifierPatches
    {
        // [UsedImplicitly]
        public static IReadOnlyList<ModifierModel> Postfix(IReadOnlyList<ModifierModel> __result)
        {
            List<ModifierModel> allGood = [.. __result, .. CustomRunManager.GetBadModifiers()];
            return allGood.AsReadOnly();
        }
    }
}