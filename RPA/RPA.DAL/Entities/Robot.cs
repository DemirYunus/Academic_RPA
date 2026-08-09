using System;
using System.Collections.Generic;

public class Robot
{
    // İstenen özellikler (Properties)
    public string KaynakAdi { get; set; }
    public List<Instance> IIR { get; set; }
    public string[] YukluYazilimlar { get; set; }
    public string TahsisliOldBlm { get; set; }
    public List<IdleWindow> LstIdleWindow { get; set; }

    // Sınıf örneği oluşturulduğunda listelerin null referans hatası vermemesi için yapıcı metot
    public Robot()
    {
        IIR = new List<Instance>();
        YukluYazilimlar = Array.Empty<string>(); // Boş bir dizi olarak başlatır
        LstIdleWindow = new List<IdleWindow>();
    }
}

// Sizin ilettiğiniz IdleWindow sınıfı
public class IdleWindow
{
    public int Start { get; set; }
    public int End { get; set; }

    public IdleWindow(int start, int end)
    {
        Start = start;
        End = end;
    }

    public override string ToString()
    {
        return $"{Start}-{End}";
    }
}