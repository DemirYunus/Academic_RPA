using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

// İterasyon sonuçlarını tutacak veri modeli
public class IterationResult
{
    public int Iteration { get; set; }
    public double Cost { get; set; }
}

public class SolutionEvaluator
{
    // Belirli bir iterasyonun sonunda oluşan robot listesinin toplam maliyetini hesaplar
    // 1. DEĞİŞİKLİK: Metoda lstProcess parametresi eklendi
    public static double CalculateCost(List<Robot> robotList, List<TaskProcess> lstProcess)
    {
        if (robotList == null || !robotList.Any()) return 0;

        // 2. DEĞİŞİKLİK: Maliyeti hesaplamadan önce yazılımları GÜNCELLE!
        UpdateRobotSoftwares(robotList, lstProcess);

        // 3. DEĞİŞİKLİK: Sadece içine iş (Instance) atanmış aktif robotları filtrele
        var activeRobots = robotList.Where(r => r.IIR != null && r.IIR.Count > 0).ToList();

        // Sadece aktif robotlar için 3000 birim maliyet
        double totalCost = activeRobots.Count * 3000;

        // Robotlardaki yüklü yazılımların maliyetlerini ekle
        foreach (var robot in activeRobots) // Sadece aktif robotların yazılımlarını topla
        {
            if (robot.LoadedSoftware != null)
            {
                foreach (var sw in robot.LoadedSoftware)
                {
                    string name = sw.Name?.Trim().ToLower();

                    if (name == "sw1") totalCost += 100;
                    else if (name == "sw2") totalCost += 150;
                    else if (name == "sw3") totalCost += 200;
                }
            }
        }

        return totalCost;
    }

    // 2. YARDIMCI METOT (RepairOperators sınıfından buraya taşındı ve "public" yapıldı)
    public static void UpdateRobotSoftwares(List<Robot> robots, List<TaskProcess> lstProcess)
    {
        foreach (var robot in robots)
        {
            if (robot.IIR != null && robot.IIR.Count > 0)
            {
                var robotProcessIds = robot.IIR.Select(i => i.ID_Process).Distinct().ToList();

                var requiredSoftwares = lstProcess
                    .Where(p => robotProcessIds.Contains(p.ProcessID) && p.RequiredSoftwares != null)
                    .SelectMany(p => p.RequiredSoftwares)
                    .GroupBy(s => s.Name)
                    .Select(g => g.First())
                    .ToList();

                robot.LoadedSoftware = requiredSoftwares;
            }
            else
            {
                robot.LoadedSoftware = new List<Software>();
            }
        }
    }
}

