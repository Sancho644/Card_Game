using DG.Tweening;
using TMPro;
using UnityEngine;

namespace Game.Presentation.Cards
{
    /// <summary>
    /// A 3D card: face, back, highlight, a power badge during battles. Knows nothing about rules, only what
    /// to show and where. The root is placed on the table; everything visible sits under a visual child that
    /// emphasis animations (lift, pulse) move, so they never fight the placement. Renderers may already be
    /// destroyed when the scene unloads, so every access goes through Unity's own null check (the ?. operator
    /// does not see destroyed objects).
    /// </summary>
    public sealed class CardView : MonoBehaviour
    {
        private static readonly int ColorProperty = Shader.PropertyToID("_BaseColor");
        private static readonly int MapProperty = Shader.PropertyToID("_BaseMap");
        private static readonly int MapTransformProperty = Shader.PropertyToID("_BaseMap_ST");

        [SerializeField] private Transform _visual;
        [SerializeField] private Renderer _frame;
        [SerializeField] private Renderer _body;
        [SerializeField] private Renderer _rarityBadge;
        [SerializeField] private Renderer _art;
        [SerializeField] private Renderer _artPlaceholder;
        [SerializeField] private Renderer _artIcon;
        [SerializeField] private Renderer _strip;
        [SerializeField] private Renderer _stripIcon;
        [SerializeField] private GameObject _basePower;
        [SerializeField] private Renderer _basePowerBackground;
        [SerializeField] private TMP_Text _basePowerText;
        [SerializeField] private Renderer _back;
        [SerializeField] private Renderer[] _highlight;
        [SerializeField] private Renderer[] _battleGlow;
        [SerializeField] private TMP_Text _title;
        [SerializeField] private TMP_Text _stats;
        [SerializeField] private TMP_Text _text;
        [SerializeField] private TMP_Text _rarity;
        [SerializeField] private TMP_Text _status;
        [SerializeField] private GameObject _conditionBadge;
        [SerializeField] private GameObject _powerBadge;
        [SerializeField] private Renderer _powerBackground;
        [SerializeField] private TMP_Text _powerText;
        [SerializeField] private Collider _collider;

        private MaterialPropertyBlock _block;
        private Tween _move;
        private Tween _powerFlash;
        private CardFace _face;
        private CardSkin _skin;
        private float _dimming = 1f;

        /// <summary>Match-local card instance id; negative for anonymous backs of hidden cards.</summary>
        public int InstanceId { get; private set; }

        /// <summary>Card id, or null while the card is hidden from the viewer.</summary>
        public string CardId { get; private set; }

        public bool IsKnown => CardId != null;

        public Collider Collider => _collider;

        /// <summary>Everything visible on the card; emphasis animations move this, not the card itself.</summary>
        public Transform Visual => _visual != null ? _visual : transform;

        /// <summary>World position of the power badge, where power changes pop up.</summary>
        public Vector3 PowerAnchor => _powerBadge != null ? _powerBadge.transform.position : transform.position;

        public void Bind(int instanceId, string cardId, CardFace face, CardSkin skin)
        {
            InstanceId = instanceId;
            CardId = cardId;
            name = cardId == null ? $"Card #{instanceId}" : $"Card #{instanceId} {cardId}";

            _face = face;
            _skin = skin;
            _dimming = 1f;
            SetSprite(_back, skin.BackArt, skin.Back);
            SetColor(_powerBackground, skin.PowerBadge);
            ApplyFace();
            if (_conditionBadge != null)
                _conditionBadge.SetActive(face.HasCondition);
            SetStatus(null);
            SetHighlight(null);
            SetBattle(null);
            SetPower(null);
        }

        /// <summary>A short note on the card, e.g. that a field is closed. Null hides it.</summary>
        public void SetStatus(string status)
        {
            if (_status == null)
                return;
            _status.text = status ?? "";
            _status.gameObject.SetActive(!string.IsNullOrEmpty(status));
        }

        /// <summary>Darkens the face, e.g. the owner's own closed field. 1 is the normal face.</summary>
        public void SetDimming(float brightness)
        {
            if (Mathf.Approximately(brightness, _dimming))
                return;
            _dimming = brightness;
            ApplyFace();
        }

        public void SetHighlight(Color? color) => SetGlow(_highlight, color);

        public void SetBattle(Color? color) => SetGlow(_battleGlow, color);

        /// <summary>Shows the creature's current power in a badge; null hides it.</summary>
        public void SetPower(int? power)
        {
            if (_powerBadge == null)
                return;
            _powerBadge.SetActive(power.HasValue);
            if (power.HasValue && _powerText != null)
                _powerText.text = power.Value.ToString();
        }

