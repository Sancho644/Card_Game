using Game.Data.Presentation;
using UnityEngine;
namespace Game.Presentation.Cards
{
    /// <summary>Reusable contour only: shader pulse and travelling light never move the card or its collider.</summary>
    public sealed class CardPlayableGlow : MonoBehaviour
    {
        private static readonly int Tint=Shader.PropertyToID("_Tint");
        private static readonly int Clock=Shader.PropertyToID("_EffectTime");
        private static readonly int Periods=Shader.PropertyToID("_Periods");
        private LineRenderer _line;
        private MaterialPropertyBlock _block;
        public void Configure(CardHighlightSettings settings)
        {
            if(_line==null){
                _line=gameObject.AddComponent<LineRenderer>();_line.useWorldSpace=false;_line.loop=true;
                _line.alignment=LineAlignment.TransformZ;_line.textureMode=LineTextureMode.Stretch;
                _line.numCornerVertices=3;_line.shadowCastingMode=UnityEngine.Rendering.ShadowCastingMode.Off;_line.receiveShadows=false;
                _block=new MaterialPropertyBlock();
            }
            _line.sharedMaterial=settings.Material;_line.widthMultiplier=settings.Width;
            var w=.70f+settings.Padding;var h=1f+settings.Padding;const float c=.065f;const float z=-.08f;
            _line.positionCount=8;_line.SetPositions(new[]{new Vector3(-w+c,-h,z),new Vector3(w-c,-h,z),new Vector3(w,-h+c,z),new Vector3(w,h-c,z),new Vector3(w-c,h,z),new Vector3(-w+c,h,z),new Vector3(-w,h-c,z),new Vector3(-w,-h+c,z)});
            _block.SetVector(Periods,new Vector4(settings.PulseSeconds,settings.TravelSeconds,0,0));
            SetColor(null);
        }
        public void SetColor(Color? color)
        {
            if(_line==null)return;
            if(color.HasValue){_block.SetColor(Tint,color.Value);_block.SetFloat(Clock,Time.unscaledTime);_line.SetPropertyBlock(_block);}
            _line.enabled=color.HasValue;enabled=color.HasValue;
        }
        private void Update(){_block.SetFloat(Clock,Time.unscaledTime);_line.SetPropertyBlock(_block);}
        private void OnDisable(){if(_line!=null)_line.enabled=false;}
        private void OnEnable(){if(_line!=null)_line.enabled=true;}
    }
}
