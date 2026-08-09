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
}