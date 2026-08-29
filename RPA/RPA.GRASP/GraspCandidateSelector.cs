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

    // ========================================================================
    // Ortak Yardımcı Metotlar
    // ========================================================================

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