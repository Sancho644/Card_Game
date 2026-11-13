using System.Collections.Generic;
using Game.Data.Presentation;
using UnityEngine;
using UnityEngine.Rendering;

namespace Game.Presentation.Table
{
    /// <summary>Low-poly gold trim fitted to the real table plane after camera framing.</summary>
    [DefaultExecutionOrder(200)]
    public sealed class OvalTableView : MonoBehaviour
    {
        [SerializeField] private Camera _camera;
        [SerializeField] private TableBoardSettings _settings;
        private Mesh _trim;
        private Mesh _gems;
        private Matrix4x4 _applied;
        private bool _ready;
        public Vector2 Radii { get; private set; }
        public float CentreZ { get; private set; }

        private void Awake()
        {
            _trim=Create("GoldOval",_settings.GoldMaterials);
            _gems=Create("TurquoiseInlays",new[]{_settings.Gem});
            Rebuild();
        }

        private Mesh Create(string name,Material[] materials)
        {
            var child=new GameObject(name,typeof(MeshFilter),typeof(MeshRenderer));child.transform.SetParent(transform,false);
            var mesh=new Mesh{name=name};child.GetComponent<MeshFilter>().sharedMesh=mesh;
            var renderer=child.GetComponent<MeshRenderer>();renderer.sharedMaterials=materials;
            renderer.shadowCastingMode=ShadowCastingMode.Off;renderer.receiveShadows=false;
            return mesh;
        }

        private void LateUpdate()
        {
            var matrix=_camera.projectionMatrix*_camera.worldToCameraMatrix;
            if(!_ready||matrix!=_applied)Rebuild();
        }

        private Vector3 At(float x,float y)
        {
            var ray=_camera.ViewportPointToRay(new Vector3(x,y,0));
            var plane=new Plane(Vector3.up,new Vector3(0,_settings.SurfaceY,0));
            return plane.Raycast(ray,out var distance)?ray.GetPoint(distance):Vector3.zero;
        }

        public void Rebuild()
        {
            if(_trim==null||_camera==null)return;
            _applied=_camera.projectionMatrix*_camera.worldToCameraMatrix;_ready=true;
            var near=At(.5f,_settings.NearEdge);var far=At(.5f,_settings.FarEdge);
            CentreZ=(near.z+far.z)*.5f;
            var radiusZ=Mathf.Max(5,(far.z-near.z)*.5f);
            var side=At(_settings.SideInset,.46f);
            var relative=(side.z-CentreZ)/radiusZ;
            var radiusX=Mathf.Max(8,Mathf.Abs(side.x)/Mathf.Sqrt(Mathf.Max(.1f,1-relative*relative)));
            Radii=new Vector2(radiusX,radiusZ);
            var vertices=new List<Vector3>();var triangles=new[]{new List<int>(),new List<int>(),new List<int>()};
            Ring(vertices,triangles,radiusX,radiusZ,_settings.RimWidth,_settings.RimHeight);
            Ring(vertices,triangles,radiusX-_settings.InlayGap,radiusZ-_settings.InlayGap,_settings.RimWidth*.18f,_settings.RimHeight*.35f);
            var half=Mathf.Sqrt(Mathf.Max(0,1-CentreZ*CentreZ/(radiusZ*radiusZ)))*(radiusX-_settings.InlayGap);
            Quad(vertices,triangles[1],new Vector3(-half,_settings.SurfaceY, -_settings.DividerWidth/2),
                new Vector3(-half,_settings.SurfaceY,_settings.DividerWidth/2),
                new Vector3(half,_settings.SurfaceY,_settings.DividerWidth/2),new Vector3(half,_settings.SurfaceY,-_settings.DividerWidth/2));
            _trim.Clear();_trim.SetVertices(vertices);_trim.subMeshCount=3;
            for(var i=0;i<3;i++)_trim.SetTriangles(triangles[i],i);
            _trim.RecalculateNormals();_trim.RecalculateBounds();
            vertices.Clear();var gemTriangles=new List<int>();
            for(var i=0;i<8;i++)
            {
                var a=i*Mathf.PI/4;var centre=new Vector3(radiusX*Mathf.Cos(a),_settings.SurfaceY+_settings.RimHeight+.005f,CentreZ+radiusZ*Mathf.Sin(a));
                Diamond(vertices,gemTriangles,centre,_settings.GemSize);
            }
            Diamond(vertices,gemTriangles,new Vector3(0,_settings.SurfaceY+.008f,0),_settings.GemSize);
            _gems.Clear();_gems.SetVertices(vertices);_gems.SetTriangles(gemTriangles,0);_gems.RecalculateNormals();_gems.RecalculateBounds();
        }

        private void Ring(List<Vector3> vertices,List<int>[] triangles,float rx,float rz,float width,float height)
        {
            var start=vertices.Count;var n=_settings.Segments;
            for(var i=0;i<=n;i++)
            {
                var a=2*Mathf.PI*i/n;var x=Mathf.Cos(a);var z=Mathf.Sin(a);
                var centre=new Vector3(rx*x,_settings.SurfaceY,CentreZ+rz*z);
                var normal=new Vector3(x/rx,0,z/rz).normalized;
                vertices.Add(centre-normal*width*.5f);
                vertices.Add(centre-normal*width*.3f+Vector3.up*height);
                vertices.Add(centre+normal*width*.3f+Vector3.up*height);
                vertices.Add(centre+normal*width*.5f);
            }
            for(var i=0;i<n;i++)for(var j=0;j<3;j++)
            {
                var a=start+i*4+j;var b=a+4;
                triangles[j].AddRange(new[]{a,b,a+1,a+1,b,b+1});
            }
        }

        private static void Quad(List<Vector3> v,List<int> t,Vector3 a,Vector3 b,Vector3 c,Vector3 d)
        {var i=v.Count;v.AddRange(new[]{a,b,c,d});t.AddRange(new[]{i,i+1,i+2,i,i+2,i+3});}

        private static void Diamond(List<Vector3> v,List<int> t,Vector3 p,float size)
        {
            var i=v.Count;v.AddRange(new[]{p+Vector3.up*size*.25f,p+Vector3.left*size,p+Vector3.forward*size,p+Vector3.right*size,p+Vector3.back*size});
            t.AddRange(new[]{i,i+1,i+2,i,i+2,i+3,i,i+3,i+4,i,i+4,i+1});
        }

        private void OnDestroy(){if(_trim!=null)Destroy(_trim);if(_gems!=null)Destroy(_gems);}
    }
}
