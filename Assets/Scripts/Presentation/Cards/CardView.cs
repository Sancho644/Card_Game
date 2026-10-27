using DG.Tweening;
using TMPro;
using UnityEngine;

namespace Game.Presentation.Cards
{
    /// <summary>A 3D card: face, back, highlight. Knows nothing about rules, only what to show and where.</summary>
    public sealed class CardView : MonoBehaviour
    {
        private static readonly int ColorProperty = Shader.PropertyToID("_BaseColor");

        [SerializeField] private Renderer _frame;
        [SerializeField] private Renderer _body;
        [SerializeField] private Renderer _rarityBadge;
        [SerializeField] private Renderer _back;
        [SerializeField] private Renderer[] _highlight;
        [SerializeField] private Renderer[] _battleGlow;
        [SerializeField] private TMP_Text _title;
        [SerializeField] private TMP_Text _stats;
        [SerializeField] private TMP_Text _text;
        [SerializeField] private TMP_Text _rarity;
        [SerializeField] private TMP_Text _status;
        [SerializeField] private Collider _collider;

        private MaterialPropertyBlock _block;
        private Tween _move;

        /// <summary>Match-local card instance id; negative for anonymous backs of hidden cards.</summary>
        public int InstanceId { get; private set; }

        /// <summary>Card id, or null while the card is hidden from the viewer.</summary>
        public string CardId { get; private set; }

        public bool IsKnown => CardId != null;

        public Collider Collider => _collider;

        public void Bind(int instanceId, string cardId, CardFace face, Color body, Color back, Color text)
        {
            InstanceId = instanceId;
            CardId = cardId;
            name = cardId == null ? $"Card #{instanceId}" : $"Card #{instanceId} {cardId}";

            SetColor(_frame, face.FrameColor);
            SetColor(_body, body);
            SetColor(_rarityBadge, face.RarityColor);
            SetColor(_back, back);
            SetText(_title, face.Title, text);
            SetText(_stats, face.Stats, text);
            SetText(_text, face.Body, text);
            SetText(_rarity, face.Rarity, text);
            SetStatus(null);
            SetHighlight(null);
            SetBattle(null);
        }

        /// <summary>A short note on the card, e.g. that a field is closed. Null hides it.</summary>
        public void SetStatus(string status)
        {
            _status.text = status ?? "";
            _status.gameObject.SetActive(!string.IsNullOrEmpty(status));
        }

        public void SetHighlight(Color? color) => SetGlow(_highlight, color);

        public void SetBattle(Color? color) => SetGlow(_battleGlow, color);

        public void Place(CardPose pose)
        {
            _move?.Kill();
            transform.SetPositionAndRotation(pose.Position, pose.Rotation);
        }

        public Tween MoveTo(CardPose pose, float duration)
        {
            _move?.Kill();
            if (duration <= 0f)
            {
                Place(pose);
                return null;
            }

            _move = DOTween.Sequence()
                .Join(transform.DOMove(pose.Position, duration).SetEase(Ease.OutCubic))
                .Join(transform.DORotateQuaternion(pose.Rotation, duration).SetEase(Ease.OutCubic));
            return _move;
        }

        private void SetGlow(Renderer[] renderers, Color? color)
        {
            foreach (var glow in renderers)
            {
                glow.enabled = color.HasValue;
                if (color.HasValue)
                    SetColor(glow, color.Value);
            }
        }

        private void SetColor(Renderer target, Color color)
        {
            if (_block == null)
                _block = new MaterialPropertyBlock();
            target.GetPropertyBlock(_block);
            _block.SetColor(ColorProperty, color);
            target.SetPropertyBlock(_block);
        }

        private static void SetText(TMP_Text target, string value, Color color)
        {
            target.text = value ?? "";
            target.color = color;
        }

        private void OnDestroy() => _move?.Kill();
    }
}
