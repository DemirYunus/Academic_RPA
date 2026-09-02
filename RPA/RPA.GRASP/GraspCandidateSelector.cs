using System;
using System.Collections.Generic;
using System.Linq;

public class GraspCandidateSelector
{
    private readonly Random _random;

    public GraspCandidateSelector()
    {
        // Rastgele seçimler için tek bir Random nesnesi kullanıyoruz.
        _random = new Random();
    }

    // ========================================================================
    // 1. Average ProcessingTime (Büyükten Küçüğe Sıralı - Maksimizasyon)
    // ========================================================================

    public TaskProcess SelectByProcessingTime_Cardinality(List<TaskProcess> sortedList, int k)
    {
        return SelectRandomFromTopK(sortedList, k);
    }

    public TaskProcess SelectByProcessingTime_ValueBased(List<TaskProcess> sortedList, double alpha)
    {
        if (sortedList == null || !sortedList.Any()) return null;

        // Liste büyükten küçüğe sıralı olduğu için ilk eleman max, son eleman min'dir.
        double c_max = GetAvgProcessingTime(sortedList.First());
        double c_min = GetAvgProcessingTime(sortedList.Last());

        // Maksimizasyon kuralı
        double threshold = c_max - alpha * (c_max - c_min);

        var rcl = sortedList.Where(tp => GetAvgProcessingTime(tp) >= threshold).ToList();
        return SelectRandomFromList(rcl);
    }

    // ========================================================================
    // 2. Average WindowLenght (Küçükten Büyüğe Sıralı - Minimizasyon)
    // ========================================================================

    public TaskProcess SelectByWindowLength_Cardinality(List<TaskProcess> sortedList, int k)
    {
        return SelectRandomFromTopK(sortedList, k);
    }

    public TaskProcess SelectByWindowLength_ValueBased(List<TaskProcess> sortedList, double alpha)
    {
        if (sortedList == null || !sortedList.Any()) return null;

        // Liste küçükten büyüğe sıralı olduğu için ilk eleman min, son eleman max'tır.
        double c_min = GetAvgWindowLength(sortedList.First());
        double c_max = GetAvgWindowLength(sortedList.Last());

        // Minimizasyon kuralı
        double threshold = c_min + alpha * (c_max - c_min);

        var rcl = sortedList.Where(tp => GetAvgWindowLength(tp) <= threshold).ToList();
        return SelectRandomFromList(rcl);
    }

    // ========================================================================
    // 3. Instance Sayısı (Büyükten Küçüğe Sıralı - Maksimizasyon)
    // ========================================================================

    public TaskProcess SelectByInstanceCount_Cardinality(List<TaskProcess> sortedList, int k)
    {
        return SelectRandomFromTopK(sortedList, k);
    }

    public TaskProcess SelectByInstanceCount_ValueBased(List<TaskProcess> sortedList, double alpha)
    {
        if (sortedList == null || !sortedList.Any()) return null;

        double c_max = sortedList.First().InstancesOfProcess.Count;
        double c_min = sortedList.Last().InstancesOfProcess.Count;

        // Maksimizasyon kuralı
        double threshold = c_max - alpha * (c_max - c_min);

        var rcl = sortedList.Where(tp => tp.InstancesOfProcess.Count >= threshold).ToList();
        return SelectRandomFromList(rcl);
    }

    // 2. YENİ RCL METODU: Makro-Kategori ve Sıkılık Tabanlı RCL
    public TaskProcess SelectByMacroTierAndTightness_ValueBased(List<TaskProcess> sortedList, double alpha)
    {
        if (sortedList == null || !sortedList.Any()) return null;

        var topTask = sortedList.First();

        // O anki en öncelikli işin hangi MAKRO kategoride olduğunu bul
        bool hasAccountConstraint = topTask.Account > 0;
        bool hasDeptConstraint = topTask.Department > 0;

        List<TaskProcess> macroTierCandidates;

        // 1. Makro Kategori Havuzunu Oluştur (Havuz genişletildi, rastgelelik kurtarıldı)
        if (hasAccountConstraint)
        {
            // KATEGORİ 1: Account kısıtı olan TÜM işler (Account ID'si fark etmez)
            macroTierCandidates = sortedList.Where(tp => tp.Account > 0).ToList();
        }
        else if (hasDeptConstraint)
        {
            // KATEGORİ 2: Account yok ama Departman kısıtı olan TÜM işler
            macroTierCandidates = sortedList.Where(tp => tp.Account <= 0 && tp.Department > 0).ToList();
        }
        else
        {
            // KATEGORİ 3: Tamamen Evrensel işler
            macroTierCandidates = sortedList.Where(tp => tp.Account <= 0 && tp.Department <= 0).ToList();
        }

        // 2. Sıkılık (Tightness) Üzerinden Değer Tabanlı Eşik (Alpha) Uygula
        // Makro havuz içindeki en sıkı ve en gevşek değerleri dinamik bul
        double c_max = macroTierCandidates.Max(tp => GetMaxTightness(tp));
        double c_min = macroTierCandidates.Min(tp => GetMaxTightness(tp));

        // Maksimizasyon kuralı (Sıkılığı eşiğin üzerinde olanları RCL'ye al)
        double threshold = c_max - alpha * (c_max - c_min);

        var rcl = macroTierCandidates.Where(tp => GetMaxTightness(tp) >= threshold).ToList();

        return SelectRandomFromList(rcl);
    }

    // ========================================================================
    // Ortak Yardımcı Metotlar
    // ========================================================================

    // 1. YARDIMCI METOT: Prosesin Maksimum Sıkılığını (Tightness) Hesaplar
    private double GetMaxTightness(TaskProcess tp)
    {
        if (tp.InstancesOfProcess == null || !tp.InstancesOfProcess.Any())
            return 0.0;

        return tp.InstancesOfProcess.Max(i =>
            (i.DueTime - i.ReleaseTime) > 0
                ? (double)i.ProcessingTime / (i.DueTime - i.ReleaseTime)
                : 1.0);
    }

    private TaskProcess SelectRandomFromTopK(List<TaskProcess> sortedList, int k)
    {
        if (sortedList == null || !sortedList.Any()) return null;

        // Liste zaten sıralı geldiği için sadece ilk k elemanı alıyoruz.
        var rcl = sortedList.Take(k).ToList();
        return SelectRandomFromList(rcl);
    }

    private TaskProcess SelectRandomFromList(List<TaskProcess> rcl)
    {
        if (rcl == null || !rcl.Any()) return null;

        int index = _random.Next(rcl.Count);
        return rcl[index];
    }

    private double GetAvgProcessingTime(TaskProcess tp)
    {
        if (tp.InstancesOfProcess == null || !tp.InstancesOfProcess.Any()) return 0;
        return tp.InstancesOfProcess.Average(i => i.ProcessingTime);
    }

    private double GetAvgWindowLength(TaskProcess tp)
    {
        if (tp.InstancesOfProcess == null || !tp.InstancesOfProcess.Any()) return 0;
        return tp.InstancesOfProcess.Average(i => i.WindowLenght);
    }
}