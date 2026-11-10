using System;
using Game.Data.Presentation;
using TMPro;
using UnityEngine;
namespace Game.Presentation.Menu
{
    public sealed class CollectionLibraryView
    {
        public readonly RectTransform Root,Content,Deck;
        public readonly TMP_InputField Search,Name;
        public readonly TMP_Text Count,Status;
        public readonly UnityEngine.UI.Button Edit,Play,Clear,Use;
        public readonly UnityEngine.UI.ScrollRect Scroll;
        public Action CloseClicked,NewClicked,EditClicked,PlayClicked,ClearClicked,UseClicked;
        public Action<string> Searched,Renamed;
        public CollectionLibraryView(Transform parent,CollectionUi ui,CollectionSettings style,UiTexts texts)
        {
            Root=ui.Rect(parent,"DeckLibrary",0,0,1,1);
            ui.Text(Root,"Title",texts.Get("library.myDecks"),.025f,.925f,.44f,.06f,style.TitleSize);
            ui.Button(Root,"Close",texts.Get("collection.back"),.85f,.925f,.135f,.06f,()=>CloseClicked?.Invoke());
            ui.Button(Root,"NewDeck",texts.Get("library.newDeck"),.025f,.835f,.19f,.065f,()=>NewClicked?.Invoke());
            Search=ui.Input(Root,"DeckSearch",.23f,.835f,.30f,.065f,texts.Get("library.searchDecks"),s=>Searched?.Invoke(s),true);
            Count=ui.Text(Root,"DeckCount","",.545f,.835f,.10f,.065f,style.SmallSize);
            var panel=ui.Frame(Root,"DeckListPanel",.015f,.025f,.63f,.79f);
            Content=ui.Scroll(panel,"DeckScroll",.02f,.025f,.96f,.95f,out Scroll);
            var right=ui.Frame(Root,"PreviewPanel",.655f,.025f,.33f,.88f);
            Name=ui.Input(right,"LibraryDeckName",.04f,.915f,.76f,.065f,texts.Get("collection.name"),s=>Renamed?.Invoke(s));Name.characterLimit=32;
            Clear=ui.Button(right,"ClearDeck",texts.Get("library.actions"),.82f,.915f,.14f,.065f,()=>ClearClicked?.Invoke());
            Status=ui.Text(right,"Validation","",.05f,.84f,.9f,.06f,style.SmallSize);Status.enableAutoSizing=true;Status.fontSizeMin=12;Status.fontSizeMax=style.SmallSize;
            Deck=ui.Rect(right,"DeckMatrix",.025f,.20f,.95f,.63f);
            Edit=ui.Button(right,"EditDeck",texts.Get("library.edit"),.05f,.115f,.42f,.065f,()=>EditClicked?.Invoke());
            Play=ui.Button(right,"PlayAi",texts.Get("collection.play"),.50f,.115f,.45f,.065f,()=>PlayClicked?.Invoke());
            Use=ui.Button(right,"UseDeck",texts.Get("collection.use"),.20f,.025f,.60f,.065f,()=>UseClicked?.Invoke());
        }
    }
}
