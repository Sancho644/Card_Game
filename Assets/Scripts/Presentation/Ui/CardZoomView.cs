using Game.Presentation.Cards;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Game.Presentation.Ui
{
    /// <summary>A card shown large over the table; any tap closes it.</summary>
    public sealed class CardZoomView : MonoBehaviour
    {
        [SerializeField] private Image _frame;
        [SerializeField] private Image _rarityBadge;
        [SerializeField] private TMP_Text _title;
        [SerializeField] private TMP_Text _stats;
        [SerializeField] private TMP_Text _body;
        [SerializeField] private TMP_Text _rarity;
        [SerializeField] private Button _close;

        public bool IsOpen => gameObject.activeSelf;

        public void Show(CardFace face)
        {
            _frame.color = face.FrameColor;
            _rarityBadge.color = face.RarityColor;
            _title.text = face.Title;
            _stats.text = face.Stats;
            _body.text = face.Body;
            _rarity.text = face.Rarity;
            UiButtons.Bind(_close, Hide);
            gameObject.SetActive(true);
        }

        public void Hide() => gameObject.SetActive(false);
    }
}
