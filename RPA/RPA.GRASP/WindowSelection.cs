using System;
using System.Collections.Generic;
using System.Linq;

public class WindowSelection
{
    // ========================================================================
    // 1. First Fit (İlk Uygun): Zaman ekseninde en erken başlayan boşluğu seçer.
    // ========================================================================
    public static RobotSelectionResult SelectFirstFit(RobotSelectionResult inputResult)
    {
        // En küçük Start değerine sahip pencereyi getir
        return ApplySelectionRule(inputResult, windows => windows.OrderBy(w => w.Start).First());
    }

    // ========================================================================
    // 2. Best Fit (En Uygun): Süre olarak en dar olan (kalan kapasiteyi en aza indiren) boşluğu seçer.
    // ========================================================================
    public static RobotSelectionResult SelectBestFit(RobotSelectionResult inputResult)
    {
        // Pencere uzunluğu (End - Start) en küçük olanı getir
        return ApplySelectionRule(inputResult, windows => windows.OrderBy(w => w.End - w.Start).First());
    }

    // ========================================================================
    // 3. Worst Fit (En Kötü Uygun): Süre olarak en geniş olan (kalan kapasiteyi en çok bırakan) boşluğu seçer.
    // ========================================================================
    public static RobotSelectionResult SelectWorstFit(RobotSelectionResult inputResult)
    {
        // Pencere uzunluğu (End - Start) en büyük olanı getir
        return ApplySelectionRule(inputResult, windows => windows.OrderByDescending(w => w.End - w.Start).First());
    }

    // ========================================================================
    // 4. Random Select (Rastgele Seçim): Pencereler arasından rastgele bir boşluk seçer.
    // ========================================================================
    private static readonly Random _rnd = new Random();
    public static RobotSelectionResult RandomSelect(RobotSelectionResult inputResult)
    {
        // Gelen pencereler arasından rastgele bir indeks seçerek döndür
        return ApplySelectionRule(inputResult, windows => windows[_rnd.Next(windows.Count)]);
    }

    // ========================================================================
    // Ortak Seçim Motoru (Yeni metotlar eklemeyi kolaylaştıran delege yapısı)
    // ========================================================================
    private static RobotSelectionResult ApplySelectionRule(RobotSelectionResult inputResult, Func<List<IdleWindow>, IdleWindow> selectionRule)
    {
        // Gelen veri boş veya uygunsuz ise aynı nesneyi geri döndür
        if (inputResult == null || inputResult.FeasibilityResult == null || !inputResult.FeasibilityResult.IsFeasible)
        {
            return inputResult;
        }

        // Orijinal veriyi bozmamak için yeni bir sonuç nesnesi oluşturuluyor
        var finalFeasibilityResult = new ProcessFeasibilityResult
        {
            IsFeasible = true
        };

        foreach (var instanceWindow in inputResult.FeasibilityResult.InstanceWindows)
        {
            // İlgili instance için kuralı çalıştır ve tek bir boşluk seç
            var selectedWindow = selectionRule(instanceWindow.AvailableWindows);

            // Seçilen bu tek boşluğu listeye ekle
            finalFeasibilityResult.InstanceWindows.Add(new InstanceWindowResult
            {
                ID_Process_Instance = instanceWindow.ID_Process_Instance,
                AvailableWindows = new List<IdleWindow> { selectedWindow } // Sadece seçilen pencereyi içerir
            });
        }

        // Seçilen aynı robot ve yeni oluşturulan filtrelenmiş zaman planı ile sonucu dön
        return new RobotSelectionResult(inputResult.SelectedRobot, finalFeasibilityResult);
    }
}