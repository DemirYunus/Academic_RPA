using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

public static class ProcessListGenerator
{
    // 1. Process listesini rastgele sıralayan metot
    public static List<TaskProcess> GetRandomSorted(List<TaskProcess> processes)
    {
        // Guid.NewGuid() kullanımı koleksiyonu rastgele karıştırmak için pratik ve hızlı bir yöntemdir.
        return processes.OrderBy(p => Guid.NewGuid()).ToList();
    }

    // 2. Process'in içindeki Instance sayısına göre büyükten küçüğe (azalan) sıralayan metot
    public static List<TaskProcess> GetSortedByInstanceCountDesc(List<TaskProcess> processes)
    {
        return processes.OrderByDescending(p => p.InstancesOfProcess.Count).ToList();
    }

    // 3. Instance'ların ortalama WindowLenght değerine göre küçükten büyüğe (artan) sıralayan metot
    public static List<TaskProcess> GetSortedByAvgWindowLengthAsc(List<TaskProcess> processes)
    {
        return processes.OrderBy(p =>
            // Eğer process'in instance'ı varsa ortalamasını al, yoksa 0 kabul et
            p.InstancesOfProcess.Any()
                ? p.InstancesOfProcess.Average(i => i.WindowLenght)
                : 0
        ).ToList();
    }

    // 4. Instance'ların ortalama ProcessingTime değerine göre büyükten küçüğe (azalan) sıralayan metot
    public static List<TaskProcess> GetSortedByAvgProcessingTimeDesc(List<TaskProcess> processes)
    {
        return processes.OrderByDescending(p =>
            // Eğer process'in instance'ı varsa ProcessingTime ortalamasını al, yoksa 0 kabul et
            p.InstancesOfProcess.Any()
                ? p.InstancesOfProcess.Average(i => i.ProcessingTime)
                : 0
        ).ToList();
    }


    // 5. Instance'ların ortalama WindowLenght değerine göre büyükten küçüğe (azalan) sıralayan metot
    public static List<TaskProcess> GetSortedByAvgWindowLengthDesc(List<TaskProcess> processes)
    {
        return processes.OrderBy(p =>
            // Eğer process'in instance'ı varsa ortalamasını al, yoksa 0 kabul et
            p.InstancesOfProcess.Any()
                ? p.InstancesOfProcess.Average(i => i.WindowLenght)
                : 0
        ).ToList();
    }

    /// <summary>
    /// Görevleri (TaskProcess) sistemi en çok kısıtlayacak (darboğaz yaratacak) özelliklerine göre 
    /// "Most Constrained Variable First" (En Kısıtlı Değişken Öncelikli) stratejisiyle sıralar.
    /// Sıralama önceliği sırasıyla; Departman (Department) azalan, Hesap (Account) azalan 
    /// ve İşlem Süresi (ProcessingTime) azalan şeklindedir. Yerleştirilmesi en zor ve karmaşık olan 
    /// görevleri listenin en başına alarak, algoritmanın yerel optimuma (local optimum) erken takılmasını önler.
    /// </summary>
    /// <param name="processes">Sıralanacak olan TaskProcess listesi</param>
    /// <returns>Kısıtlayıcılık seviyesine göre azalan sırada oluşturulmuş yeni liste</returns>
    public static List<TaskProcess> MostConstrainedVariableFirst(List<TaskProcess> processes)
    {
        if (processes == null || processes.Count == 0)
            return new List<TaskProcess>();

        // Sistemi en çok kısıtlayacak (darboğaz yaratacak) görevleri en başa al
        return processes
            .OrderByDescending(p => p.Department)
            .ThenByDescending(p => p.Account)
            .ThenByDescending(p => p.InstancesOfProcess.Average(i => i.ProcessingTime))
            .ToList();
    }

