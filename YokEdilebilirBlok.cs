using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Vector2f = UnityEngine.Vector2;
using Vector2i = ClipperLib.IntPoint;
using OlcayKalyoncuoglu;
public class YokEdilebilirBlok : MonoBehaviour
{
    List<List<Vector2i>> _polygons;

    List<EdgeCollider2D> _Colliders = new();

    List<List<Vector2i>> _Polygons { get { return _polygons; } }

    Mesh _Mesh;

    MeshRenderer _MeshRenderer;

    private void Awake()
    {

        _Mesh = new Mesh();
        _Mesh.MarkDynamic();

        MeshFilter _MeshFilter = gameObject.AddComponent<MeshFilter>();
        _MeshFilter.mesh = _Mesh;

        _MeshRenderer = gameObject.AddComponent<MeshRenderer>();

    }
    public void MateryalTanimla(Material material) // SetMaterial
    {
        _MeshRenderer.material = material;

    }

    public void GeometriyiGuncelle(List<List<Vector2i>> inPolygons, float width,float height, float depth) // UpdateGeometry
    {
        if (_polygons!=null)
            _polygons.Clear();  
        else
            _polygons = new List<List<Vector2i>>();

        List<List<Vector2>> EdgesList = new();

        int TotalVertexCount = 0;
        int EdgeTriangleIndexCount = 0;

        // Burada toplam köþe sayýsýný belirliyoruz.
        for (int i = 0; i < inPolygons.Count; i++)
        {
            Vector2i[] TemizlenmisPoligon = BlokYonetimi.Execute(inPolygons[i], EdgesList);

            if (TemizlenmisPoligon !=null)
            {
                _polygons.Add(new List<Vector2i>(TemizlenmisPoligon));

                TotalVertexCount += TemizlenmisPoligon.Length;
            }
        }
        // Burada toplam Edge nokta sayýsýný belirliyoruz.

        for (int i = 0; i < EdgesList.Count; i++)
        {

            int vertexCount = EdgesList[i].Count;
            TotalVertexCount += (vertexCount - 1) * 4;
            EdgeTriangleIndexCount += (vertexCount - 1) * 6;
        }

        Vector3[] vertices = new Vector3[TotalVertexCount];
        Vector2f[] TexCords = new Vector2f[TotalVertexCount];

        List<int> Triangles = new();
        int[] EdgeTriangles = new int[EdgeTriangleIndexCount];


        int vertexIndex = 0;
        int vertexOffset = 0;

        // Poligonlarý oluþturuyoruz.

        for (int i = 0; i < _polygons.Count; i++)
        {

            List<Vector2i> _L_Polygon = _polygons[i];
            int vertextCount = _L_Polygon.Count;

            for (int a = vertextCount-1; a >= 0; a--)
            {
                Vector3 point= _L_Polygon[a].ToVector3f();
                vertices[vertexIndex] = point;
                TexCords[vertexIndex] = new Vector2f(point.x / width, point.y / height);
                vertexIndex++;
            }

            Triangulate.Execute(vertices,vertexOffset,vertexOffset + vertextCount,Triangles);
            vertexOffset += vertextCount;
        }


        // Edge oluþturuyoruz.

        int edgeTriangleIndex = 0;
        

        // Burada toplam köþe sayýsýný belirliyoruz.
        for (int i = 0; i < EdgesList.Count; i++)
        {

            List<Vector2f> EdgePoints = EdgesList[i];

            int vertexCount = EdgePoints.Count;
            Vector3 point1,point2;


            for (int a = 0; a < vertexCount-1; a++)
            {
                point1 = EdgePoints[a].ToVector3f();
                point2 = EdgePoints[a+1].ToVector3f();

                vertices[vertexIndex + 0] = point1;
                vertices[vertexIndex + 2] = point2;

                point1.z += depth;
                point2.z += depth;

                vertices[vertexIndex + 1] = point1;
                vertices[vertexIndex + 3] = point2;

                EdgeTriangles[edgeTriangleIndex + 0] = vertexIndex;
                EdgeTriangles[edgeTriangleIndex + 1] = vertexIndex+2;
                EdgeTriangles[edgeTriangleIndex + 2] = vertexIndex+1;
                EdgeTriangles[edgeTriangleIndex + 3] = vertexIndex+2;
                EdgeTriangles[edgeTriangleIndex + 4] = vertexIndex+3;
                EdgeTriangles[edgeTriangleIndex + 5] = vertexIndex+1;

                vertexIndex += 4;
                edgeTriangleIndex += 6;

            }

        }


        Triangles.AddRange(EdgeTriangles);
        _Mesh.Clear();
        _Mesh.vertices = vertices;

        _Mesh.uv = TexCords;
        _Mesh.triangles= Triangles.ToArray();

        _Mesh.RecalculateNormals();


        CollidersGuncelle(EdgesList);


    }

    void  CollidersGuncelle (List<List<Vector2>> EdgesList) // UpdateColliders
    {
        int ColliderCount = _Colliders.Count;
        int EdgesCount = EdgesList.Count;

        if (ColliderCount < EdgesCount)
        {
            for (int i = EdgesCount - ColliderCount; i > 0; i--)
            {
                _Colliders.Add(gameObject.AddComponent<EdgeCollider2D>());
            }
        }
        else if (EdgesCount < ColliderCount)
        {
            for (int i = ColliderCount -1; i >= EdgesCount; i--)
            {
                Destroy(_Colliders[i]);
                _Colliders.RemoveAt(i);
            }
        }

        for (int i = 0; i < _Colliders.Count; i++)
        {
            _Colliders[i].points = EdgesList[i].ToArray();
        }
    }

}
