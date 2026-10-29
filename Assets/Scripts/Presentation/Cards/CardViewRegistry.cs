using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Game.Presentation.Cards
{
    /// <summary>
    /// Card views by match-local instance id, plus anonymous backs for cards the viewer may not identify
    /// (the opponent's hand). A hidden card never gets a view with its real id.
    /// </summary>
    public sealed class CardViewRegistry
    {
        private readonly CardView _prefab;
        private readonly Transform _root;
        private readonly CardFaceBuilder _faces;
        private readonly CardSkin _skin;
        private readonly Dictionary<int, CardView> _known = new Dictionary<int, CardView>();
        private readonly List<CardView> _anonymous = new List<CardView>();

        public CardViewRegistry(CardView prefab, Transform root, CardFaceBuilder faces, CardSkin skin)
        {
            _prefab = prefab;
            _root = root;
            _faces = faces;
            _skin = skin;
        }

        public IEnumerable<CardView> All => _known.Values.Concat(_anonymous);

        public IReadOnlyList<CardView> Anonymous => _anonymous;

        public CardView Find(int instanceId) => _known.TryGetValue(instanceId, out var view) ? view : null;

        /// <summary>The view of a card, created at <paramref name="spawn"/> if new, rebound if its identity became known.</summary>
        public CardView GetOrCreate(int instanceId, string cardId, CardPose spawn)
        {
            if (!_known.TryGetValue(instanceId, out var view))
            {
                view = Spawn(spawn);
                _known.Add(instanceId, view);
                Bind(view, instanceId, cardId);
            }
            else if (view.CardId != cardId)
            {
                Bind(view, instanceId, cardId);
            }

            return view;
        }

        /// <summary>Keeps exactly <paramref name="count"/> anonymous backs, adding new ones at <paramref name="spawn"/>.</summary>
        public void SetAnonymousCount(int count, CardPose spawn)
        {
            while (_anonymous.Count < count)
            {
                var view = Spawn(spawn);
                Bind(view, -(_anonymous.Count + 1), null);
                _anonymous.Add(view);
            }

            while (_anonymous.Count > count)
            {
                var last = _anonymous[_anonymous.Count - 1];
                _anonymous.RemoveAt(_anonymous.Count - 1);
                Object.Destroy(last.gameObject);
            }
        }

        /// <summary>Destroys the views of cards that are no longer visible to the viewer.</summary>
        public void RemoveExcept(ICollection<int> visible)
        {
            foreach (var id in _known.Keys.Where(id => !visible.Contains(id)).ToList())
            {
                Object.Destroy(_known[id].gameObject);
                _known.Remove(id);
            }
        }

        public void Clear()
        {
            RemoveExcept(new int[0]);
            SetAnonymousCount(0, default);
        }

        private CardView Spawn(CardPose pose) => Object.Instantiate(_prefab, pose.Position, pose.Rotation, _root);

        private void Bind(CardView view, int instanceId, string cardId) =>
            view.Bind(instanceId, cardId, _faces.Build(cardId), _skin);
    }
}
