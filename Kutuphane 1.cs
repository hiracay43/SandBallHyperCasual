using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Vector2f = UnityEngine.Vector2;
using Vector2i = ClipperLib.IntPoint;
using int64 = System.Int64;


namespace OlcayKalyoncuoglu
{
    public static class BlokYonetimi
    {

        public static int64 Oran;
        public static Vector2i MevcutAltNokta;
        public static Vector2i MevcutUstNokta;
        public static int GetMask(Vector2i vektor)
        {

            int mask = 0;

            if (vektor.x == MevcutAltNokta.x)
                mask |= 1;

            if (vektor.x == MevcutUstNokta.x)
                mask |= 2;

            if (vektor.y == MevcutAltNokta.y)
                mask |= 4;

            if (vektor.y == MevcutUstNokta.y)
                mask |= 8;

            return mask;

        }
        public class DelikAc : MonoBehaviour, IClip
        {
            private struct DokunmaOrtusmeKontrolu
            {
                public float a;
                public float b;
                public float c;
                public float aci;
                public float BolunenSayi;

                public DokunmaOrtusmeKontrolu(Vector2 p1, Vector2 p2)
                {
                    Vector2 d = p2 - p1;
                    float m = d.magnitude; // vektörün uzunluðunu döndürür - Vektörün uzunluðu kareköktür
                    a = -d.y / m;
                    b = d.x / m;
                    c = -(a * p1.x + b * p1.y);
                    //radyan cinsinden açý deðerini derece cinsine çevirmemiz mümkündür.
                    aci = Mathf.Rad2Deg * Mathf.Atan2(-a, b); // tanjantýn y/x þeklinde hesaplanma

                    float da;
                    if (d.x / d.y < 0f)
                        da = 45 + aci;
                    else
                        da = 45 - aci;
                    //- Mathf.Abs-  Verilen sayýnýn mutlak deðerini elde etmemizi saðlar. -20 20.5 de olsa 20 verir
                    // Mathf.Cos - Radyan cinsinden verilen açý deðerinin kosinüs deðerini elde etmemizi saðlar. -1 ve 1 arasýnda deðer döndürür.
                    BolunenSayi = Mathf.Abs(1.0f / 1.4f * Mathf.Cos(Mathf.Deg2Rad * da));
                }

                public float GetDistance(Vector2 p)
                {
                    return Mathf.Abs(a * p.x + b * p.y + c);
                }
            }

            [SerializeField] AnaTerrain _Terrain;

            [SerializeField] float _Cap = 1.2f;
            float _YariCap = 1.2f;
            [SerializeField] int _SegmentSayisi = 10;
            [SerializeField] float _DokunmaMesafesi = .1f;

            Vector2f _MevcutDokunmaNoktasi;
            Vector2f _OncekiDokunmaNoktasi;

            TouchPhase _TouchPhase;

            DokunmaOrtusmeKontrolu _TouchLine;

            List<Vector2i> _Verteksler = new();

            Camera MainCamera;
            float CameraZPos;

            private void Awake()
            {
                MainCamera = Camera.main;
                CameraZPos = MainCamera.transform.position.z;
                _YariCap = _Cap / 2f;

            }
            void Update()
            {
                DokunmayiGuncelle();
            }
            void DokunmayiGuncelle()
            {
                if (DokunmaKontrolu.TouchCount > 0)
                {

                    Touch touch = DokunmaKontrolu.GetTouch(0);
                    Vector2 touchPosition = touch.position;

                    _TouchPhase = touch.phase;

                    if (touch.phase == TouchPhase.Began)
                    {
                        Vector2 PlanePozisyonu = MainCamera.ScreenToWorldPoint(new Vector3(touchPosition.x, touchPosition.y, -CameraZPos));
                        _MevcutDokunmaNoktasi = PlanePozisyonu - _Terrain.TerrainPozisyonunuGetir();
                        VerteksleriOlusturBaslangic(_MevcutDokunmaNoktasi);
                        _Terrain.Kirpma(this);
                        _OncekiDokunmaNoktasi = _MevcutDokunmaNoktasi;
                    }

                }

            }

