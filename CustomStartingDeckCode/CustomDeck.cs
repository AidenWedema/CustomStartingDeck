using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Models.Relics;
using MegaCrit.Sts2.Core.Nodes.CommonUi;
using MegaCrit.Sts2.Core.Rewards;

namespace CustomStartingDeck.CustomStartingDeckCode;

public class CustomDeck : ModifierModel
{
    public override bool ClearsPlayerDeck => true;

    private static IEnumerable<CardModel> blacklistedCards =>
    [
        ModelDb.Card<DeprecatedCard>(),         // Obviously we can't have the depricated card
        ModelDb.Card<FranticEscape>(),          // This will probably either go horribly wrong when played, or absolutely nothing happens
        // The mind demon effect cards don't end up in your deck, so they should not be selectable
        ModelDb.Card<MindRot>(),                
        ModelDb.Card<Disintegration>(),
        ModelDb.Card<WasteAway>(),
        ModelDb.Card<Sloth>(),
    ];

    private static IEnumerable<RelicModel> blacklistedRelics =>
    [
        ModelDb.Relic<DeprecatedRelic>()
    ];

    public override Func<Task> GenerateNeowOption(EventModel eventModel)
    {
        return (Func<Task>) (() => ChooseCards(eventModel.Owner!));
    }

    private static async Task ChooseCards(Player player)
    {
        // Get all cards
        var cardpool = ModelDb.AllCards.Except(blacklistedCards).ToList();

        // Remove all cards without a type
        cardpool = cardpool.FindAll((c) => c.Type != CardType.None);
        
        // Sort cards by character (ironclad, silent, regent, necrobinder, defect, colorless, ect) -> rarity (basic, common, uncommon, rare, ancient) -> type (attack, skill, power) -> alphabet
        var characterOrder = new Dictionary<string, int>
        {
            ["ironclad"] = 0,
            ["silent"] = 1,
            ["regent"] = 2,
            ["necrobinder"] = 3,
            ["defect"] = 4,
            ["colorless"] = 5,
            ["token"] = 6,
            ["event"] = 7,
            ["quest"] = 8,
            ["status"] = 9,
            ["curse"] = 10
        };
        
        var rarityOrder = new Dictionary<CardRarity, int>
        {
            [CardRarity.Basic] = 0,
            [CardRarity.Common] = 1,
            [CardRarity.Uncommon] = 2,
            [CardRarity.Rare] = 3,
            [CardRarity.Ancient] = 4
        };
        
        var typeOrder = new Dictionary<CardType, int>
        {
            [CardType.Attack] = 0,
            [CardType.Skill] = 1,
            [CardType.Power] = 2
        };
        
        cardpool = cardpool
            .OrderBy(c => characterOrder.GetValueOrDefault(c.Pool.Title, int.MaxValue))
            .ThenBy(c => rarityOrder.GetValueOrDefault(c.Rarity, int.MaxValue))
            .ThenBy(c => typeOrder.GetValueOrDefault(c.Type, int.MaxValue))
            .ThenBy(c => c.Title)
            .ToList();
        
        // Prompt the player to pick a card until their deck is full
        var amountOfCards = ModConfig.DeckSize;
        if (!ModConfig.AllowDuplicates) await SelectCard(player, cardpool, amountOfCards);  // If duplicates are not allowed, select all cards in the deck at once
        else for (var i = 0; i < amountOfCards; i++)    // If duplicates are allowed, pick every card separately
            await SelectCard(player, cardpool, 1);
        
        // Show a relic reward if StartRelics is enabled
        if (ModConfig.StartRelics)
        {
            // Get all relics
            var relicpool = ModelDb.AllRelics.Except(blacklistedRelics).ToList();
            // Make a list of relic rewards
            List<Reward> relicRewards = [];
            relicRewards.AddRange(relicpool.Select(relic => new RelicReward(relic.ToMutable(), player)));
            // Show the rewards selection
            await new RewardsSet(player).WithCustomRewards(relicRewards).Offer();
        }
    }

    /// <summary>
    /// Open the card selection grid
    /// </summary>
    /// <param name="player">The current player</param>
    /// <param name="cards">A list of all CardModel to show in the selection</param>
    /// <param name="amount">The amount of cards to select</param>
    private static async Task SelectCard(Player player, List<CardModel> cards, int amount)
    {
        // Get a list of CardCreationResults (done like this because CardFactory.CreateForReward keeps throwing errors and does things that are irrelevant for this)
        // The list is created here because otherwise duplicate cards would not be possible as it would add the card instance that is already in your deck to your deck again.
        List<CardCreationResult> list = [];
        list.AddRange(cards.Select(card => new CardCreationResult(player.RunState.CreateCard(card, player))));
        
        // Show the card selection
        CardSelectorPrefs prefs = new CardSelectorPrefs(new LocString("modifiers", "CUSTOM_DECK.selectionPrompt"), amount)
        {
            Cancelable = false,
            RequireManualConfirmation = true
        };
        CardCmd.PreviewCardPileAdd(await CardPileCmd.Add((await CardSelectCmd.FromSimpleGridForRewards(new BlockingPlayerChoiceContext(), list.ToList(), player, prefs)).ToList(), PileType.Deck), style: CardPreviewStyle.GridLayout);
    }
}