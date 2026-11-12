using UnityEngine;

namespace Game.Data.Presentation
{
    [CreateAssetMenu(menuName = "Game/Config/Battle Windows", fileName = "BattleWindows")]
    public sealed class BattleWindowSettings : ScriptableObject
    {
        [SerializeField] private CollectionSettings _style;
        [SerializeField] private Texture2D _emblems;
        [SerializeField] private Color _scrim = new Color(.005f, .018f, .02f, .82f);
        [SerializeField] private Color _opaque = new Color(.012f, .045f, .044f, 1);
        [SerializeField] private Color _ink = new Color(1, .94f, .77f, 1);
        [SerializeField] private Color _gold = new Color(1, .76f, .36f, 1);
        [SerializeField] private Color _teal = new Color(.37f, .92f, .84f, 1);
        [SerializeField, Min(18)] private float _titleSize = 38;
        [SerializeField, Min(16)] private float _bodySize = 25;
        public CollectionSettings Style => _style;
        public Texture2D Emblems => _emblems;
        public Color Scrim => _scrim;
        public Color Opaque => _opaque;
        public Color Ink => _ink;
        public Color Gold => _gold;
        public Color Teal => _teal;
        public float TitleSize => _titleSize;
        public float BodySize => _bodySize;
    }
}
