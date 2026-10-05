using System.Diagnostics;
using System.Text;

/**
   "Tersine" Kargo ve Lojistik Simülasyonu - konsol prototipi

   Temel döngü:
   1. Oyna   : Banttan gelen kargoyu doğru kamyona gönder (1, 2, 3).
   2. Kaos   : Bant her bölümde hızlanır. Özel kargolar gelir:
               - Hassas eşya : fırlatılamaz, yavaşça kaydırılır (tuş + ENTER)
               - Patlayıcı   : kamyona girerse patlar, BOŞLUK ile imha bölgesine
               - Canlı hayvan: sadece 4 numaralı hayvan taşıma aracına
   3. Geliştir: Kazandığın altınla robot, bant freni, sigorta ve tema al.

   Enerji sistemi: 5 enerji, başarısız bölüm 1 enerji yer, zamanla dolar.
   Ödüllü reklamlar burada sadece simülasyondur (gerçek oyunda AdMob / Unity Ads).
*/
class KargoOyunu
{
    enum KargoTuru { Normal, Hassas, Patlayici, Hayvan }

    enum Sonuc { Dogru, Yanlis, Kirildi, Patlama, ZamanDoldu }

    class Kargo
    {
        public KargoTuru Tur;
        public int Hedef; // Normal ve hassas kargolar için 1-3 arası kamyon
    }

    class Tema
    {
        public string Ad = "";
        public int Fiyat;
        public string[] Kategoriler = Array.Empty<string>();
        public ConsoleColor[] Renkler = Array.Empty<ConsoleColor>();
    }

    static readonly Tema[] Temalar =
    {
        new Tema { Ad = "Klasik Depo", Fiyat = 0,
            Kategoriler = new[] { "ELEKTRONİK", "GİYİM", "GIDA" },
            Renkler = new[] { ConsoleColor.Cyan, ConsoleColor.Magenta, ConsoleColor.Yellow } },
        new Tema { Ad = "Neon Kutular", Fiyat = 300,
            Kategoriler = new[] { "NEON MAVİ", "NEON PEMBE", "NEON YEŞİL" },
            Renkler = new[] { ConsoleColor.Blue, ConsoleColor.Magenta, ConsoleColor.Green } },
        new Tema { Ad = "Noel Baba'nın Hediye Merkezi", Fiyat = 500,
            Kategoriler = new[] { "OYUNCAK", "ŞEKER", "KİTAP" },
            Renkler = new[] { ConsoleColor.Red, ConsoleColor.White, ConsoleColor.DarkYellow } },
    };

    const int MaksEnerji = 5;
    const int EnerjiDolumSaniye = 120;

    static readonly Random Rnd = new();

    static int altin = 0;
    static int bolum = 1;
    static int enerji = MaksEnerji;
    static DateTime sonEnerjiDolumu = DateTime.Now;

    static int robotSeviye = 0;   // normal kargoları kendisi ayırma şansı
    static int frenSeviye = 0;    // bant süresine ek milisaniye
    static int sigortaSeviye = 0; // bölüm başına ek hata hakkı

    static int aktifTema = 0;
    static readonly bool[] sahipTemalar = { true, false, false };

    public static void Oyna()
    {
        try { Console.OutputEncoding = Encoding.UTF8; } catch { }

        while (true)
        {
            EnerjiGuncelle();
            Temizle();
            RenkliSatir("=== TERSİNE KARGO ===", ConsoleColor.Yellow);
            Console.WriteLine($"Bölüm: {bolum}   Altın: {altin}   Enerji: {EnerjiYazisi()}");
            Console.WriteLine($"Tema: {Temalar[aktifTema].Ad}");
            Console.WriteLine();
            Console.WriteLine("[1] Oyna");
            Console.WriteLine("[2] Depo Mağazası");
            Console.WriteLine("[3] Temalar");
            Console.WriteLine("[4] Nasıl Oynanır?");
            Console.WriteLine("[0] Çıkış");

            switch (Console.ReadKey(true).KeyChar)
            {
                case '1': BolumOyna(); break;
                case '2': Magaza(); break;
                case '3': TemaMenusu(); break;
                case '4': NasilOynanir(); break;
                case '0': return;
            }
        }
    }

