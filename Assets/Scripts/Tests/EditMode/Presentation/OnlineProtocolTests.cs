using System;
using System.Linq;
using Game.Core;
using Game.Core.Commands;
using Game.Core.Events;
using Game.Core.State;
using Game.Data.Cards;
using Game.Data.Config;
using Game.Data.Decks;
using Game.Presentation.Network;
using Game.Services.Match;
using NUnit.Framework;
using UnityEditor;
namespace Game.Presentation.Tests
{
    public sealed class OnlineProtocolTests
    {
        private LocalMatchSession Engine()
        {
            var db=AssetDatabase.LoadAssetAtPath<CardDatabase>("Assets/Data/CardDatabase.asset");
            var path=AssetDatabase.FindAssets("t:GameRulesConfig").Select(AssetDatabase.GUIDToAssetPath).First();
            return new LocalMatchSession(db.ToCatalog(),AssetDatabase.LoadAssetAtPath<GameRulesConfig>(path).ToRules());
        }
        private MatchSetup Setup()
        {
            var decks=AssetDatabase.FindAssets("t:DeckDef").Select(AssetDatabase.GUIDToAssetPath).Select(AssetDatabase.LoadAssetAtPath<DeckDef>).ToArray();
            return new MatchSetup(decks[0].ToDeckList(),decks[1].ToDeckList());
        }
        [Test]
        public void WholeMatch_RecipientFramesRoundTripWithPrivateEventsFiltered()
        {
            var engine=Engine();var authority=new OnlineAuthority(engine);var requests=new long[2];
            var frames=new OnlineFrame[2];var batches=0;
            authority.Updated+=(player,frame)=>{
                var copy=OnlineCodec.Decode<OnlineFrame>(OnlineCodec.Encode(frame));frames[(int)player]=copy;batches++;
                Assert.AreEqual(OnlineCodec.Encode(frame.View),OnlineCodec.Encode(copy.View));
                Assert.AreEqual(frame.View.Wins(player),copy.View.Wins(player));
                foreach(var field in copy.View.Fields.Where(f=>f.Owner!=player&&!f.IsOpen))Assert.IsNull(field.CardId);
                foreach(var e in copy.Events.Select(OnlineCodec.Unpack<GameEvent>))Assert.IsTrue(e.IsVisibleTo(player));
                foreach(var c in copy.Actions.Select(OnlineCodec.Unpack<GameCommand>))Assert.AreEqual(player,c.Player);
                if(copy.View.Battle!=null)foreach(var p in PlayerIds.All)Assert.AreEqual(frame.View.Battle.IsReady(p),copy.View.Battle.IsReady(p));
                Assert.AreEqual(OnlineCodec.Encode(frame.Power),OnlineCodec.Encode(copy.Power));
            };
            authority.Start(Setup(),123);
            foreach(var p in PlayerIds.All){var hand=frames[(int)p].Events.Select(OnlineCodec.Unpack<GameEvent>).OfType<HandDealt>().ToArray();Assert.AreEqual(1,hand.Length);Assert.AreEqual(p,hand[0].Owner);}
            for(int turn=0;turn<1000&&engine.GetView(PlayerId.Player1).Phase!=MatchPhase.Ended;turn++){
                var player=PlayerIds.All.First(p=>engine.GetLegalActions(p).Count>0);
                var legal=engine.GetLegalActions(player).ToList();
                var index=legal.FindIndex(c=>c is Ready);if(index<0)index=0;
                authority.Submit(player,++requests[(int)player],authority.Version,index);
            }
            Assert.AreEqual(MatchPhase.Ended,engine.GetView(PlayerId.Player1).Phase);Assert.Greater(batches,20);
        }
        [Test]
        public void RepeatedAndStaleRequests_DoNotExecuteAnotherAction()
        {
            var authority=new OnlineAuthority(Engine());authority.Start(Setup(),12);
            OnlineFrame last=null;authority.Updated+=(p,f)=>last=f;
            authority.Submit(PlayerId.Player1,1,authority.Version,0);var after=authority.Version;
            authority.Submit(PlayerId.Player1,1,after,0);Assert.AreEqual(after,authority.Version);
            authority.Submit(PlayerId.Player1,2,after-1,0);Assert.AreEqual(after,authority.Version);Assert.AreEqual(RejectReason.WrongPhase,last.Rejection);
            authority.Submit(PlayerId.Player2,1,after,-1);Assert.AreEqual(after,authority.Version);
        }
        [Test]
        public void Client_PendingCommandBlocksDuplicates_ThenAcknowledgementReenablesActions()
        {
            var authority=new OnlineAuthority(Engine());var packets=0;long request=0;var version=0;var action=0;
            var client=new OnlineMatchSession(PlayerId.Player1,(r,v,a)=>{packets++;request=r;version=v;action=a;});
            authority.Updated+=(p,f)=>{if(p==PlayerId.Player1)client.Receive(OnlineCodec.Decode<OnlineFrame>(OnlineCodec.Encode(f)));};
            authority.Start(Setup(),3);client.Start(null,0);
            var chosen=client.GetLegalActions(PlayerId.Player1).First();var completed=false;
            client.SubmitAsync(chosen,r=>completed=r.IsAccepted);client.SubmitAsync(chosen,_=>Assert.Fail("Duplicate completed"));
            Assert.AreEqual(1,packets);Assert.IsEmpty(client.GetLegalActions(PlayerId.Player1));
            authority.Submit(PlayerId.Player1,request,version,action);Assert.IsTrue(completed);
            Assert.Throws<InvalidOperationException>(()=>client.GetView(PlayerId.Player2));
        }
        [Test]
        public void Codec_RejectsUnlistedTypesAndOversizedPackets()
        {
            Assert.Throws<ArgumentException>(()=>OnlineCodec.Unpack<GameCommand>(new OnlineValue{Kind="System.IO.File"}));
            Assert.Throws<ArgumentException>(()=>OnlineCodec.Decode<OnlinePacket>(new string('x',OnlineCodec.MaxBytes+1)));
        }
    }
}
