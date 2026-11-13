using UnityEngine;

namespace Game.Data.Presentation
{
    [CreateAssetMenu(menuName="Game/Config/Table Board",fileName="TableBoard")]
    public sealed class TableBoardSettings : ScriptableObject
    {
        [SerializeField] private Material _goldShadow;
        [SerializeField] private Material _goldFace;
        [SerializeField] private Material _goldHighlight;
        [SerializeField] private Material _gem;
        [Header("Screen framing")]
        [SerializeField] private float _farEdge=.94f;
        [SerializeField] private float _nearEdge=-.16f;
        [SerializeField] private float _sideInset=.005f;
        [Header("Geometry")]
        [SerializeField,Range(48,256)] private int _segments=160;
        [SerializeField,Min(.02f)] private float _rimWidth=.18f;
        [SerializeField,Min(.005f)] private float _rimHeight=.045f;
        [SerializeField,Min(.05f)] private float _inlayGap=.24f;
        [SerializeField,Min(.005f)] private float _dividerWidth=.018f;
        [SerializeField,Min(.02f)] private float _gemSize=.14f;
        [SerializeField] private float _surfaceY=-.025f;
        public Material[] GoldMaterials=>new[]{_goldShadow,_goldFace,_goldHighlight};
        public Material Gem=>_gem;
        public float FarEdge=>_farEdge;
        public float NearEdge=>_nearEdge;
        public float SideInset=>_sideInset;
        public int Segments=>_segments;
        public float RimWidth=>_rimWidth;
        public float RimHeight=>_rimHeight;
        public float InlayGap=>_inlayGap;
        public float DividerWidth=>_dividerWidth;
        public float GemSize=>_gemSize;
        public float SurfaceY=>_surfaceY;
    }
}
