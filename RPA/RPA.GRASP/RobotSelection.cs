using System;
using System.Collections.Generic;

// Seçim işlemi sonucunda hem robotu hem de yerleşebileceği boşlukları taşıyacak sınıf
public class RobotSelectionResult
{
    public Robot SelectedRobot { get; set; }
    public ProcessFeasibilityResult FeasibilityResult { get; set; }

    public RobotSelectionResult(Robot robot, ProcessFeasibilityResult feasibilityResult)
    {
        SelectedRobot = robot;
        FeasibilityResult = feasibilityResult;
    }
}

public class RobotSelection
{
    // ========================================================================
    // 1. First Fit (İlk Uygun) Seçim Yöntemi
    // ========================================================================
    public static RobotSelectionResult SelectFirstFit(TaskProcess candidateTaskProcess, List<Robot> candidateRobots, List<TaskProcess> allTaskProcesses)
    {
        foreach (Robot candidateRobot in candidateRobots)
        {
            // İlgili robot için uygunluk kontrolü yapılıyor
            ProcessFeasibilityResult sonuc = WindowFilter.CheckRobotFeasibilityForProcess(candidateTaskProcess, candidateRobot, allTaskProcesses);

            // Eğer uygunluk sağlandıysa, bulduğumuz ilk robotu ve sonuçlarını döndürüp döngüden (ve metottan) çıkıyoruz
            if (sonuc.IsFeasible)
            {
                return new RobotSelectionResult(candidateRobot, sonuc);
            }
        }

        // Aday listedeki hiçbir robot uygun değilse null döner
        return null;
    }
    // ========================================================================
    // 1. GLOBAL BEST-FIT (Küresel En İyi Uygunluk) Yöntemi
    // ========================================================================
    /// <summary>
    /// Tüm aday robotları tarar. İşi yerleştirdiğimizde geriye EN AZ boşluk bırakan 
    /// (Residual Slack'i en düşük olan) mükemmel eşleşmeyi bulur ve o robotu seçer.
    /// </summary>
    public static RobotSelectionResult SelectGlobalBestFit(TaskProcess candidateTaskProcess, List<Robot> candidateRobots, List<TaskProcess> allTaskProcesses)
    {
        RobotSelectionResult globalBestResult = null;
        double minResidualSpace = double.MaxValue; // En düşük artığı bulmak için maksimum değerle başlıyoruz

        foreach (Robot candidateRobot in candidateRobots)
        {
            // İlgili robot için uygunluk (fizibilite) kontrolü yapılıyor
            ProcessFeasibilityResult sonuc = WindowFilter.CheckRobotFeasibilityForProcess(candidateTaskProcess, candidateRobot, allTaskProcesses);

            if (sonuc.IsFeasible)
            {
                RobotSelectionResult tempResult = new RobotSelectionResult(candidateRobot, sonuc);

                // Bu robotta işi yerleştirebileceğimiz "en dar/en uygun" pencerenin bırakacağı boşluğu hesaplıyoruz.
                // (Not: Yardımcı metoda gidip kendi nesne özelliklerinizle bağlayınız)
                double currentResidualSpace = CalculateResidualSpace(tempResult, candidateTaskProcess);

                // Eğer bu robotun sunduğu boşluk, şu ana kadar bulduğumuz en iyi boşluktan daha az artık bırakıyorsa:
                if (currentResidualSpace < minResidualSpace)
                {
                    minResidualSpace = currentResidualSpace;
                    globalBestResult = tempResult; // Yeni kralımız bu robot
                }
            }
        }

        // Tüm robotlar tarandıktan sonra, filodaki en mükemmel eşleşmeyi döndürür.
        return globalBestResult;
    }