    static void BolumOyna()
    {
        EnerjiGuncelle();
        if (enerji == 0)
        {
            Temizle();
            RenkliSatir("Enerjin bitti!", ConsoleColor.Red);
            Console.WriteLine($"Bir sonraki enerji: {SonrakiEnerjiSaniye()} sn");
            Console.WriteLine("[R] Reklam izle (+1 enerji)   [başka tuş] Menüye dön");
            if (Console.ReadKey(true).Key != ConsoleKey.R)
                return;
            ReklamIzle();
            enerji++;
        }

        int paketSayisi = 8 + bolum * 2;
        int hakSayisi = 3 + sigortaSeviye;
        int sureMs = Math.Max(700, (int)(3000 * Math.Pow(0.92, bolum - 1))) + frenSeviye * 250;
        int hata = 0;
        int kazanc = 0;
        int combo = 0;

        for (int geri = 3; geri > 0; geri--)
        {
            Temizle();
            RenkliSatir($"Bölüm {bolum} başlıyor... {geri}", ConsoleColor.Yellow);
            Thread.Sleep(700);
        }

        for (int i = 0; i < paketSayisi && hata < hakSayisi; i++)
        {
            Kargo kargo = KargoUret();

            Temizle();
            Console.WriteLine($"Bölüm {bolum}  |  Paket {i + 1}/{paketSayisi}  |  Kazanç: {kazanc}  |  Combo: x{1 + combo / 5}");
            Console.Write("Hata hakkı: ");
            Renkli(new string('♥', hakSayisi - hata) + new string('·', hata), ConsoleColor.Red);
            Console.WriteLine();
            KamyonlariCiz();
            Console.WriteLine();
            KargoCiz(kargo);
            Console.WriteLine();

            if (kargo.Tur == KargoTuru.Normal && robotSeviye > 0 && Rnd.Next(100) < robotSeviye * 15)
            {
                RenkliSatir("ROBOT KOL kargoyu kaptı ve doğru kamyona koydu! +5", ConsoleColor.Cyan);
                kazanc += 5;
                Thread.Sleep(700);
                continue;
            }

            TamponuBosalt();
            Sonuc sonuc = Degerlendir(kargo, sureMs);
            Console.WriteLine();

            switch (sonuc)
            {
                case Sonuc.Dogru:
                    combo++;
                    int puan = (kargo.Tur == KargoTuru.Normal ? 10 : 20) * (1 + combo / 5);
                    kazanc += puan;
                    RenkliSatir($"DOĞRU! +{puan}", ConsoleColor.Green);
                    break;
                case Sonuc.Yanlis:
                    combo = 0;
                    hata++;
                    RenkliSatir("YANLIŞ KAMYON!", ConsoleColor.Red);
                    break;
                case Sonuc.Kirildi:
                    combo = 0;
                    hata++;
                    RenkliSatir("KIRILDI! Hassas eşya fırlatılmaz, ENTER ile yavaşça kaydırılır.", ConsoleColor.Red);
                    break;
                case Sonuc.Patlama:
                    combo = 0;
                    hata += 2;
                    RenkliSatir("BOOM! Patlayıcı kamyonda patladı! (-2 hak)", ConsoleColor.Red);
                    break;
                case Sonuc.ZamanDoldu:
                    combo = 0;
                    hata++;
                    RenkliSatir("Kargo banttan düştü!", ConsoleColor.Red);
                    break;
            }
            Thread.Sleep(600);
        }

        Temizle();
        if (hata < hakSayisi)
        {
            int bonus = bolum * 20;
            kazanc += bonus;
            RenkliSatir($"BÖLÜM {bolum} TAMAMLANDI!", ConsoleColor.Green);
            Console.WriteLine($"Kazanç: {kazanc} altın (bölüm bonusu +{bonus} dahil)");
            Console.WriteLine();
            Console.WriteLine("[R] Reklam izle, 3 KAT altın kazan   [başka tuş] Devam");
            if (Console.ReadKey(true).Key == ConsoleKey.R)
            {
                ReklamIzle();
                kazanc *= 3;
                RenkliSatir($"Tebrikler! {kazanc} altın kazandın.", ConsoleColor.Yellow);
                Thread.Sleep(1000);
            }
            altin += kazanc;
            bolum++;
        }
        else
        {
            EnerjiGuncelle();
            enerji--;
            int kalan = kazanc / 2;
            altin += kalan;
            RenkliSatir("Depo karıştı! Bölüm başarısız.", ConsoleColor.Red);
            Console.WriteLine($"Kazancın yarısı ({kalan} altın) kurtarıldı. -1 enerji");
            Console.WriteLine("Devam etmek için bir tuşa bas...");
            Console.ReadKey(true);
        }
    }

