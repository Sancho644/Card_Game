using UnityEngine;

namespace Game.Presentation.Ui
{
    /// <summary>Fits the rect to the screen's safe area (notches and rounded corners on phones).</summary>
    [RequireComponent(typeof(RectTransform))]
    public sealed class SafeAreaFitter : MonoBehaviour
    {
        private RectTransform _rect;
        private Rect _applied;
        private Vector2Int _screen;

        private void Awake() => _rect = GetComponent<RectTransform>();

        private void Update()
        {
            var safeArea = Screen.safeArea;
            var screen = new Vector2Int(Screen.width, Screen.height);
            if (safeArea == _applied && screen == _screen)
                return;

            _applied = safeArea;
            _screen = screen;
            if (screen.x <= 0 || screen.y <= 0)
                return;

            _rect.anchorMin = new Vector2(safeArea.xMin / screen.x, safeArea.yMin / screen.y);
            _rect.anchorMax = new Vector2(safeArea.xMax / screen.x, safeArea.yMax / screen.y);
            _rect.offsetMin = Vector2.zero;
            _rect.offsetMax = Vector2.zero;
        }
    }
}
