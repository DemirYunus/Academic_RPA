using System;
using System.Collections.Generic;

public class Robot
{
    // İstenen özellikler (Properties) güncellendi
    public string RobotName { get; set; }
    public List<Instance> IIR { get; set; }

    // String dizisi yerine Software sınıfı listesi kullanıldı
    public List<Software> LoadedSoftware { get; set; }

    public string AllocatedDepartment { get; set; }
    public List<IdleWindow> LstIdleWindow { get; set; }

    // Sınıf örneği oluşturulduğunda listelerin null referans hatası vermemesi için yapıcı metot
    public Robot()
    {
        IIR = new List<Instance>();
        LoadedSoftware = new List<Software>();
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