        /// <summary>Tints the power badge for a moment: green when the power grew, red when it fell.</summary>
        public Tween FlashPower(Color color, float duration)
        {
            if (_powerBadge == null || _powerBackground == null)
                return null;

            _powerFlash?.Kill();
            SetColor(_powerBackground, color);
            var badge = _powerBadge.transform;
            badge.localScale = Vector3.one;
            _powerFlash = DOTween.Sequence()
                .Append(badge.DOPunchScale(Vector3.one * 0.35f, duration * 0.5f, 6, 0.5f))
                .AppendInterval(duration * 0.5f)
                .AppendCallback(() => SetColor(_powerBackground, _skin.PowerBadge))
                .SetTarget(this);
            return _powerFlash;
        }

        public void Place(CardPose pose)
        {
            _move?.Kill();
            transform.SetPositionAndRotation(pose.Position, pose.Rotation);
            transform.localScale = Vector3.one * pose.Scale;
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
                .Join(transform.DORotateQuaternion(pose.Rotation, duration).SetEase(Ease.OutCubic))
                .Join(transform.DOScale(pose.Scale, duration).SetEase(Ease.OutCubic))
                .SetTarget(this);
            return _move;
        }

        private void ApplyFace()
        {
            var text = Dim(_skin.Text);
            SetColor(_frame, Dim(_face.FrameColor));
            SetColor(_body, Dim(_skin.Body));
            SetSprite(_rarityBadge, _face.RarityIcon, Dim(_face.RarityColor));
            SetColor(_strip, Dim(_face.StripColor));
            var ink = Dim(_skin.InkOn(_face.StripColor));
            SetIcon(_stripIcon, _face.ElementIcon, ink);
            SetText(_title, _face.Title, text);
            SetText(_stats, _face.Stats, ink);
            SetText(_text, _face.Body, text);
            SetText(_rarity, _face.Rarity, text);

            var hasArt = _face.Art != null;
            SetActive(_art, hasArt);
            SetActive(_artPlaceholder, !hasArt);
            if (hasArt)
            {
                SetSprite(_art, _face.Art, Dim(Color.white));
                SetIcon(_artIcon, null, Color.white);
            }
            else
            {
                var shade = _face.FrameColor * _skin.PlaceholderBrightness;
                shade.a = 1f;
                SetColor(_artPlaceholder, Dim(shade));
                SetIcon(_artIcon, _face.ElementIcon, Dim(_face.FrameColor));
            }

            if (_basePower != null)
                _basePower.SetActive(_face.Power.HasValue);
            if (_face.Power.HasValue)
            {
                SetColor(_basePowerBackground, Dim(_skin.PowerBadge));
                SetText(_basePowerText, _face.Power.Value.ToString(), text);
            }
        }

        private Color Dim(Color color) => new Color(color.r * _dimming, color.g * _dimming, color.b * _dimming, color.a);

        private void SetGlow(Renderer[] renderers, Color? color)
        {
            if (renderers == null)
                return;
            foreach (var glow in renderers)
            {
                if (glow == null)
                    continue;
                glow.enabled = color.HasValue;
                if (color.HasValue)
                    SetColor(glow, color.Value);
            }
        }

        /// <summary>An icon that hides when there is none.</summary>
        private void SetIcon(Renderer target, Sprite icon, Color tint)
        {
            SetActive(target, icon != null);
            if (icon != null)
                SetSprite(target, icon, tint);
        }

        /// <summary>Shows the sprite (or a plain color when it is null) on a quad, through the material's base map.</summary>
        private void SetSprite(Renderer target, Sprite sprite, Color tint)
        {
            if (target == null)
                return;
            if (_block == null)
                _block = new MaterialPropertyBlock();
            target.GetPropertyBlock(_block);
            _block.SetColor(ColorProperty, tint);
            if (sprite != null)
            {
                var texture = sprite.texture;
                var rect = sprite.textureRect;
                _block.SetTexture(MapProperty, texture);
                _block.SetVector(MapTransformProperty, new Vector4(
                    rect.width / texture.width, rect.height / texture.height, rect.x / texture.width, rect.y / texture.height));
            }
            else
            {
                _block.SetTexture(MapProperty, Texture2D.whiteTexture);
                _block.SetVector(MapTransformProperty, new Vector4(1f, 1f, 0f, 0f));
            }

            target.SetPropertyBlock(_block);
        }

        private void SetColor(Renderer target, Color color)
        {
            if (target == null)
                return;
            if (_block == null)
                _block = new MaterialPropertyBlock();
            target.GetPropertyBlock(_block);
            _block.SetColor(ColorProperty, color);
            target.SetPropertyBlock(_block);
        }

        private static void SetActive(Renderer target, bool active)
        {
            if (target != null)
                target.gameObject.SetActive(active);
        }

        private static void SetText(TMP_Text target, string value, Color color)
        {
            if (target == null)
                return;
            target.text = value ?? "";
            target.color = color;
        }

        private void OnDestroy()
        {
            _move?.Kill();
            _powerFlash?.Kill();
        }
    }
}
