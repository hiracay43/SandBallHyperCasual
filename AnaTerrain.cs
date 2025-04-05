
    public void Kirpma (IClip clip)
    {

        BlokYonetimi.Oran = (int64)(DelikPuruzOrani / 100f * _BlokBoyutu * VectorEx.float2int64);

        List<Vector2i> clipVertices = clip.VerteksleriGetir();

        ClipBounds bounds = clip.SinirlariGetir();

        int x1 = Mathf.Max(0, (int)(bounds.AltNokta.x / _BlokBoyutu));
        if (x1 > TerrainBoyutX - 1) return;

        int y1 = Mathf.Max(0, (int)(bounds.AltNokta.y / _BlokBoyutu));
        if (y1 > TerrainBoyutY - 1) return;

        int x2 = Mathf.Min(TerrainBoyutX - 1, (int)(bounds.UstNokta.x / _BlokBoyutu));
        if (x2 < 0) return;

        int y2 = Mathf.Min(TerrainBoyutY - 1, (int)(bounds.UstNokta.y / _BlokBoyutu));
        if (y2 < 0) return;


        for (int x = x1; x <= x2; x++)
        {

            for (int y = y1; y <= y2; y++)
            {
                if (clip.BlokKesismesiniKontrolEt(new Vector2f((x+ .5f) * _BlokBoyutu, (y + .5f) * _BlokBoyutu),_BlokBoyutu))
                {

                    YokEdilebilirBlok Block = _Bloklar[x + TerrainBoyutX * y];

                    List<List<Vector2i>> Pozisyon = new();

                    Clipper clipper = new();

                    clipper.AddPolygons(Block._Polygons, PolyType.ptSubject);
                    clipper.AddPolygon(clipVertices, PolyType.ptClip);
                    clipper.Execute(ClipType.ctDifference, Pozisyon, PolyFillType.pftNonZero, PolyFillType.pftNonZero);

                    BlokSinirlariniGuncelle(x, y);

                    Block.GeometriyiGuncelle(Pozisyon, Genislik, Yukseklik, _Derinlik);

                }

                
            }
        }
    }
}