    static Sonuc Degerlendir(Kargo kargo, int sureMs)
    {
        var sw = Stopwatch.StartNew();
        ConsoleKeyInfo? tus = TusBekle(sureMs, sw);
        if (tus == null)
            return Sonuc.ZamanDoldu;

        char c = tus.Value.KeyChar;
        bool imha = tus.Value.Key == ConsoleKey.Spacebar;

        switch (kargo.Tur)
        {
            case KargoTuru.Patlayici:
                return imha ? Sonuc.Dogru : Sonuc.Patlama;

            case KargoTuru.Hayvan:
                return c == '4' ? Sonuc.Dogru : Sonuc.Yanlis;

            case KargoTuru.Hassas:
                if (c != (char)('0' + kargo.Hedef))
                    return Sonuc.Yanlis;
                Console.WriteLine();
                RenkliSatir("Yavaşça kaydırılıyor... onay için ENTER!", ConsoleColor.Yellow);
                ConsoleKeyInfo? onay = TusBekle(sureMs, sw);
                if (onay == null)
                    return Sonuc.ZamanDoldu;
                return onay.Value.Key == ConsoleKey.Enter ? Sonuc.Dogru : Sonuc.Kirildi;

            default:
                return c == (char)('0' + kargo.Hedef) ? Sonuc.Dogru : Sonuc.Yanlis;
        }
    }

    // Süre dolana kadar tuş bekler, bu arada bant süresini çubuk olarak gösterir.
    static ConsoleKeyInfo? TusBekle(int toplamMs, Stopwatch sw)
    {
        long sonCizim = -1000;
        while (sw.ElapsedMilliseconds < toplamMs)
        {
            if (Console.KeyAvailable)
                return Console.ReadKey(true);

            if (sw.ElapsedMilliseconds - sonCizim >= 100)
            {
                sonCizim = sw.ElapsedMilliseconds;
                SureCubuguCiz(toplamMs - (int)sonCizim, toplamMs);
            }
            Thread.Sleep(15);
        }
        SureCubuguCiz(0, toplamMs);
        return null;
    }

    static void SureCubuguCiz(int kalanMs, int toplamMs)
    {
        const int Genislik = 20;
        int dolu = Math.Clamp((int)Math.Ceiling(Genislik * (double)kalanMs / toplamMs), 0, Genislik);
        ConsoleColor renk = dolu > Genislik / 2 ? ConsoleColor.Green
                          : dolu > Genislik / 4 ? ConsoleColor.Yellow
                          : ConsoleColor.Red;
        Console.Write("\rBant: [");
        Renkli(new string('#', dolu), renk);
        Console.Write(new string('-', Genislik - dolu) + $"] {kalanMs / 1000.0:0.0} sn   ");
    }

    static Kargo KargoUret()
    {
        int r = Rnd.Next(100);
        if (bolum >= 4 && r < 12)
            return new Kargo { Tur = KargoTuru.Hayvan };
        if (bolum >= 3 && r < 24)
            return new Kargo { Tur = KargoTuru.Patlayici };
        if (bolum >= 2 && r < 40)
            return new Kargo { Tur = KargoTuru.Hassas, Hedef = Rnd.Next(1, 4) };
        return new Kargo { Tur = KargoTuru.Normal, Hedef = Rnd.Next(1, 4) };
    }

