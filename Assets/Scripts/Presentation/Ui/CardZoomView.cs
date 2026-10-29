using Game.Presentation.Cards;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Game.Presentation.Ui
{
    /// <summary>A card shown large over the table, laid out like the card itself; any tap closes it.</summary>
    public sealed class CardZoomView : MonoBehaviour
    {
        [SerializeField] private Image _frame;
        [SerializeField] private Image _rarityBadge;
        [SerializeField] private Image _art;
        [SerializeField] private Image _artPlaceholder;
        [SerializeField] private Image _artIcon;
        [SerializeField] private Image _strip;
        [SerializeField] private Image _stripIcon;
        [SerializeField] private GameObject _basePower;
        [SerializeField] private TMP_Text _basePowerText;
        [SerializeField] private TMP_Text _title;
        [SerializeField] private TMP_Text _stats;
        [SerializeField] private TMP_Text _body;
        [SerializeField] private TMP_Text _rarity;
        [SerializeField] private Button _close;

        private CardSkin _skin;

        public bool IsOpen => gameObject.activeSelf;

        public void Init(CardSkin skin) => _skin = skin;

        public void Show(CardFace face)
        {
            _frame.color = face.FrameColor;
            SetImage(_rarityBadge, face.RarityIcon, face.RarityColor, keepWithoutSprite: true);
            _strip.color = face.StripColor;
            var ink = _skin.InkOn(face.StripColor);
            SetImage(_stripIcon, face.ElementIcon, ink, keepWithoutSprite: false);

            var hasArt = face.Art != null;
            _art.gameObject.SetActive(hasArt);
            _artPlaceholder.gameObject.SetActive(!hasArt);
            if (hasArt)
            {
                SetImage(_art, face.Art, Color.white, keepWithoutSprite: true);
                _artIcon.gameObject.SetActive(false);
            }
            else
            {
                var shade = face.FrameColor * _skin.PlaceholderBrightness;
                shade.a = 1f;
                _artPlaceholder.color = shade;
                SetImage(_artIcon, face.ElementIcon, face.FrameColor, keepWithoutSprite: false);
            }

            _basePower.SetActive(face.Power.HasValue);
            if (face.Power.HasValue)
                _basePowerText.text = face.Power.Value.ToString();

            _title.text = face.Title;
            _stats.text = face.Stats;
            _stats.color = ink;
            _body.text = face.Body;
            _rarity.text = face.Rarity;
            UiButtons.Bind(_close, Hide);
            gameObject.SetActive(true);
        }

        public void Hide() => gameObject.SetActive(false);

        private static void SetImage(Image image, Sprite sprite, Color color, bool keepWithoutSprite)
        {
            image.gameObject.SetActive(sprite != null || keepWithoutSprite);
            image.sprite = sprite;
            image.color = color;
        }
    }
}
