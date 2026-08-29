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
    // 2. İleride Eklenebilecek Diğer Seçim Yöntemleri İçin Taslaklar
    // ========================================================================

    /*
    public static RobotSelectionResult SelectBestFit(TaskProcess candidateTaskProcess, List<Robot> candidateRobots)
    {
        // En az boşluk bırakan veya idle pencereyi en iyi dolduran robotu seçme mantığı buraya yazılır.
        throw new NotImplementedException();
    }
    
    public static RobotSelectionResult SelectRandomFit(TaskProcess candidateTaskProcess, List<Robot> candidateRobots)
    {
        // Uygun olan robotlar arasından rastgele birini seçme mantığı buraya yazılır.
        throw new NotImplementedException();
    }
    */
}