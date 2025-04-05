using OlcayKalyoncuoglu;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Vector2f = UnityEngine.Vector2;
using Vector2i = ClipperLib.IntPoint;
using int64 = System.Int64;
using UnityEngine.UIElements;

public class AnaTerrain : MonoBehaviour
{
    public Material _Material;

    [Range(.5f, 2f)]
    public float _BlokBoyutu;

    Int64 _BlokBoyutununOlcegi;

    [Range(0f, 5f)]
    public float DelikPuruzOrani;

    [Range(1, 100)]
    public int TerrainBoyutX = 10;

    [Range(1, 100)]
    public int TerrainBoyutY = 10;

    public float _Derinlik = 1f;

    float Genislik;
    float Yukselik;

    YokEdilebilirBlok[] _Bloklar;

    private void Awake()
    {
        BlokYonetimi.Oran =(int64)(DelikPuruzOrani / 100f * _BlokBoyutu * VectorEx.float2int64);

        Genislik = _BlokBoyutu * TerrainBoyutX;
        Yukselik = _BlokBoyutu * TerrainBoyutY;
        _BlokBoyutununOlcegi = (int64)(_BlokBoyutu * VectorEx.float2int64);

        Kurulum();
    }

    void Kurulum() // Initialize
    {
        _Bloklar = new YokEdilebilirBlok[TerrainBoyutX * TerrainBoyutY];


        for (int x = 0; x < TerrainBoyutX; x++)
        {
            for (int y = 0; y < TerrainBoyutY; y++)
            {
                List<List<Vector2i>> poligonlar = new();
                List<Vector2i> verteksler = new()
                {
                    new Vector2i { x = x * _BlokBoyutununOlcegi, y = (y + 1)* _BlokBoyutununOlcegi},
                    new Vector2i { x = x * _BlokBoyutununOlcegi, y = y * _BlokBoyutununOlcegi},
                    new Vector2i { x = (x + 1 ) * _BlokBoyutununOlcegi, y = y * _BlokBoyutununOlcegi},
                    new Vector2i { x = (x + 1 ) * _BlokBoyutununOlcegi, y = (y + 1)* _BlokBoyutununOlcegi},
                };

                poligonlar.Add(verteksler);

                int idx = x + TerrainBoyutX * y;

                YokEdilebilirBlok block = BlokOlustur();
                _Bloklar[idx] = block;

                BlokSinirlariniGuncelle(x, y);

                block.GeometriyiGuncelle(poligonlar, Genislik, Yukselik, _Derinlik);
            }
        }

    }
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void BlokSinirlariniGuncelle(int x , int y)
    {
        int lx = x;
        int ly = y;
        int ux = x+1;
        int uy = y+1;

        if (lx == 0) lx = -1;
        if (ly == 0) ly = -1;
        if (ux == TerrainBoyutX) ux =  TerrainBoyutX +1;
        if (uy == TerrainBoyutY) uy = TerrainBoyutY + 1;


        BlokYonetimi.MevcutAltNokta = new Vector2i
        {
            x = lx * _BlokBoyutununOlcegi,
            y = ly * _BlokBoyutununOlcegi

        };
        BlokYonetimi.MevcutUstNokta = new Vector2i
        {
            x = ux * _BlokBoyutununOlcegi,
            y = uy * _BlokBoyutununOlcegi

        };
    }

    YokEdilebilirBlok BlokOlustur()
    {

        GameObject OlusanObje = new()
        {
            name = "YokedilebilirBlok"
        };
        OlusanObje.transform.SetParent(transform);
        OlusanObje.transform.localPosition = Vector3.zero;

        YokEdilebilirBlok BlokKopya = OlusanObje.AddComponent<YokEdilebilirBlok>();
        BlokKopya.MateryalTanimla(_Material);

        return BlokKopya;


        // BURADA OBJE HAVUZU YAPACAÐIZ

    }
}
