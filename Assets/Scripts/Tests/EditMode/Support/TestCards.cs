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
        public const string DoublingLake = "test.field.doubling-lake";
        public const string HumbleHall = "test.field.humble-hall";
        public const string SilenceField = "test.field.silence";
        public const string HostRightField = "test.field.host-right";
        public const string StingyField = "test.field.stingy";
        public const string SealField = "test.field.seal";
        public const string NarrowField = "test.field.narrow";
        public const string MercyField = "test.field.mercy";
        public const string UnbreakableField = "test.field.unbreakable";
        public const string SacrificeField = "test.field.sacrifice";
        public const string TrophyField = "test.field.trophy";
        public const string HostHomeField = "test.field.host-home";

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
        public const string Desperation = "test.ability.desperation60";
        public const string Mirror = "test.ability.mirror";
        public const string CancelCondition = "test.ability.cancel-condition";
        public const string FieldBlessing = "test.ability.field-blessing";
        public const string ForcedReveal = "test.ability.forced-reveal";
        public const string Intercept = "test.ability.intercept";
        public const string Survival = "test.ability.survival";
        public const string LastWord = "test.ability.last-word";
        public const string TieBreaker = "test.ability.tie-breaker";
        public const string Blockade = "test.ability.blockade";

        public const string Maneuver = "test.ability.maneuver";
        public const string Displacement = "test.ability.displacement";
        public const string Retreat = "test.ability.retreat";
        public const string SecondLife = "test.ability.second-life";
        public const string ReturnPower = "test.ability.return-power";
        public const string Desolation = "test.ability.desolation";
        public const string Scout = "test.ability.scout";
        public const string LockField = "test.ability.lock-field";

        public const string ForgeFlame = "test.ability.forge-flame";
        public const string StormRush = "test.ability.storm-rush";
        public const string Dawn = "test.ability.dawn";
        public const string DarkPrice = "test.ability.dark-price";

        public static CardCatalog Catalog() => new CardCatalog(new CardDefinition[]
        {
            new FieldCard(Plain, Rarity.Common, Modifiers((Element.Light, 40))),
            new FieldCard(Forge, Rarity.Epic, Modifiers((Element.Fire, 60), (Element.Water, -30))),
            new FieldCard(Temple, Rarity.Legendary, Modifiers((Element.Light, 80))),
            new FieldCard(DoublingLake, Rarity.Epic, Modifiers((Element.Fire, 50), (Element.Water, -20)),
                new[] { new DoubleFieldValuesCondition() }),
            new FieldCard(HumbleHall, Rarity.Legendary, Modifiers((Element.Fire, 60)),
                new[] { new LowerPowerWinsCondition() }),
            ConditionField(SilenceField, Rarity.Epic, new NoCommonOrEpicAbilitiesCondition()),
            ConditionField(HostRightField, Rarity.Epic, new HostOnlyAbilitiesCondition()),
            ConditionField(StingyField, Rarity.Epic, new OneAbilityPerPlayerCondition()),
            ConditionField(SealField, Rarity.Epic, new NoLegendaryAbilitiesCondition()),
            ConditionField(TrophyField, Rarity.Epic, new WinnerTakesAbilityCondition()),
            ConditionField(NarrowField, Rarity.Legendary, new NarrowMarginDrawCondition(20)),
            new FieldCard(UnbreakableField, Rarity.Legendary, Modifiers((Element.Fire, 80)),
                new[] { new FieldStaysCondition() }),
            new FieldCard(SacrificeField, Rarity.Legendary, Modifiers((Element.Fire, 100)),
                new[] { new WinnerOutOfGameCondition() }),
            new FieldCard(MercyField, Rarity.Common, Modifiers((Element.Light, 40)),
                new[] { new LoserReturnsToHandCondition() }),
            new FieldCard(HostHomeField, Rarity.Common, Modifiers((Element.Fire, 40), (Element.Water, -30)),
                new[] { new PenaltiesOnlyForGuestCondition() }),

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
            BattleAbility(Desperation, Rarity.Common,
                new PowerBonusEffect(60, new WeakerThanOpponentRequirement())),
            BattleAbility(Mirror, Rarity.Epic, new InvertFieldValuesEffect()),
            BattleAbility(CancelCondition, Rarity.Epic, new CancelFieldConditionEffect()),
            BattleAbility(FieldBlessing, Rarity.Common, new PowerBonusEffect(50, new FieldGivesBonusRequirement())),
            BattleAbility(ForcedReveal, Rarity.Epic, new PowerBonusEffect(60), new OpenBattleFieldEffect()),
            BattleAbility(Intercept, Rarity.Epic, new PowerBonusEffect(40), new CancelLastOpponentAbilityEffect()),
            BattleAbility(Survival, Rarity.Epic, new PowerBonusEffect(40), new ReturnToHandIfLostEffect()),
            BattleAbility(LastWord, Rarity.Epic, new PowerBonusEffect(40), new SilenceOpponentEffect()),
            BattleAbility(TieBreaker, Rarity.Epic, new PowerBonusEffect(50), new WinTiesEffect()),
            BattleAbility(Blockade, Rarity.Epic, new PowerBonusEffect(60), new BlockOpponentBonusesEffect()),

            Legendary(Maneuver, AbilityTiming.BeforeMainAction, new MoveCreatureEffect(ownCreature: true)),
            Legendary(Displacement, AbilityTiming.BeforeMainAction, new MoveCreatureEffect(ownCreature: false)),
            Legendary(Retreat, AbilityTiming.InBattle | AbilityTiming.InBattleOnOpponentTurn, new RetreatEffect()),
            Legendary(SecondLife, AbilityTiming.AfterBattle, new ReturnCreatureFromOutOfGameEffect()),
            Legendary(ReturnPower, AbilityTiming.AfterBattle, new ReturnAbilityFromDiscardEffect()),
            Legendary(Desolation, AbilityTiming.BeforeMainAction, new DiscardRandomOpponentAbilityEffect()),
            Legendary(Scout, AbilityTiming.BeforeMainAction | AbilityTiming.AfterMainAction, new ScoutEffect()),
            Legendary(LockField, AbilityTiming.BeforeMainAction, new LockFieldEffect()),

            Signature(ForgeFlame, AbilityTiming.BeforeBattle | AbilityTiming.InBattle,
                new PowerBonusEffect(80), new OpponentPowerEffect(-20)),
            Signature(StormRush, AbilityTiming.BeforeBattle, new PowerBonusEffect(70), new TakeFirstBattleTurnEffect()),
            Signature(Dawn, AbilityTiming.BeforeBattle | AbilityTiming.InBattle,
                new PowerBonusEffect(70), new ConditionImmunityEffect()),
            Signature(DarkPrice, AbilityTiming.BeforeBattle | AbilityTiming.InBattle,
                new PowerBonusEffect(100), new DiscardOwnAbilityEffect()),
        });

        /// <summary>A valid deck for the default rules; epic/legendary fields and ability slots can be swapped.</summary>
        public static DeckList Deck(
            string commonAbility = Boost50,
            string epicAbility = Boost40,
            string legendaryAbility = Gambit,
            string epicField = Forge,
            string legendaryField = Temple,
            string commonField = Plain) =>
            new DeckList(new[]
            {
                commonField, epicField, legendaryField,
                Fire180, Water200, Earth250,
                commonAbility, epicAbility, legendaryAbility,
            });

        private static AbilityCard BattleAbility(string id, Rarity rarity, params IAbilityEffect[] effects) =>
            new AbilityCard(id, rarity, AbilityTiming.InBattle, effects);

        private static AbilityCard Legendary(string id, AbilityTiming timing, params IAbilityEffect[] effects) =>
            new AbilityCard(id, Rarity.Legendary, timing, effects);

        /// <summary>A signature ability of <see cref="Earth250"/>.</summary>
        private static AbilityCard Signature(string id, AbilityTiming timing, params IAbilityEffect[] effects) =>
            new AbilityCard(id, Rarity.Legendary, timing, effects, boundCreatureId: Earth250);

        /// <summary>Fire +60, Water -30 (like the Forge) plus one condition.</summary>
        private static FieldCard ConditionField(string id, Rarity rarity, FieldCondition condition) =>
            new FieldCard(id, rarity, Modifiers((Element.Fire, 60), (Element.Water, -30)), new[] { condition });

        private static Dictionary<Element, int> Modifiers(params (Element element, int value)[] values)
        {
            var modifiers = new Dictionary<Element, int>();
            foreach (var (element, value) in values)
                modifiers[element] = value;
            return modifiers;
        }
    }
}