    static void KamyonlariCiz()
    {
        Tema tema = Temalar[aktifTema];
        for (int i = 0; i < 3; i++)
        {
            Console.Write($"[{i + 1}] ");
            Renkli(tema.Kategoriler[i], tema.Renkler[i]);
            Console.Write("   ");
        }
        if (bolum >= 4)
        {
            Console.Write("[4] ");
            Renkli("HAYVAN TAŞIMA", ConsoleColor.Green);
            Console.Write("   ");
        }
        if (bolum >= 3)
        {
            Console.Write("[BOŞLUK] ");
            Renkli("İMHA", ConsoleColor.DarkRed);
        }
        Console.WriteLine();
    }

    static void KargoCiz(Kargo kargo)
    {
        Tema tema = Temalar[aktifTema];
        switch (kargo.Tur)
        {
            case KargoTuru.Normal:
                Kutu(tema.Kategoriler[kargo.Hedef - 1], tema.Renkler[kargo.Hedef - 1]);
                break;
            case KargoTuru.Hassas:
                Kutu(tema.Kategoriler[kargo.Hedef - 1] + " (KIRILACAK)", tema.Renkler[kargo.Hedef - 1]);
                Console.WriteLine("Hassas eşya: tuşa bas, sonra ENTER ile yavaşça kaydır.");
                break;
            case KargoTuru.Patlayici:
                Kutu("!! PATLAYICI !!", ConsoleColor.Red);
                Console.WriteLine("Kamyona koyma! BOŞLUK ile imha bölgesine gönder.");
                break;
            case KargoTuru.Hayvan:
                Kutu("CANLI HAYVAN", ConsoleColor.Green);
                Console.WriteLine("Sadece 4 numaralı hayvan taşıma aracına!");
                break;
        }
    }

    static void Kutu(string yazi, ConsoleColor renk)
    {
        string cizgi = "+" + new string('-', yazi.Length + 4) + "+";
        Renkli(cizgi + "\n|  " + yazi + "  |\n" + cizgi, renk);
        Console.WriteLine();
    }

    static void Magaza()
    {
        while (true)
        {
            Temizle();
            RenkliSatir("=== DEPO MAĞAZASI ===", ConsoleColor.Yellow);
            Console.WriteLine($"Altın: {altin}");
            Console.WriteLine();
            MagazaSatiri(1, "Sıralama Robotu", "normal kargoları %15 şansla kendisi ayırır", robotSeviye, 3, RobotFiyat());
            MagazaSatiri(2, "Bant Freni", "her paket için +0.25 sn", frenSeviye, 4, FrenFiyat());
            MagazaSatiri(3, "Kargo Sigortası", "bölüm başına +1 hata hakkı", sigortaSeviye, 2, SigortaFiyat());
            Console.WriteLine("[0] Geri");

            switch (Console.ReadKey(true).KeyChar)
            {
                case '1': SatinAl(ref robotSeviye, 3, RobotFiyat()); break;
                case '2': SatinAl(ref frenSeviye, 4, FrenFiyat()); break;
                case '3': SatinAl(ref sigortaSeviye, 2, SigortaFiyat()); break;
                case '0': return;
            }
        }
    }

    static int RobotFiyat() => 150 * (robotSeviye + 1);
    static int FrenFiyat() => 100 * (frenSeviye + 1);
    static int SigortaFiyat() => 200 * (sigortaSeviye + 1);

    static void MagazaSatiri(int no, string ad, string aciklama, int seviye, int maks, int fiyat)
    {
        string durum = seviye >= maks ? "MAKS" : $"{fiyat} altın";
        Console.WriteLine($"[{no}] {ad} (Seviye {seviye}/{maks}) - {durum}");
        Console.WriteLine($"     {aciklama}");
    }

    static void SatinAl(ref int seviye, int maks, int fiyat)
    {
        if (seviye >= maks)
            RenkliSatir("Bu geliştirme zaten maksimum seviyede.", ConsoleColor.Yellow);
        else if (altin < fiyat)
            RenkliSatir("Yeterli altının yok!", ConsoleColor.Red);
        else
        {
            altin -= fiyat;
            seviye++;
            RenkliSatir("Satın alındı!", ConsoleColor.Green);
        }
        Console.WriteLine();
        Thread.Sleep(800);
    }

