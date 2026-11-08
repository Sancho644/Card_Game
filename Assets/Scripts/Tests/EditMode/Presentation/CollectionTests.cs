using System;
using System.Collections.Generic;
using System.Linq;
using Game.Core.Cards;
using Game.Data.Cards;
using Game.Data.Config;
using Game.Data.Decks;
using Game.Services.Collection;
using NUnit.Framework;
using UnityEditor;

namespace Game.Presentation.Tests
{
    public sealed class CollectionTests
    {
        private CardDatabase _database;
        private GameRulesConfig _rules;
        private List<DeckPreset> _defaults;
        private MemoryCollectionStore _store;
        [SetUp]
        public void SetUp()
        {
            _database = AssetDatabase.LoadAssetAtPath<CardDatabase>("Assets/Data/CardDatabase.asset");
            _rules = AssetDatabase.LoadAssetAtPath<GameRulesConfig>("Assets/Data/Config/GameRules.asset");
            _defaults = AssetDatabase.FindAssets("t:DeckDef").Select(AssetDatabase.GUIDToAssetPath)
                .Select(AssetDatabase.LoadAssetAtPath<DeckDef>).Select(d => new DeckPreset(d.DisplayName, d.ToDeckList().CardIds)).ToList();
            _store = new MemoryCollectionStore();
        }
        private PlayerCollection Load() => new PlayerCollection(_store, _database.ToCatalog(), _rules.ToRules(), _defaults);

        [Test]
        public void FirstLaunch_ThreeValidPresets_AndWholeCollection()
        {
            var c = Load();
            for (var i = 0; i < 3; i++) { Assert.IsTrue(c.Validate(i).IsValid); Assert.AreEqual(9, c.Get(i).Cards.Count); }
            Assert.AreEqual(_database.Cards.Count, c.OwnedCards.Count);
            Assert.Throws<ArgumentOutOfRangeException>(() => c.Get(3));
        }
        [Test]
        public void RenameReplaceSelect_RestoredAfterLoading_WithoutChangingOtherPresets()
        {
            var c = Load(); var other = c.Get(0).Cards.ToArray();
            var card = _database.Cards.First(d => d.Type == CardType.Field && !c.Get(2).Cards.Contains(d.Id));
            c.Rename(2, "  Моя колода  "); c.Replace(2, card.Id); Assert.IsTrue(c.Select(2));
            var loaded = Load(); Assert.AreEqual(2, loaded.SelectedIndex); Assert.AreEqual("Моя колода", loaded.Selected.Name);
            Assert.Contains(card.Id, loaded.Selected.Cards.ToArray()); Assert.AreEqual(9, loaded.Selected.Cards.Count);
            CollectionAssert.AreEqual(other, loaded.Get(0).Cards); Assert.IsTrue(loaded.Validate(2).IsValid);
        }
        [Test]
        public void SignatureWithoutCreature_IsSavedAsDraft_ButCannotBeSelected()
        {
            var c = Load(); var catalog = _database.ToCatalog();
            var ability = catalog.All.OfType<AbilityCard>().First(a => a.IsSignature);
            c.Replace(1, ability.Id);
            var replacement = catalog.All.First(a => a.Type == CardType.Creature && a.Rarity == Rarity.Legendary && a.Id != ability.BoundCreatureId);
            c.Replace(1, replacement.Id);
            Assert.IsFalse(c.Select(1)); Assert.AreEqual(0, c.SelectedIndex);
            Assert.IsFalse(Load().Validate(1).IsValid);
            c.Replace(1, ability.BoundCreatureId);
            Assert.IsTrue(c.Select(1)); Assert.IsTrue(Load().Validate(1).IsValid);
        }
        [Test]
        public void DamagedStorage_UnknownIdsAndSelection_RecoverToUsableDefaults()
        {
            _store.Write("collection.v1.deck.0.cards", "missing.card"); _store.Write("collection.v1.selected", "999");
            var c = Load(); Assert.AreEqual(0, c.SelectedIndex); Assert.IsTrue(c.Validate(0).IsValid);
        }
        [Test]
        public void RemoveCard_PersistsEmptySlot_WithoutRemovingOwnership()
        {
            var c = Load(); var id = c.Get(0).Cards[0];
            c.Remove(0, id); var loaded = Load();
            Assert.AreEqual(8, loaded.Get(0).Cards.Count);
            Assert.IsFalse(loaded.Get(0).Cards.Contains(id));
            Assert.Contains(id, loaded.OwnedCards.ToArray());
            Assert.IsFalse(loaded.Select(0));
            loaded.Replace(0, id); Assert.IsTrue(loaded.Select(0));
        }
        [Test]
        public void DeletePreset_PreservesPersistentEmptyCell_WhichCanBeRebuilt()
        {
            var c = Load(); var cards = c.Get(1).Cards.ToArray();
            c.Delete(1); var loaded = Load();
            Assert.IsEmpty(loaded.Get(1).Cards); Assert.IsEmpty(loaded.Get(1).Name);
            Assert.IsFalse(loaded.Select(1)); Assert.AreEqual(9, loaded.Get(2).Cards.Count);
            loaded.Rename(1, "Новая"); foreach (var id in cards) loaded.Replace(1,id);
            Assert.IsTrue(loaded.Select(1)); Assert.AreEqual("Новая", Load().Get(1).Name);
        }

        [Test]
        public void Names_TrimmedLimited_EmptyDoesNotOverwrite()
        {
            var c = Load(); c.Rename(0, new string('А', 80)); Assert.AreEqual(32, Load().Get(0).Name.Length);
            Assert.Throws<ArgumentException>(() => c.Rename(0, " \n\t")); Assert.AreEqual(32, Load().Get(0).Name.Length);
        }
    }
}
