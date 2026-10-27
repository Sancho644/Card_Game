using System.Collections.Generic;
using Game.Core.Battle;
using Game.Core.Cards;
using Game.Core.Decks;
using Game.Core.Effects;

namespace Game.Core.Tests
{
    /// <summary>
    /// Small card set for engine tests. Forge, Fire180 and Water200 follow the
    /// «Пример» section of docs/rules.md; the real card set comes with stage 2.
    /// </summary>
    internal static class TestCards
    {
        public const string Plain = "test.field.plain";
        public const string Forge = "test.field.forge";
        public const string Temple = "test.field.temple";

        public const string Fire180 = "test.creature.fire180";
        public const string Water200 = "test.creature.water200";
        public const string Earth250 = "test.creature.earth250";
        public const string Dark250 = "test.creature.dark250";

        public const string Boost50 = "test.ability.boost50";
        public const string Boost40 = "test.ability.boost40";
        public const string Gambit = "test.ability.gambit";
        public const string Gambit2 = "test.ability.gambit2";
        public const string EarthSignature = "test.ability.earth-signature";
        public const string HostBoost = "test.ability.host50";
        public const string GuestBoost = "test.ability.guest50";
        public const string ClosedFieldBoost = "test.ability.closed50";
        public const string FireBoost = "test.ability.fire60";

        public static CardCatalog Catalog() => new CardCatalog(new CardDefinition[]
        {
            new FieldCard(Plain, Rarity.Common, Modifiers((Element.Light, 40))),
            new FieldCard(Forge, Rarity.Epic, Modifiers((Element.Fire, 60), (Element.Water, -30))),
            new FieldCard(Temple, Rarity.Legendary, Modifiers((Element.Light, 80))),

            new CreatureCard(Fire180, Rarity.Common, Element.Fire, 180),
            new CreatureCard(Water200, Rarity.Epic, Element.Water, 200),
            new CreatureCard(Earth250, Rarity.Legendary, Element.Earth, 250),
            new CreatureCard(Dark250, Rarity.Legendary, Element.Dark, 250),

            BattleAbility(Boost50, Rarity.Common, new PowerBonusEffect(50)),
            BattleAbility(Boost40, Rarity.Epic, new PowerBonusEffect(40)),
            BattleAbility(Gambit, Rarity.Legendary),
            BattleAbility(Gambit2, Rarity.Legendary),
            new AbilityCard(EarthSignature, Rarity.Legendary, AbilityTiming.InBattle,
                new[] { new PowerBonusEffect(60) }, boundCreatureId: Earth250),
            BattleAbility(HostBoost, Rarity.Common,
                new PowerBonusEffect(50, new BattleRoleRequirement(BattleRole.Host))),
            BattleAbility(GuestBoost, Rarity.Common,
                new PowerBonusEffect(50, new BattleRoleRequirement(BattleRole.Guest))),
            BattleAbility(ClosedFieldBoost, Rarity.Common,
                new PowerBonusEffect(50, new FieldStateRequirement(mustBeOpen: false))),
            BattleAbility(FireBoost, Rarity.Common,
                new PowerBonusEffect(60, new CreatureElementRequirement(Element.Fire))),
        });

        /// <summary>A valid deck for the default rules; ability slots can be swapped.</summary>
        public static DeckList Deck(
            string commonAbility = Boost50,
            string epicAbility = Boost40,
            string legendaryAbility = Gambit) =>
            new DeckList(new[]
            {
                Plain, Forge, Temple,
                Fire180, Water200, Earth250,
                commonAbility, epicAbility, legendaryAbility,
            });

        private static AbilityCard BattleAbility(string id, Rarity rarity, params IAbilityEffect[] effects) =>
            new AbilityCard(id, rarity, AbilityTiming.InBattle, effects);

        private static Dictionary<Element, int> Modifiers(params (Element element, int value)[] values)
        {
            var modifiers = new Dictionary<Element, int>();
            foreach (var (element, value) in values)
                modifiers[element] = value;
            return modifiers;
        }
    }
}
