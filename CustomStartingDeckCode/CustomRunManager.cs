// Credits for this script to Pikcube in the sts2 modding discord
using MegaCrit.Sts2.Core.Models;

namespace CustomStartingDeck.CustomStartingDeckCode;

public static class CustomRunManager
{
    private static List<Type> AdditionalGoodModifiers { get; } = [];
    private static List<Type> AdditionalBadModifiers { get; } = [];

    public static void Register<T>(CustomRunType runType) where T : ModifierModel
    {
        switch (runType)
        {
            case CustomRunType.Good:
                AdditionalGoodModifiers.Add(typeof(T));
                break;
            case CustomRunType.Bad:
                AdditionalBadModifiers.Add(typeof(T));
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(runType), runType, null);
        }
    }

    public static IEnumerable<ModifierModel> GetGoodModifiers()
    {
        return AdditionalGoodModifiers.Select(type => ModelDb.GetById<ModifierModel>(ModelDb.GetId(type)));
    }

    public static IEnumerable<ModifierModel> GetBadModifiers()
    {
        return AdditionalBadModifiers.Select(type => ModelDb.GetById<ModifierModel>(ModelDb.GetId(type)));
    }
}

public enum CustomRunType
{
    Good = 1,
    Bad = 2,
}