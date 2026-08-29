using System;
using System.Collections.Generic;
using System.Linq;

public class DestructionOperators
{
    private static readonly Random _rnd = new Random();

    /// <summary>
    /// Faz-1: Rastgele bir departman seçer (Dept > 0) ve o departmana tahsisli 
    /// tüm robotlardaki işleri sökerek havuza (processesToRelocate) alır.
    /// İçi boşaltılan robotları da 'emptiedRobots' olarak dışarı aktarır.
    /// </summary>
    public static List<TaskProcess> Phase1_DepartmentRemoval(
        List<Robot> currentRobotList,
        List<TaskProcess> lstProcess,
        out List<Robot> emptiedRobots)
    {
        List<TaskProcess> processesToRelocate = new List<TaskProcess>();
        emptiedRobots = new List<Robot>();

        // 1. Üzerinde iş olan ve Departmanı > 0 olan aktif robotları tespit et
        var deptRobots = currentRobotList
            .Where(r => r.IIR != null && r.IIR.Any() && r.AllocatedDepartment != "0")
            .ToList();

        if (!deptRobots.Any())
        {
            return processesToRelocate; // Sökülecek departmanlı iş kalmamış (Faz-1 pas geçilebilir)
        }

        // 2. Rastgele bir departman seç (Örn: Departman 2)
        string targetDept = deptRobots[_rnd.Next(deptRobots.Count)].AllocatedDepartment;

        // 3. Bu departmana tahsisli tüm robotları bul
        var targetRobots = deptRobots.Where(r => r.AllocatedDepartment == targetDept).ToList();

        // 4. Robotlardaki işleri sök ve robotları temizle
        foreach (var robot in targetRobots)
        {
            var uniqueProcessIds = robot.IIR.Select(i => i.ID_Process).Distinct().ToList();
            var processesOnRobot = lstProcess.Where(p => uniqueProcessIds.Contains(p.ProcessID)).ToList();

            processesToRelocate.AddRange(processesOnRobot);

            // Robotun içini tamamen boşalt
            robot.IIR.Clear();
            robot.LstIdleWindow.Clear(); // Onarımda baştan hesaplanacak

            emptiedRobots.Add(robot);
        }

        return processesToRelocate;
    }

    /// <summary>
    /// Faz-2: Evrensel (Dept = 0) robotlardan En Boş 2 + Rastgele 1 robotu seçerek söker.
    /// </summary>
    public static List<TaskProcess> Phase2_UniversalRemoval(
        List<Robot> currentRobotList,
        List<TaskProcess> lstProcess,
        out List<Robot> emptiedRobots)
    {
        List<TaskProcess> processesToRelocate = new List<TaskProcess>();
        emptiedRobots = new List<Robot>();

        // 1. Evrensel olan ve üzerinde iş bulunan aktif robotları listele
        var universalRobots = currentRobotList
            .Where(r => r.AllocatedDepartment == "0" && r.IIR != null && r.IIR.Any())
            .ToList();

        // Eğer sistemde 3'ten az evrensel robot varsa, hepsini seç
        int targetCount = Math.Min(3, universalRobots.Count);
        if (targetCount == 0) return processesToRelocate;

        // 2. Robotları üzerindeki iş sayısına (yüke) göre azdan çoğa sırala
        var sortedByLoad = universalRobots.OrderBy(r => r.IIR.Count).ToList();

        // 3. En az yüklü 2 robotu havuza al
        emptiedRobots.AddRange(sortedByLoad.Take(2));

        // 4. Kalanlardan rastgele 1 tane seç (çeşitlilik için)
        if (targetCount == 3)
        {
            var remainingForRandom = sortedByLoad.Skip(2).ToList();
            if (remainingForRandom.Any())
            {
                emptiedRobots.Add(remainingForRandom[_rnd.Next(remainingForRandom.Count)]);
            }
        }

        // 5. Seçilen robotların işlerini sök ve içlerini boşalt
        foreach (var robot in emptiedRobots)
        {
            var uniqueProcessIds = robot.IIR.Select(i => i.ID_Process).Distinct().ToList();
            var processesOnRobot = lstProcess.Where(p => uniqueProcessIds.Contains(p.ProcessID)).ToList();

            processesToRelocate.AddRange(processesOnRobot);

            robot.IIR.Clear();
            robot.LstIdleWindow.Clear();
        }

        return processesToRelocate.Distinct().ToList(); // Güvenlik tekilleştirmesi
    }
}

