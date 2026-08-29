using System;
using System.Collections.Generic;
using System.Linq;

public class IdleWindowUpdater
{
    /// <summary>
    /// Robotun üzerindeki işleri (Instance) tarayarak 0 ile horizonEnd (Örn: 1440) 
    /// arasındaki boş zaman (IdleWindow) pencerelerini hesaplar ve günceller.
    /// </summary>
    public static void UpdateRobotWindows(Robot robot, int horizonEnd = 1440)
    {
        // 1. Liste başlatma ve eski pencereleri temizleme
        if (robot.LstIdleWindow == null)
        {
            robot.LstIdleWindow = new List<IdleWindow>();
        }
        robot.LstIdleWindow.Clear();

        // 2. Robotun içi tamamen boşsa, ufuk çizgisine kadar tek bir devasa boşluk vardır
        if (robot.IIR == null || !robot.IIR.Any())
        {
            // Yeni modele uygun olarak yapıcı metot (constructor) kullanıldı
            robot.LstIdleWindow.Add(new IdleWindow(0, horizonEnd));
            return;
        }

        // 3. İşleri (Instance'ları) başlangıç zamanlarına göre küçükten büyüğe sırala
        // StartTime ve FinishTime double? olduğu için null olmayanları alıyoruz
        var sortedInstances = robot.IIR
            .Where(i => i.StartTime.HasValue && i.FinishTime.HasValue)
            .OrderBy(i => i.StartTime.Value)
            .ToList();

        int currentTime = 0;

        foreach (var instance in sortedInstances)
        {
            // double değerleri karşılaştırma ve atama için int'e çeviriyoruz
            int startTime = (int)instance.StartTime.Value;
            int finishTime = (int)instance.FinishTime.Value;

            // Mevcut zaman sıradaki işin başlangıcından küçükse, arada boşluk vardır
            if (currentTime < startTime)
            {
                // Yeni nesne modelinize uygun olarak parametreli oluşturuldu
                robot.LstIdleWindow.Add(new IdleWindow(currentTime, startTime));
            }

            // Zamanı, bu işin bitiş zamanına ilerlet. 
            currentTime = Math.Max(currentTime, finishTime);
        }

        // 4. Tüm işler bittikten sonra gün sonuna (1440'a) kadar hala boşluk kaldıysa onu da ekle
        if (currentTime < horizonEnd)
        {
            robot.LstIdleWindow.Add(new IdleWindow(currentTime, horizonEnd));
        }
    }
}