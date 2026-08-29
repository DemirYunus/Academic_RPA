using System;
using System.Collections.Generic;
using System.Linq;
using static System.Runtime.InteropServices.JavaScript.JSType;
using Google.OrTools.Sat;

public class CP
{


    /// <summary>
    /// Sökülen ve hedef robotun üzerindeki işlemlerin tamamını (havuzu) sıfır gecikme 
    /// ve yasaklı periyot kısıtlarına göre çizelgeler.
    /// </summary>
    /// <param name="mergedInstances">Çizelgelenecek tüm işlemler havuzu</param>
    /// <param name="forbiddenPeriodsByProcess">GetForbiddenPeriodsByAccount metodundan gelen yasaklı zamanlar</param>
    /// <returns>Eğer 0 gecikmeli uygun bir çözüm bulunursa true, bulunamazsa false döner.</returns>
    public static bool SolveZeroTardiness(
        List<Instance> mergedInstances,
        Dictionary<string, List<Tuple<int, int>>> forbiddenPeriodsByProcess)
    {
        if (mergedInstances == null || !mergedInstances.Any())
            return true; // Boş liste zaten çözülmüş sayılır

        CpModel model = new CpModel();

        // OR-Tools Değişkenlerini tutacağımız sözlükler
        Dictionary<string, IntVar> startVars = new Dictionary<string, IntVar>();
        Dictionary<string, IntVar> endVars = new Dictionary<string, IntVar>();
        List<IntervalVar> allIntervals = new List<IntervalVar>();

        // 1. HER BİR İŞLEM İÇİN DEĞİŞKENLERİ VE KISITLARI OLUŞTUR
        foreach (var inst in mergedInstances)
        {
            int p = inst.ProcessingTime;

            // 0 Gecikme Kısıtı: Başlangıç en erken ReleaseTime, Bitiş en geç DueTime olabilir.
            int minStart = inst.ReleaseTime;
            int maxEnd = inst.DueTime;

            // Güvenlik kontrolü: İşlem süresi, verilen zaman penceresinden büyükse çözüm zaten imkansızdır
            if (minStart + p > maxEnd)
            {
                return false;
            }

            // Değişkenleri tanımla
            IntVar start = model.NewIntVar(minStart, maxEnd - p, $"start_{inst.ID_Process_Instance}");
            IntVar end = model.NewIntVar(minStart + p, maxEnd, $"end_{inst.ID_Process_Instance}");
            IntervalVar interval = model.NewIntervalVar(start, p, end, $"interval_{inst.ID_Process_Instance}");

            startVars[inst.ID_Process_Instance] = start;
            endVars[inst.ID_Process_Instance] = end;
            allIntervals.Add(interval);

            // 2. YASAKLI PERİYOT (HESAP ÇAKIŞMASI) KISITLARINI EKLE
            if (forbiddenPeriodsByProcess.ContainsKey(inst.ID_Process))
            {
                var forbiddenPeriods = forbiddenPeriodsByProcess[inst.ID_Process];

                foreach (var fp in forbiddenPeriods)
                {
                    int fStart = fp.Item1;
                    int fEnd = fp.Item2;

                    // Mantık: İşlem, yasaklı periyottan ya TAMAMEN ÖNCE bitmeli ya da TAMAMEN SONRA başlamalıdır.
                    // Yani: (end <= fStart) VEYA (start >= fEnd)

                    BoolVar isBefore = model.NewBoolVar($"before_{inst.ID_Process_Instance}_{fStart}");
                    BoolVar isAfter = model.NewBoolVar($"after_{inst.ID_Process_Instance}_{fStart}");

                    // Eğer isBefore true ise, islem yasaklı periyottan önce bitmek zorundadır
                    model.Add(end <= fStart).OnlyEnforceIf(isBefore);

                    // Eğer isAfter true ise, islem yasaklı periyottan sonra başlamak zorundadır
                    model.Add(start >= fEnd).OnlyEnforceIf(isAfter);

                    // Bu iki durumdan EN AZ BİRİ geçerli olmak zorundadır
                    model.AddBoolOr(new ILiteral[] { isBefore, isAfter });
                }
            }
        }

        // 3. ROBOT İÇİ ÇAKIŞMA KISITI (KAPASİTE = 1)
        // Aynı robotun üzerindeki hiçbir işlem birbiriyle üst üste binemez
        model.AddNoOverlap(allIntervals);

        // 4. ÇÖZÜCÜYÜ ÇALIŞTIR
        CpSolver solver = new CpSolver();

        // İşlemciyi sonsuz döngüden korumak için zaman sınırı eklemek best-practice'dir. (Örn: 10 saniye)
        solver.StringParameters = "max_time_in_seconds:10.0;";

        CpSolverStatus status = solver.Solve(model);

        // 5. SONUÇLARI KONTROL ET VE NESNELERE YAZ
        if (status == CpSolverStatus.Optimal || status == CpSolverStatus.Feasible)
        {
            // Çözüm bulundu! Değerleri nesnelere atayalım
            foreach (var inst in mergedInstances)
            {
                inst.StartTime = solver.Value(startVars[inst.ID_Process_Instance]);
                inst.FinishTime = solver.Value(endVars[inst.ID_Process_Instance]);
                inst.Tardiness = 0; // Kısıtlardan dolayı zaten 0 olmak zorunda
            }
            return true;
        }

        // status == Infeasible (Uygun çözüm yok) veya Unknown (Süre yetmedi)
        return false;
    }
}