    /// <summary>
    /// Görevleri (TaskProcess) sistemi en çok kısıtlayacak özelliklerine göre 
    /// "Most Constrained Variable First" (En Kısıtlı Değişken Öncelikli) stratejisinin alternatif versiyonuyla sıralar.
    /// Sıralama önceliği sırasıyla; Hesap (Account) azalan, Departman (Department) azalan 
    /// ve İşlem Süresi (ProcessingTime) azalan şeklindedir. Ortak kaynak kullanımının (Account) 
    /// daha kritik ve kısıtlayıcı olduğu senaryolarda darboğazları aşmak için kullanılır.
    /// </summary>
    /// <param name="processes">Sıralanacak olan TaskProcess listesi</param>
    /// <returns>Kısıtlayıcılık seviyesine göre azalan sırada oluşturulmuş yeni liste</returns>
    public static List<TaskProcess> MostConstrainedVariableFirstV2(List<TaskProcess> processes)
    {
        if (processes == null || processes.Count == 0)
            return new List<TaskProcess>();

        // Ortak kaynak (Account) kısıtını en yüksek önceliğe alarak sırala
        return processes
            .OrderByDescending(p => p.Account)
            .ThenByDescending(p => p.Department)
            .ThenByDescending(p => p.InstancesOfProcess.Average(i => i.ProcessingTime))
            .ToList();
    }

    /// <summary>
    /// Görevleri sırasıyla; Departman, Account ve "Zaman Penceresi Sıkışıklığı" (Tightness Ratio)
    /// değerlerine göre büyükten küçüğe sıralayarak en kısıtlı işlemleri (Most Constrained) önceliklendirir.
    /// Sıkışıklık Oranı = ProcessingTime / (DueTime - ReleaseTime)
    /// </summary>
    public static List<TaskProcess> MostConstrainedVariableFirstV3(List<TaskProcess> processes)
    {
        if (processes == null || processes.Count == 0)
            return new List<TaskProcess>();

        // Sistemi en çok kısıtlayacak (darboğaz yaratacak) görevleri en başa al
        return processes
            .OrderByDescending(p => p.Department)
            .ThenByDescending(p => p.Account)
            .ThenByDescending(p =>
                p.InstancesOfProcess != null && p.InstancesOfProcess.Any()
                    ? p.InstancesOfProcess.Average(i =>
                        // Sıfıra bölme hatasını önlemek için pencere uzunluğunu kontrol et
                        (i.DueTime - i.ReleaseTime) > 0
                            ? (double)i.ProcessingTime / (i.DueTime - i.ReleaseTime)
                            : 1.0) // Eğer pencere sıfırsa, en kısıtlı durumdur (1.0 kabul edilir)
                    : 0.0)
            .ToList();
    }

    /// <summary>
    /// Görevleri sırasıyla; Departman, Account ve içerdikleri alt işlemlerin (Instance) sahip olduğu 
    /// "En Yüksek Zaman Penceresi Sıkışıklığı" (Maximum Tightness Ratio) değerlerine göre büyükten küçüğe sıralar.
    /// Sıkışıklık Oranı = ProcessingTime / (DueTime - ReleaseTime)
    /// </summary>
    public static List<TaskProcess> MostConstrainedVariableFirstV4(List<TaskProcess> processes)
    {
        if (processes == null || processes.Count == 0)
            return new List<TaskProcess>();

        // Sistemi en çok kısıtlayacak (darboğaz yaratacak) görevleri en başa al
        return processes
            .OrderByDescending(p => p.Account)
            .ThenByDescending(p => p.Department)
            .ThenByDescending(p =>
                p.InstancesOfProcess != null && p.InstancesOfProcess.Any()
                    ? p.InstancesOfProcess.Max(i =>
                        // Sıfıra bölme hatasını önlemek için pencere uzunluğunu kontrol et
                        (i.DueTime - i.ReleaseTime) > 0
                            ? (double)i.ProcessingTime / (i.DueTime - i.ReleaseTime)
                            : 1.0) // Eğer pencere sıfırsa, en kısıtlı durumdur (1.0 kabul edilir)
                    : 0.0)
            .ToList();
    }
}