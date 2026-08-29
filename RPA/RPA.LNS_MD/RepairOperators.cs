using System;
using System.Collections.Generic;
using System.Linq;

public class RepairOperators
{
    /// <summary>
    /// Faz-1: Sökülen departman işlerini, kendi boşaltılan robotlarına KADEMELİ olarak geri yerleştirir.
    /// Account çakışmalarını önlemek için diğer robotlardaki bağlı işleri "Zamanı Esnek, Robotu Sabit" olarak CP'ye bildirir.
    /// </summary>
    public static bool Phase1_DepartmentRepair(
        List<TaskProcess> processesToRelocate,
        List<Robot> emptiedRobots,
        List<Robot> allActiveRobots, // Havuza dahil olmayan diğer (dokunulmamış) robotlar
        List<TaskProcess> lstProcess)
    {
        if (!processesToRelocate.Any() || !emptiedRobots.Any()) return true;

        // --- ACCOUNT RADARI (Bağlı İşleri Bul) ---
        var targetAccounts = processesToRelocate.Where(p => p.Account > 0).Select(p => p.Account).Distinct().ToList();

        // Ataması DEĞİŞMEYECEK ama sırası (zamanı) KAYDIRILABİLECEK işlerin listesi
        List<(TaskProcess Process, Robot LockedRobot)> fixedProcessesWithSliding = new List<(TaskProcess, Robot)>();


        // ---ACCOUNT RADARI(Çapraz Kısıtları Yönetme Aşaması)-- -
        // Sökülen işlerin Account numaralarıyla çakışma ihtimali olan, 
        // DİĞER (söküm yapılmamış, dokunulmamış) robotlardaki mevcut işleri tespit ediyoruz.

        var remainingActiveRobots = allActiveRobots.Except(emptiedRobots).ToList();

        foreach (var robot in remainingActiveRobots)
        {
            if (robot.IIR != null && robot.IIR.Any())
            {
                var robotProcessIds = robot.IIR.Select(i => i.ID_Process).Distinct().ToList();
                var matchingProcesses = lstProcess
                    .Where(p => robotProcessIds.Contains(p.ProcessID) && targetAccounts.Contains(p.Account))
                    .ToList();

                foreach (var p in matchingProcesses)
                {
                    fixedProcessesWithSliding.Add((p, robot));
                }
            }
        }

        // --- KADEMELİ SIKIŞTIRMA (Önce 1 robot, sonra 2, sonra 3...) ---
        List<Robot> activeCapacityForCP = new List<Robot>();
        bool isSolutionFound = false;
        int maxCapacity = emptiedRobots.Count;

        for (int i = 0; i < maxCapacity; i++)
        {
            activeCapacityForCP.Add(emptiedRobots[i]); // Kademe kademe robot ekle

            // CP Çözücüyü Çağır (Atama esnek, Kilitliler sadece zaman esnek)
            isSolutionFound = CPSolver.SolveWithPartialFlexibility(
                processesToRelocate,
                fixedProcessesWithSliding,
                activeCapacityForCP
            );

            if (isSolutionFound)
            {
                // Kullanılmayan boş robotları tespit et ve ana listeden sil!
                var unusedRobots = emptiedRobots.Except(activeCapacityForCP).ToList();
                foreach (var unused in unusedRobots)
                {
                    allActiveRobots.Remove(unused);
                }

                // BAŞARILI! Sökülen örneğin 3 robotluk işi, belki 2 robota sığdırdık.
                // Zaman pencerelerini güncelle
                foreach (var robot in activeCapacityForCP)
                {
                    IdleWindowUpdater.UpdateRobotWindows(robot, 1440);
                }

                // Zamanı kaydırılan diğer (dokunulmamış) robotların da pencerelerini güncelle
                var slidingRobots = fixedProcessesWithSliding.Select(f => f.LockedRobot).Distinct().ToList();
                foreach (var robot in slidingRobots)
                {
                    IdleWindowUpdater.UpdateRobotWindows(robot, 1440);
                }

                // DÜZELTME NOKTASI 1: YAZILIMLARI GÜNCELLE!
                UpdateRobotSoftwares(allActiveRobots, lstProcess);

                return true;
            }
        }

        // For döngüsü bitti ve sığmadıysa çözüm bulunamadı demektir (En kötü ihtimal)
        return false;
    }

