using System.Linq;
using Game.Core.Cards;
using Game.Data.Cards;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace Game.AI.Tests
{
    public class BalanceVariantTests
    {
        [Test]
        public void Variant_ReplacesCardsById_BaseVariantFirst_DatabaseUntouched()
        {
            var fox = Creature("creature.fox", 150);
            var owl = Creature("creature.owl", 200);
            var database = Database(fox, owl);
            var power = Variant("power", null, Creature("creature.fox", 155), Creature("creature.owl", 190));
            var balance = Variant("balance", power, Creature("creature.owl", 210));

            var catalog = balance.ToCatalog(database);

            Assert.AreEqual(155, ((CreatureCard)catalog.Get("creature.fox")).Power, "From the base variant.");
            Assert.AreEqual(210, ((CreatureCard)catalog.Get("creature.owl")).Power, "The variant itself goes on top.");
            Assert.AreEqual(150, ((CreatureCard)database.ToCatalog().Get("creature.fox")).Power);
            CollectionAssert.IsEmpty(balance.Validate(database));
        }

        [Test]
        public void Variant_UnknownId_IsReported()
        {
            var database = Database(Creature("creature.fox", 150));
            var variant = Variant("broken", null, Creature("creature.ghost", 150));

            Assert.AreEqual(1, variant.Validate(database).Count());
        }

        private static CreatureDef Creature(string id, int power)
        {
            var card = ScriptableObject.CreateInstance<CreatureDef>();
            card.name = id;
            var data = new SerializedObject(card);
            data.FindProperty("_id").stringValue = id;
            data.FindProperty("_displayName").stringValue = id;
            data.FindProperty("_power").intValue = power;
            data.ApplyModifiedPropertiesWithoutUndo();
            return card;
        }

        private static CardDatabase Database(params CardDef[] cards) => WithList<CardDatabase>("_cards", cards, _ => { });

        private static BalanceVariantDef Variant(string id, BalanceVariantDef basedOn, params CardDef[] overrides) =>
            WithList<BalanceVariantDef>("_overrides", overrides, data =>
            {
                data.FindProperty("_id").stringValue = id;
                data.FindProperty("_basedOn").objectReferenceValue = basedOn;
            });

        private static T WithList<T>(string listName, CardDef[] cards, System.Action<SerializedObject> setUp) where T : ScriptableObject
        {
            var asset = ScriptableObject.CreateInstance<T>();
            var data = new SerializedObject(asset);
            var list = data.FindProperty(listName);
            list.arraySize = cards.Length;
            for (var i = 0; i < cards.Length; i++)
                list.GetArrayElementAtIndex(i).objectReferenceValue = cards[i];
            setUp(data);
            data.ApplyModifiedPropertiesWithoutUndo();
            return asset;
        }
    }
}
