using System;
using System.Collections.Generic;
using Game.Core.Cards;
using Game.Data.Presentation;
using TMPro;
using UnityEngine;
namespace Game.Presentation.Menu
{
    public sealed class CollectionEditorView
    {
        public readonly RectTransform Root, Grid, Deck;
        public readonly TMP_InputField Name, Search;
        public readonly TMP_Text Count, Page, Status, Focused, Filter;
        public readonly UnityEngine.UI.Button Add, Remove, Play, Previous, Next, AllTypes, Rarity, Details;
        public readonly UnityEngine.UI.Button CollectionTab, DeckTab;
        public readonly Dictionary<CardType,UnityEngine.UI.Button> Types=new Dictionary<CardType,UnityEngine.UI.Button>();
        public readonly Dictionary<Element,UnityEngine.UI.Button> Elements=new Dictionary<Element,UnityEngine.UI.Button>();
        public Action BackClicked, CloseClicked, AddClicked, RemoveClicked, DetailsClicked, PlayClicked, PreviousClicked, NextClicked, RarityClicked, DeleteClicked;
        public Action<string> Renamed, Searched;
        public Action<CardType?> TypeSelected;
        public Action<Element> ElementSelected;
        public Action<bool> SourceSelected;
        public CollectionEditorView(Transform parent,CollectionUi ui,CollectionSettings style,UiTexts texts)
        {
            Root=ui.Rect(parent,"Editor",0,0,1,1);
            ui.Button(Root,"MyDecks",texts.Get("library.myDecks"),.015f,.925f,.14f,.06f,()=>BackClicked?.Invoke());
            Name=ui.Input(Root,"DeckName",.17f,.925f,.34f,.06f,texts.Get("collection.name"),s=>Renamed?.Invoke(s));Name.characterLimit=32;
            ui.Button(Root,"DeletePreset",texts.Get("library.actions"),.53f,.925f,.13f,.06f,()=>DeleteClicked?.Invoke());
            ui.Text(Root,"Saved",texts.Get("library.saved"),.69f,.925f,.14f,.06f,style.SmallSize);
            ui.Button(Root,"Close",texts.Get("collection.back"),.85f,.925f,.135f,.06f,()=>CloseClicked?.Invoke());
            var left=ui.Frame(Root,"CollectionPanel",.015f,.025f,.58f,.88f);
            var right=ui.Frame(Root,"DeckPanel",.605f,.025f,.38f,.88f);
            ui.Text(left,"Title",texts.Get("library.collection"),.025f,.905f,.25f,.07f,style.TitleSize);
            DeckTab=ui.Button(left,"DeckCards",texts.Get("collection.deckCards"),.28f,.91f,.14f,.06f,()=>SourceSelected?.Invoke(false));
            CollectionTab=ui.Button(left,"CollectionCards",texts.Get("collection.browseCollection"),.43f,.91f,.14f,.06f,()=>SourceSelected?.Invoke(true));
            Search=ui.Input(left,"CardSearch",.59f,.91f,.385f,.06f,texts.Get("library.searchCards"),s=>Searched?.Invoke(s),true);
            AllTypes=ui.Button(left,"FilterAll",texts.Get("collection.filter.All"),.025f,.825f,.125f,.055f,()=>TypeSelected?.Invoke(null));
            var index=1;
            foreach(CardType type in Enum.GetValues(typeof(CardType))){var choice=type;Types[type]=ui.Button(left,"Filter"+type,texts.Get("collection.filter."+type),.025f+index++*.13f,.825f,.125f,.055f,()=>TypeSelected?.Invoke(choice));}
            index=0;
            foreach(Element element in Enum.GetValues(typeof(Element))){
                var choice=element;
                var b=ui.Button(left,"Element"+element,"",.56f+index++*.045f,.825f,.042f,.055f,()=>ElementSelected?.Invoke(choice));
                var icon=ui.Rect(b.transform,"Icon",.15f,.15f,.7f,.7f).gameObject.AddComponent<UnityEngine.UI.Image>();
                icon.sprite=style.CardVisuals.IconOf(element);icon.preserveAspect=true;icon.raycastTarget=false;Elements[element]=b;
            }
            Rarity=ui.Button(left,"RarityFilter",texts.Get("library.rarity"),.84f,.825f,.135f,.055f,()=>RarityClicked?.Invoke());
            Filter=ui.Text(left,"Filter","",.025f,.76f,.95f,.045f,style.SmallSize);
            Grid=ui.Rect(left,"CardGrid",.025f,.19f,.95f,.56f);
            Previous=ui.Button(left,"Previous","<",.36f,.125f,.07f,.05f,()=>PreviousClicked?.Invoke());
            Page=ui.Text(left,"Page","",.44f,.125f,.18f,.05f,style.SmallSize);Page.alignment=TextAlignmentOptions.Midline;
            Next=ui.Button(left,"Next",">",.63f,.125f,.07f,.05f,()=>NextClicked?.Invoke());
            var bar=ui.Frame(left,"CardActions",.02f,.015f,.96f,.09f);
            Focused=ui.Text(bar,"Focused","",.025f,.15f,.45f,.7f,style.SmallSize);
            Details=ui.Button(bar,"Details",texts.Get("library.details"),.49f,.15f,.16f,.7f,()=>DetailsClicked?.Invoke());
            Remove=ui.Button(bar,"RemoveFromDeck",texts.Get("library.removeCard"),.66f,.15f,.14f,.7f,()=>RemoveClicked?.Invoke());
            Add=ui.Button(bar,"AddToDeck",texts.Get("collection.replace"),.81f,.15f,.17f,.7f,()=>AddClicked?.Invoke());
            Count=ui.Text(right,"DeckCount","",.05f,.91f,.90f,.06f,style.TitleSize);Count.alignment=TextAlignmentOptions.Midline;
            Deck=ui.Rect(right,"DeckMatrix",.025f,.18f,.95f,.69f);
            Status=ui.Text(right,"Validation","",.05f,.11f,.90f,.055f,style.SmallSize);Status.enableAutoSizing=true;Status.fontSizeMin=12;Status.fontSizeMax=style.SmallSize;
            Play=ui.Button(right,"PlayAi",texts.Get("collection.play"),.08f,.025f,.84f,.075f,()=>PlayClicked?.Invoke());
            Play.GetComponentInChildren<TMP_Text>().fontSizeMax=style.TitleSize;
        }
    }
}
