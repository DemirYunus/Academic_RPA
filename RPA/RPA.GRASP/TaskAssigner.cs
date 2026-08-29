using System;
using System.Collections.Generic;
using System.Linq;

// Atama yerleşim tipini belirten numaralandırma
public enum AlignmentStrategy
{
    LeftAligned,
    CenterAligned,
    RightAligned
}

public class TaskAssigner
{    private static readonly Random _rnd = new Random();

    // Atama ve veri güncelleme metodumuz (Referans tipli nesneler üzerinde çalıştığı için geriye değer dönmesine gerek yoktur - void)
    public static void AssignAndUpdate(TaskProcess taskProcess, RobotSelectionResult selectionResult, AlignmentStrategy alignment)
    {
        if (taskProcess == null || selectionResult == null || selectionResult.SelectedRobot == null) return;

        Robot robot = selectionResult.SelectedRobot;
        ProcessFeasibilityResult plan = selectionResult.FeasibilityResult;

        if (plan == null || !plan.IsFeasible) return;

        // 1. Robot bilgilerini güncelleme (Bölüm ve Yazılımlar)
        // Bölüm ataması (Daha önce atanmışsa bile aynı olacağı için üzerine yazmak sorun yaratmaz)
        robot.AllocatedDepartment = taskProcess.Department.ToString();

        // Gerekli yazılımları robota ekleme (Eğer yoksa)
        if (taskProcess.RequiredSoftwares != null)
        {
            if (robot.LoadedSoftware == null) robot.LoadedSoftware = new List<Software>();

            foreach (var reqSw in taskProcess.RequiredSoftwares)
            {
                if (!robot.LoadedSoftware.Any(ls => string.Equals(ls.Name, reqSw.Name, StringComparison.OrdinalIgnoreCase)))
                {
                    robot.LoadedSoftware.Add(new Software { Name = reqSw.Name });
                }
            }
        }

        // 2. Instance Atamaları ve Zaman Çizelgeleme
        foreach (var instancePlan in plan.InstanceWindows)
        {
            // Planlanan instance'ı TaskProcess içinden bul
            var instance = taskProcess.InstancesOfProcess.FirstOrDefault(i => i.ID_Process_Instance == instancePlan.ID_Process_Instance);
            if (instance == null || !instancePlan.AvailableWindows.Any()) continue;

            // WindowSelection sınıfıyla 1'e düşürülmüş pencereyi al
            IdleWindow targetWindow = instancePlan.AvailableWindows.First();
            int pTime = instance.ProcessingTime;

            // Stratejiye göre başlangıç ve bitiş zamanlarını belirle
            int actualStart = 0;
            int actualEnd = 0;

            switch (alignment)
            {
                case AlignmentStrategy.LeftAligned: // Sola Dayalı
                    actualStart = targetWindow.Start;
                    actualEnd = actualStart + pTime;
                    break;

                case AlignmentStrategy.RightAligned: // Sağa Dayalı
                    actualEnd = targetWindow.End;
                    actualStart = actualEnd - pTime;
                    break;

                case AlignmentStrategy.CenterAligned: // Ortala
                    int margin = (targetWindow.End - targetWindow.Start - pTime) / 2;
                    actualStart = targetWindow.Start + margin;
                    actualEnd = actualStart + pTime;
                    break;
            }

            // Instance'ı güncelle
            instance.StartTime = actualStart;
            instance.FinishTime = actualEnd;

            // Robot numarası ataması. Eğer RobotID veya isimde numara yoksa hash veya ID parse işlemi yapılabilir.
            // Örnek olarak RobotID attribute'u varsa o atanır, yoksa isim içinden sayısal değer alınabilir.
            instance.RobotNumber = ExtractRobotNumber(robot.RobotName);

            // Robotun IIR listesine ekle
            if (robot.IIR == null) robot.IIR = new List<Instance>();
            robot.IIR.Add(instance);

            // 3. Robotun Boş Zaman Pencerelerini (Idle Windows) Güncelle
            robot.LstIdleWindow = RPA.GRASP.IdleWindowUpdater.UpdateIdleTimes(robot.LstIdleWindow, actualStart, actualEnd);
        }
    }

    // Robot adından numara çıkartan basit bir yardımcı metot (Örn: "R1" -> 1, "Robot25" -> 25)
    private static int ExtractRobotNumber(string robotName)
    {
        if (string.IsNullOrWhiteSpace(robotName)) return 0;
        string numStr = new string(robotName.Where(char.IsDigit).ToArray());
        return int.TryParse(numStr, out int num) ? num : 0;
    }

    /// <summary>
    /// AlignmentStrategy enum'u içindeki hizalama seçeneklerinden birini eşit olasılıkla rastgele seçer.
    /// </summary>
    public static AlignmentStrategy GetRandomAlignmentStrategy()
    {
        // Enum içindeki tüm değerleri bir diziye al
        Array values = Enum.GetValues(typeof(AlignmentStrategy));

        // Rastgele bir indeks üret ve o indeksteki stratejiyi döndür
        return (AlignmentStrategy)values.GetValue(_rnd.Next(values.Length));
    }
}