            void VerteksleriOlusturBaslangic(Vector2 center)
            {
                _Verteksler.Clear();
                for (int i = 0; i < _SegmentSayisi; i++)
                {
                    float aci = Mathf.Deg2Rad * (-90f - 360f / _SegmentSayisi * i);
                    Vector2 point = new(center.x + _YariCap * Mathf.Cos(aci), center.y + _YariCap * Mathf.Sin(aci));
                    Vector2i Point_i64 = point.ToVector2i();
                    _Verteksler.Add(Point_i64);
                }
            }

        }
        public static bool KenarlardakiNoktaKontrolu(Vector2i v)
        {
            return v.x - MevcutAltNokta.x- Oran > 0 && v.y - MevcutAltNokta.y- Oran > 0 && MevcutUstNokta.x- v.x - Oran >0 && MevcutUstNokta.y - v.y - Oran >0;

        }
        public static void KenarlariOlustur(List<Vector2i> inPolygon, int[] mask, int a,int b, ref int RemoveCount)
        {

            if (b - a < 2)
                return;

            int begin = a + 1;
            int end = b - 1;
            int index = 0;
            float dmax = 0f;

            VectorEx.Line line = new(inPolygon[a], inPolygon[b]);



            for (int i = begin; i <= end; i++)
            {
                float d = line.GetDistance(inPolygon[i]);

                if (d > dmax)
                {
                    index = i;
                    dmax = d;
                }
            }

            if (dmax > Oran)
            {
                KenarlariOlustur(inPolygon, mask, a, index, ref RemoveCount);
                KenarlariOlustur(inPolygon, mask, index,b, ref RemoveCount);
            }else
            {
                for (int i = begin; i <= end; i++)
                {

                    if (KenarlardakiNoktaKontrolu(inPolygon[i]))
                    {
                        mask[i] = 1;
                        RemoveCount++;
                    }
                }
            }
        }              
      
    }
    public static class Triangulate
    {
        public static void Execute(Vector3[] vertices, int begin, int end, List<int> triangles)
        {
            int size = end - begin;
            if (size == 3)
            {
                triangles.Add(0 + begin);
                triangles.Add(1 + begin);
                triangles.Add(2 + begin);
            }
            else if (size > 3)
            {
                int remainingCount = size;
                bool[] visited = new bool[size];

                int a = 0 + begin;
                int b = 1 + begin;
                int c = 2 + begin;

                int it = size * 3;
                while (it >= 0)
                {
                    it--;
                    bool validTriangle = true;

                    if (!TriangleIsClockwise(vertices[a], vertices[b], vertices[c]))
                    {
                        validTriangle = false;
                    }

                    if (validTriangle)
                    {
                        for (int i = 0; i < size; i++)
                        {
                            int idx = i + begin;
                            if (visited[i] == false && idx != a && idx != b && idx != c)
                            {
                                if (TriangleContainsPoint(vertices[a], vertices[b], vertices[c], vertices[idx]))
                                {
                                    validTriangle = false;
                                    break;
                                }
                            }
                        }
                    }

                    if (validTriangle)
                    {
                        triangles.Add(a);
                        triangles.Add(b);
                        triangles.Add(c);

                        if (remainingCount == 3)
                        {
                            break;
                        }

                        visited[b - begin] = true;
                        remainingCount--;

                        b = c;
                    }
                    else
                    {
                        a = b;
                        b = c;
                    }

                    for (int i = 0; i < size; i++)
                    {
                        c++;
                        if (c >= end)
                            c = begin;
                        if (!visited[c - begin])
                        {
                            break;
                        }
                    }
                }
            }
        }

        public static bool TriangleContainsPoint(Vector2f A, Vector2f B, Vector2f C, Vector2f P)
        {
            float ax, ay, bx, by, cx, cy, apx, apy, bpx, bpy, cpx, cpy;

            ax = C.x - B.x; ay = C.y - B.y;
            bx = A.x - C.x; by = A.y - C.y;
            cx = B.x - A.x; cy = B.y - A.y;
            apx = P.x - A.x; apy = P.y - A.y;
            bpx = P.x - B.x; bpy = P.y - B.y;
            cpx = P.x - C.x; cpy = P.y - C.y;

            float C1 = ax * bpy - ay * bpx;
            float C2 = cx * apy - cy * apx;
            float C3 = bx * cpy - by * cpx;

            return (C1 > 0f && C2 > 0f && C3 > 0f) || (C1 < 0f && C2 < 0f && C3 < 0f);
        }

        public static bool TriangleIsClockwise(Vector2f A, Vector2f B, Vector2f C)
        {
            return (B.x - A.x) * (C.y - A.y) - (B.y - A.y) * (C.x - A.x) <= 0f;
        }
    }
    public static class VectorEx
    {
        public static float float2int64 = 100000.0f;

        public static Vector2i ToVector2i(this Vector2f p)
        {
            return new Vector2i
            {
                x = (int64)(p.x * float2int64),
                y = (int64)(p.y * float2int64)
            };
        }

        public static Vector3 ToVector3f(this Vector2f p)
        {
            return new Vector3
            {
                x = p.x,
                y = p.y,
                z = 0f
            };
        }

        public static Vector2f ToVector2f(this Vector2i p)
        {
            return new Vector2f
            {
                x = (float)(p.x / float2int64),
                y = (float)(p.y / float2int64)
            };
        }

        public static Vector3 ToVector3f(this Vector2i p)
        {
            return new Vector3
            {
                x = (float)(p.x / float2int64),
                y = (float)(p.y / float2int64),
                z = 0f
            };
        }

        public static float Cross(Vector2f A, Vector2f B)
        {
            float m = A.x * B.y - A.y * B.x;
            return m;
        }

        public static Vector2f Cross(Vector2 A, float s)
        {
            return new Vector2f { x = -A.y * s, y = A.x * s };
        }

        public struct Line // ax + by + c = 0
        {
            public float a;
            public float b;
            public float c;
            public float angle;

            public Line(Vector2i p1, Vector2i p2)
            {
                Vector2i d = new Vector2i { x = p2.x - p1.x, y = p2.y - p1.y };
                float m = Mathf.Sqrt(d.x * d.x + d.y * d.y);
                a = -d.y / m;
                b = d.x / m;
                c = -(a * p1.x + b * p1.y);
                angle = 0f;
            }

            public Line(Vector2 p1, Vector2 p2)
            {
                Vector2 d = p2 - p1;
                float m = d.magnitude;
                a = -d.y / m;
                b = d.x / m;
                c = -(a * p1.x + b * p1.y);
                angle = Mathf.Rad2Deg * Mathf.Atan2(-a, b);
            }

            public float GetDistance(Vector2i p)
            {
                return Mathf.Abs(a * p.x + b * p.y + c);
            }

            public float GetDistance(Vector2 p)
            {
                return Mathf.Abs(a * p.x + b * p.y + c);
            }
        }
    }
}


