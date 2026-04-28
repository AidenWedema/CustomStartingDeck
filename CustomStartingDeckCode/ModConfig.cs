using BaseLib.Config;

namespace CustomStartingDeck.CustomStartingDeckCode;

public class ModConfig : SimpleModConfig
{
    [ConfigHoverTip]
    public static int DeckSize { get; set; } = 10;
    
    [ConfigHoverTip]
    public static bool AllowDuplicates { get; set; } = false;
    
    [ConfigHoverTip]
    public static bool StartRelics { get; set; } = false;
}