    static void TemaMenusu()
    {
        while (true)
        {
            Temizle();
            RenkliSatir("=== TEMALAR ===", ConsoleColor.Yellow);
            Console.WriteLine($"Altın: {altin}");
            Console.WriteLine();
            for (int i = 0; i < Temalar.Length; i++)
            {
                string durum = i == aktifTema ? "KULLANILIYOR"
                             : sahipTemalar[i] ? "Sahipsin"
                             : $"{Temalar[i].Fiyat} altın";
                Console.WriteLine($"[{i + 1}] {Temalar[i].Ad} - {durum}");
            }
            Console.WriteLine("[0] Geri");

            char c = Console.ReadKey(true).KeyChar;
            if (c == '0')
                return;

            int secim = c - '1';
            if (secim < 0 || secim >= Temalar.Length)
                continue;

            if (!sahipTemalar[secim])
            {
                if (altin < Temalar[secim].Fiyat)
                {
                    RenkliSatir("Yeterli altının yok!", ConsoleColor.Red);
                    Thread.Sleep(800);
                    continue;
                }
                altin -= Temalar[secim].Fiyat;
                sahipTemalar[secim] = true;
            }
            aktifTema = secim;
        }
    }

    static void NasilOynanir()
    {
        Temizle();
        RenkliSatir("=== NASIL OYNANIR? ===", ConsoleColor.Yellow);
        Console.WriteLine("Banttan gelen kargoyu süre bitmeden doğru kamyona gönder.");
        Console.WriteLine("  1, 2, 3     : kargonun kategorisine göre kamyon");
        Console.WriteLine("  Hassas eşya : doğru tuş + ENTER (fırlatırsan kırılır)  [Bölüm 2+]");
        Console.WriteLine("  Patlayıcı   : BOŞLUK ile imha (kamyona girerse -2 hak) [Bölüm 3+]");
        Console.WriteLine("  Canlı hayvan: 4 ile hayvan taşıma aracı                 [Bölüm 4+]");
        Console.WriteLine();
        Console.WriteLine("Her bölümde bant hızlanır. Art arda doğru yaparsan combo çarpanı artar.");
        Console.WriteLine("Başarısız bölüm 1 enerji yer; enerji her 2 dakikada 1 dolar.");
        Console.WriteLine();
        Console.WriteLine("Menüye dönmek için bir tuşa bas...");
        Console.ReadKey(true);
    }

    static void ReklamIzle()
    {
        for (int sn = 3; sn > 0; sn--)
        {
            Console.Write($"\rReklam oynatılıyor (simülasyon)... {sn} ");
            Thread.Sleep(1000);
        }
        Console.WriteLine();
    }

    static void EnerjiGuncelle()
    {
        if (enerji >= MaksEnerji)
        {
            sonEnerjiDolumu = DateTime.Now;
            return;
        }
        int dolan = (int)((DateTime.Now - sonEnerjiDolumu).TotalSeconds / EnerjiDolumSaniye);
        if (dolan > 0)
        {
            enerji = Math.Min(MaksEnerji, enerji + dolan);
            sonEnerjiDolumu = sonEnerjiDolumu.AddSeconds(dolan * EnerjiDolumSaniye);
        }
    }

    static int SonrakiEnerjiSaniye() =>
        EnerjiDolumSaniye - (int)(DateTime.Now - sonEnerjiDolumu).TotalSeconds;

    static string EnerjiYazisi()
    {
        string yazi = $"{enerji}/{MaksEnerji}";
        if (enerji < MaksEnerji)
            yazi += $" (+1 için {SonrakiEnerjiSaniye()} sn)";
        return yazi;
    }

    static void TamponuBosalt()
    {
        while (Console.KeyAvailable)
            Console.ReadKey(true);
    }

    static void Renkli(string yazi, ConsoleColor renk)
    {
        Console.ForegroundColor = renk;
        Console.Write(yazi);
        Console.ResetColor();
    }

    static void RenkliSatir(string yazi, ConsoleColor renk)
    {
        Renkli(yazi, renk);
        Console.WriteLine();
    }

    static void Temizle()
    {
        try { Console.Clear(); } catch (IOException) { }
    }
}
