using OlcayKalyoncuoglu;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Vector2f = UnityEngine.Vector2;
using Vector2i = ClipperLib.IntPoint;

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
        if (DokunmaKontrolu.TouchCount>0)
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