    // ========================================================================
    // 3. MINIMUM FRAGMENTATION PENALTY (Minimum Parçalanma Cezası) Yöntemi
    // ========================================================================
    /// <summary>
    /// İş yerleştirildikten sonra geriye kalan "kullanılamaz kadar küçük" çöp boşlukları cezalandırır.
    /// Filonun zaman çizelgesini en az parçalayan (bütünlüğü koruyan) robotu seçer.
    /// </summary>
    public static RobotSelectionResult SelectMinimumFragmentation(TaskProcess candidateTaskProcess, List<Robot> candidateRobots, List<TaskProcess> allTaskProcesses, double unusableGapThreshold = 10.0)
    {
        RobotSelectionResult bestRobotResult = null;
        double minPenalty = double.MaxValue; // En düşük cezayı arıyoruz

        foreach (Robot candidateRobot in candidateRobots)
        {
            ProcessFeasibilityResult sonuc = WindowFilter.CheckRobotFeasibilityForProcess(candidateTaskProcess, candidateRobot, allTaskProcesses);

            if (sonuc.IsFeasible)
            {
                RobotSelectionResult tempResult = new RobotSelectionResult(candidateRobot, sonuc);

                // Bu robotu seçersek filoda yaratacağımız parçalanma cezasını hesaplıyoruz
                double currentPenalty = CalculateFragmentationPenalty(tempResult, candidateTaskProcess, unusableGapThreshold);

                if (currentPenalty < minPenalty)
                {
                    minPenalty = currentPenalty;
                    bestRobotResult = tempResult;
                }
            }
        }

        return bestRobotResult;
    }


    // ========================================================================
    // YARDIMCI (HELPER) METOTLAR
    // ========================================================================

    /// <summary>
    /// Global Best-Fit için: Robotun içindeki uygun pencerelerden, 
    /// iş yerleştirildikten sonra kalacak en küçük boşluk süresini (Residual Slack) hesaplar.
    /// </summary>
    private static double CalculateResidualSpace(RobotSelectionResult result, TaskProcess task)
    {
        // DİKKAT: ProcessFeasibilityResult nesnenizin içindeki zaman aralıklarını tutan listeye göre burayı uyarlayınız.
        // Örnek varsayım: result.FeasibilityResult.FeasibleWindows listesi olsun.

        double minimumSlack = double.MaxValue;

        /* 
        // Kendi yapınıza göre burayı açın:
        foreach(var window in result.FeasibilityResult.FeasibleWindows)
        {
            // Pencere süresi eksi İşlem süresi
            double slack = (window.End - window.Start) - task.ProcessingTime; 
            if(slack >= 0 && slack < minimumSlack)
            {
                minimumSlack = slack;
            }
        }
        */

        // Şimdilik hata vermemesi için sahte (dummy) bir değer dönüyoruz
        return minimumSlack == double.MaxValue ? 0 : minimumSlack;
    }

    /// <summary>
    /// Minimum Fragmentation için: Kalan boşluk unusableGapThreshold (Örn: 10 dk) değerinden küçükse 
    /// bu "çöp" bir boşluktur ve algoritmaya ceza puanı olarak yansır.
    /// </summary>
    private static double CalculateFragmentationPenalty(RobotSelectionResult result, TaskProcess task, double unusableGapThreshold)
    {
        double penalty = 0;

        /*
        // Kendi yapınıza göre burayı açın:
        // Varsayım: İşi 'en iyi' pencereye sol dayalı (Left-Aligned) koyduğumuzu varsayarak kalacak boşluğu hesaplıyoruz.
        var bestWindow = ... // (Burada mevcut WindowSelection.SelectBestFit metodunuzu çağırıp en iyi pencereyi alabilirsiniz)
        
        double remainingGap = (bestWindow.End - bestWindow.Start) - task.ProcessingTime;

        // Eğer kalan boşluk, başka bir işin sığamayacağı kadar küçükse (Örn: 5 dk) bunu ceza olarak ekle
        if(remainingGap > 0 && remainingGap < unusableGapThreshold)
        {
            // Kalan boşluk ne kadar küçükse ceza o kadar büyüktür. Veya direkt kalan dakika ceza olarak yazılır.
            penalty += (unusableGapThreshold - remainingGap); 
        }
        */

        return penalty;
    }
}