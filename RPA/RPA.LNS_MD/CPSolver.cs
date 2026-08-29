using System;
using System.Collections.Generic;
using System.Linq;
using Google.OrTools.Sat;

public class CPSolver
{
    /// <summary>
    /// Hem sökülen işler (Tam Esnek) hem de kilitli işler (Sadece Zaman Esnek) için
    /// Zero Tardiness (Sıfır Gecikme) ve No-Overlap (Çakışmazlık) kısıtlarıyla çözücüyü çalıştırır.
    /// </summary>
    public static bool SolveWithPartialFlexibility(
        List<TaskProcess> freeProcesses,
        List<(TaskProcess Process, Robot LockedRobot)> fixedProcesses,
        List<Robot> availableRobots)
    {
        CpModel model = new CpModel();

        // Çözüm bulunduğunda değerleri okuyabilmek için değişkenleri hafızada tutacağımız sözlükler
        Dictionary<Instance, IntVar> startVars = new Dictionary<Instance, IntVar>();
        Dictionary<Instance, IntVar> endVars = new Dictionary<Instance, IntVar>();
        Dictionary<TaskProcess, Dictionary<Robot, BoolVar>> assignVars = new Dictionary<TaskProcess, Dictionary<Robot, BoolVar>>();

        // Çakışmazlık kısıtlarını gruplayacağımız listeler
        Dictionary<int, List<IntervalVar>> robotIntervals = new Dictionary<int, List<IntervalVar>>(); // RobotID -> Intervals
        Dictionary<int, List<IntervalVar>> accountIntervals = new Dictionary<int, List<IntervalVar>>(); // Account -> Intervals

        // --- 1. SÖKÜLMÜŞ (SERBEST) İŞLERİN MODELLENMESİ (Atama + Sıralama Esnek) ---
        foreach (var p in freeProcesses)
        {
            assignVars[p] = new Dictionary<Robot, BoolVar>();
            List<BoolVar> processRobotBools = new List<BoolVar>();

            // KISIT 1: TaskProcess'e ait tüm Instance'lar AYNI robota gitmeli.
            // Bu yüzden atama değişkenini Instance bazında değil, TaskProcess bazında oluşturuyoruz.
            foreach (var r in availableRobots)
            {
                BoolVar assignRobot = model.NewBoolVar($"assign_p{p.ProcessID}_r{r.RobotID}");
                assignVars[p][r] = assignRobot;
                processRobotBools.Add(assignRobot);
            }
            // Sadece ve sadece 1 robota atanabilir
            model.AddExactlyOne(processRobotBools);

            foreach (var inst in p.InstancesOfProcess)
            {
                // 1440 Sınırını Kısıta Dahil Et (DueTime ile 1440'ı yarıştır, küçük olanı sınır kabul et)
                int maxAllowedTime = Math.Min(inst.DueTime, 1440);

                // Infeasible (İmkansız) durum kontrolü (Süre yetmiyorsa modeli patlatmamak için)
                if (inst.ReleaseTime + inst.ProcessingTime > maxAllowedTime) return false;

                IntVar start = model.NewIntVar(inst.ReleaseTime, maxAllowedTime - inst.ProcessingTime, $"start_{inst.ID_Process_Instance}");
                IntVar end = model.NewIntVar(inst.ReleaseTime + inst.ProcessingTime, maxAllowedTime, $"end_{inst.ID_Process_Instance}");

                // Ana Interval (Hesaplar (Account) için kullanılacak Global Interval)
                IntervalVar globalInterval = model.NewIntervalVar(start, inst.ProcessingTime, end, $"global_interval_{inst.ID_Process_Instance}");

                startVars[inst] = start;
                endVars[inst] = end;

                // Account gruplamasına ekle
                if (p.Account > 0)
                {
                    if (!accountIntervals.ContainsKey(p.Account)) accountIntervals[p.Account] = new List<IntervalVar>();
                    accountIntervals[p.Account].Add(globalInterval);
                }

                // Robot alternatifleri için "Optional Interval" (Sadece atandığı robotta var olacak)
                foreach (var r in availableRobots)
                {
                    IntervalVar optInterval = model.NewOptionalIntervalVar(
                        start, inst.ProcessingTime, end, assignVars[p][r], $"opt_interval_{inst.ID_Process_Instance}_r{r.RobotID}");

                    if (!robotIntervals.ContainsKey(r.RobotID)) robotIntervals[r.RobotID] = new List<IntervalVar>();
                    robotIntervals[r.RobotID].Add(optInterval);
                }
            }
        }

        // --- 2. KİLİTLİ MEVCUT İŞLERİN MODELLENMESİ (Sadece Sıralama Esnek) ---
        foreach (var fixedItem in fixedProcesses)
        {
            var p = fixedItem.Process;
            var lockedRobot = fixedItem.LockedRobot;

            foreach (var inst in p.InstancesOfProcess)
            {
                // 1440 Sınırını Kısıta Dahil Et
                int maxAllowedTime = Math.Min(inst.DueTime, 1440);

                if (inst.ReleaseTime + inst.ProcessingTime > maxAllowedTime) return false;

                IntVar start = model.NewIntVar(inst.ReleaseTime, maxAllowedTime - inst.ProcessingTime, $"fixed_start_{inst.ID_Process_Instance}");
                IntVar end = model.NewIntVar(inst.ReleaseTime + inst.ProcessingTime, maxAllowedTime, $"fixed_end_{inst.ID_Process_Instance}");

                IntervalVar interval = model.NewIntervalVar(start, inst.ProcessingTime, end, $"fixed_interval_{inst.ID_Process_Instance}");

                startVars[inst] = start;
                endVars[inst] = end;

                // Kilitli işler kendi robotunun aralık listesine DİREKT (zorunlu olarak) eklenir
                if (!robotIntervals.ContainsKey(lockedRobot.RobotID)) robotIntervals[lockedRobot.RobotID] = new List<IntervalVar>();
                robotIntervals[lockedRobot.RobotID].Add(interval);

                // Account grubuna ekle
                if (p.Account > 0)
                {
                    if (!accountIntervals.ContainsKey(p.Account)) accountIntervals[p.Account] = new List<IntervalVar>();
                    accountIntervals[p.Account].Add(interval);
                }
            }
        }

        // --- KISIT 2: Aynı robottaki Instance'lar çakışamaz ---
        foreach (var rIntervals in robotIntervals.Values)
        {
            if (rIntervals.Count > 1) model.AddNoOverlap(rIntervals);
        }

        // --- KISIT 3: Farklı robotta dahi olsa aynı Account çakışamaz ---
        foreach (var aIntervals in accountIntervals.Values)
        {
            if (aIntervals.Count > 1) model.AddNoOverlap(aIntervals);
        }

        // --- ÇÖZÜCÜYÜ ÇALIŞTIR ---
        CpSolver solver = new CpSolver();
        // Arama uzayını daralttığımız için çözücü çok hızlı çalışacaktır. 5 saniye limit iyidir.
        solver.StringParameters = "max_time_in_seconds:5.0; num_search_workers:4;";

        CpSolverStatus status = solver.Solve(model);

        // --- SONUÇLARI UYGULA ---
        if (status == CpSolverStatus.Optimal || status == CpSolverStatus.Feasible)
        {
            // 1. Serbest işleri atandıkları robotlara ekle
            foreach (var p in freeProcesses)
            {
                Robot assignedRobot = null;
                foreach (var r in availableRobots)
                {
                    if (solver.Value(assignVars[p][r]) == 1) // Çözücü bu robotu seçmiş
                    {
                        assignedRobot = r;
                        break;
                    }
                }

                if (assignedRobot != null)
                {
                    foreach (var inst in p.InstancesOfProcess)
                    {
                        // Sökülen işi yeni robotunun IIR listesine fiziksel olarak geri ekliyoruz
                        assignedRobot.IIR.Add(inst);
                        inst.RobotNumber = assignedRobot.RobotID;
                    }
                }
            }

            // 2. Tüm işlerin (Hem serbest hem kilitlilerin) yeni zamanlarını güncelle
            foreach (var inst in startVars.Keys)
            {
                inst.StartTime = solver.Value(startVars[inst]);
                inst.FinishTime = solver.Value(endVars[inst]);
                inst.Tardiness = 0; // Model zaten dueTime içinde kalmaya zorlandı
            }

            return true;
        }

        return false; // Sığmadı veya Account kısıtına takıldı
    }
}