    /// <summary>
    /// Faz-2: Sökülen evrensel işleri 7 robotluk bir fanusa yerleştirmeyi dener.
    /// Sığmazsa fanusu akıllıca (boşluklara göre) büyüterek CP'yi tekrar çağırır.
    /// </summary>
    public static bool Phase2_UniversalRepair(
        List<TaskProcess> processesToRelocate,
        List<Robot> emptiedRobots,
        List<Robot> allActiveRobots,
        List<TaskProcess> lstProcess,
        int initialSubsetSize)
    {
        if (!processesToRelocate.Any()) return true;

        // 1. Dokunulmayan evrensel (Dept = 0) robotları tespit et
        var remainingUniversal = allActiveRobots
            .Where(r => r.AllocatedDepartment == "0")
            .Except(emptiedRobots)
            .ToList();

        // 2. Başlangıç Fanusunu (Alt Küme) oluştur (Maksimum initialSubsetSize robot)
        int subsetSize = Math.Min(initialSubsetSize, remainingUniversal.Count);

        // Rastgele initialSubsetSize robotu "Atama Yapılabilir" fanusuna al
        List<Robot> assignmentCapacity = remainingUniversal.OrderBy(x => Guid.NewGuid()).Take(subsetSize).ToList();

        // 3. Fanus dışında kalan yedek kuvvetleri belirle (Genişleme için). Arttırımlı olarak kullanılacak.
        List<Robot> outsideRobots = remainingUniversal.Except(assignmentCapacity).ToList();

        // AKILLI SEÇİM: Dışarıdakileri "Toplam Boşluk Süresine" göre ÇOKTAN AZA sırala
        outsideRobots = outsideRobots.OrderByDescending(r =>
            r.LstIdleWindow.Sum(w => w.End - w.Start)).ToList();

        // Yıktığımız orijinal 3 robotu da "En son çare" (Fallback) yedeklerine ekle
        List<Robot> fallbackEmptyRobots = new List<Robot>(emptiedRobots);

        // Radar için aranan account numaraları
        var targetAccounts = processesToRelocate.Where(p => p.Account > 0).Select(p => p.Account).Distinct().ToList();

        bool isSolutionFound = false;

        // --- KADEMELİ ONARIM DÖNGÜSÜ ---
        while (!isSolutionFound)
        {
            // KISIT HAZIRLIĞI (Her kapasite artışında güncellenir)
            List<(TaskProcess Process, Robot LockedRobot)> fixedProcessesWithSliding = new List<(TaskProcess, Robot)>();

            // A) Fanustaki robotların ÜZERİNDEKİ TÜM İŞLER: Sadece zamanı kaydırılabilir (Sabit Robot)
            foreach (var robot in assignmentCapacity)
            {
                if (robot.IIR != null && robot.IIR.Any())
                {
                    var processIds = robot.IIR.Select(i => i.ID_Process).Distinct();
                    var procs = lstProcess.Where(p => processIds.Contains(p.ProcessID));
                    foreach (var p in procs) fixedProcessesWithSliding.Add((p, robot));
                }
            }

            // B) Dışarıdaki robotların SADECE ACCOUNT ÇAKIŞAN İŞLERİ: Sadece zamanı kaydırılabilir
            // DÜZELTME: Sadece evrensel yedeklere değil, departman robotları dahil fanus dışındaki TÜM robotlara bak!
            var allOutsideRobots = allActiveRobots.Except(assignmentCapacity).ToList();

            foreach (var robot in allOutsideRobots)
            {
                if (robot.IIR != null && robot.IIR.Any())
                {
                    var processIds = robot.IIR.Select(i => i.ID_Process).Distinct();
                    var matchingProcs = lstProcess.Where(p =>
                        processIds.Contains(p.ProcessID) && targetAccounts.Contains(p.Account));

                    foreach (var p in matchingProcs) fixedProcessesWithSliding.Add((p, robot));
                }
            }

            // --- CP ÇÖZÜCÜYÜ ÇAĞIR ---
            isSolutionFound = CPSolver.SolveWithPartialFlexibility(
                processesToRelocate,          // Atama + Sıralama Esnek
                fixedProcessesWithSliding,    // Sadece Sıralama Esnek (Robot Sabit)
                assignmentCapacity            // Havuzdaki işlerin atanabileceği Fanus robotları
            );

            if (isSolutionFound)
            {
                // Yıktığımız 3 robottan, geri çağırmaya gerek duymadıklarımızı (yedekte kalanları) ana listeden tamamen sil!
                foreach (var unused in fallbackEmptyRobots)
                {
                    allActiveRobots.Remove(unused);
                }

                // Başarılı! Zaman pencerelerini güncelle
                foreach (var r in assignmentCapacity.Concat(outsideRobots))
                    IdleWindowUpdater.UpdateRobotWindows(r, 1440);

                // DÜZELTME NOKTASI 2: YAZILIMLARI GÜNCELLE!
                UpdateRobotSoftwares(allActiveRobots, lstProcess);

                return true;
            }
            else
            {
                // ÇÖZÜMSÜZ. Fanusu Genişlet!
                if (outsideRobots.Any())
                {
                    // Kademe 1: En boş aktif robotu fanusa dahil et
                    var robotToAdd = outsideRobots.First();
                    assignmentCapacity.Add(robotToAdd);
                    outsideRobots.Remove(robotToAdd);
                }
                else if (fallbackEmptyRobots.Any())
                {
                    // Kademe 2: Aktifler bitti, yıktığımız 3 boş robottan ekle
                    var robotToAdd = fallbackEmptyRobots.First();
                    assignmentCapacity.Add(robotToAdd);
                    fallbackEmptyRobots.Remove(robotToAdd);
                }
                else
                {
                    // Kademe 3: Tüm kapasite (orijinal 15 robot) kullanıldı ama sığmadı.
                    return false;
                }
            }
        }
        return false;
    }

    // DÜZELTME NOKTASI 3: YAZILIM GÜNCELLEME METODU (Buraya eklendi)
    public static void UpdateRobotSoftwares(List<Robot> robots, List<TaskProcess> lstProcess)
    {
        foreach (var robot in robots)
        {
            if (robot.IIR != null && robot.IIR.Count > 0)
            {
                // 1. Adım: Robotun içindeki instance'ların ProcessID'lerini tekilleştirerek al
                var robotProcessIds = robot.IIR.Select(i => i.ID_Process).Distinct().ToList();

                // 2. Adım: lstProcess listesinden bu ID'lere ait orijinal TaskProcess nesnelerini bul
                // ve onların gerektirdiği yazılımları toplayıp tekilleştir
                var requiredSoftwares = lstProcess
                    .Where(p => robotProcessIds.Contains(p.ProcessID) && p.RequiredSoftwares != null)
                    .SelectMany(p => p.RequiredSoftwares)
                    .GroupBy(s => s.Name) // Aynı yazılımı (sw1, sw2 vb.) tekilleştir
                    .Select(g => g.First())
                    .ToList();

                robot.LoadedSoftware = requiredSoftwares;
            }
            else
            {
                // Robot boşsa yazılımları temizle
                robot.LoadedSoftware = new List<Software>();
            }
        }
    }
}