using System;
using System.Collections.Generic;
using System.Linq;
using Game.Core.Cards;
using Game.Data.Cards;
using Game.Data.Presentation;
using Game.Services.Collection;
using UnityEngine;
namespace Game.Presentation.Menu
{
    public sealed class CollectionDeckGrid
    {
        private readonly CollectionUi _ui;
        private readonly CollectionCardView _cards;
        private readonly CardDatabase _database;
        private readonly UiTexts _texts;
        private readonly RectTransform[] _holders=new RectTransform[9];
        private readonly List<GameObject> _rendered=new List<GameObject>();
        public CollectionDeckGrid(Transform parent,CollectionUi ui,CollectionCardView cards,CardDatabase database,UiTexts texts)
        {
            _ui=ui;_cards=cards;_database=database;_texts=texts;
            for(var col=0;col<3;col++){
                var label=ui.Text(parent,"Rarity"+col,texts.Get(UiTextKeys.RarityPrefix+(Rarity)col),.17f+col*.27f,.925f,.255f,.065f,18);
                label.alignment=TMPro.TextAlignmentOptions.Midline;label.enableAutoSizing=true;label.fontSizeMin=12;label.fontSizeMax=18;
            }
            for(var row=0;row<3;row++){
                var y=.615f-row*.305f;
                var label=ui.Text(parent,"Type"+row,texts.Get("collection.filter."+(CardType)row),.005f,y+.06f,.15f,.16f,19);
                label.alignment=TMPro.TextAlignmentOptions.Midline;label.enableAutoSizing=true;label.fontSizeMin=12;label.fontSizeMax=19;
                for(var col=0;col<3;col++)_holders[row*3+col]=ui.Rect(parent,"Slot"+(row*3+col),.165f+col*.27f,y,.26f,.30f);
            }
        }
        public void Refresh(DeckPreset deck,CardDef focused,Action<CardType,Rarity,CardDef> selected)
        {
            Clear(_rendered);
            for(var row=0;row<3;row++)for(var col=0;col<3;col++){
                var type=(CardType)row;var rarity=(Rarity)col;var holder=_holders[row*3+col];
                var card=deck.Cards.Select(_database.Find).FirstOrDefault(c=>c.Type==type&&c.Rarity==rarity);
                if(card==null){
                    var empty=_ui.Button(holder,"EmptySlot"+(row*3+col),_texts.Get("library.emptySlot"),.08f,.06f,.84f,.88f,()=>selected(type,rarity,null));
                    _rendered.Add(empty.gameObject);
                }else{
                    var active=focused!=null&&focused.Type==type&&focused.Rarity==rarity;
                    var button=_cards.Create(holder,card,0,false,active,"",()=>selected(type,rarity,card),1,1);
                    _rendered.Add(button.transform.parent.parent.parent.gameObject);
                }
            }
        }
        public static void Clear(List<GameObject> objects){foreach(var go in objects)if(go!=null){go.SetActive(false);UnityEngine.Object.Destroy(go);}objects.Clear();}
    }
}
