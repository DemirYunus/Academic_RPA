using System;
using System.Collections.Generic;
using System.Linq;

public enum RobotSortRule
{
    IirCountAscending,
    IdleWindowCountAscending,
    IdleWindowAverageDurationAscending,
    IirCountDescending,
    IdleWindowCountDescending,
    IdleWindowAverageDurationDescending
}

public class RobotListGenerator
{
    private readonly int _department;
    private readonly int _account;
    private readonly List<Software> _requiredSoftwares;
    private readonly List<Robot> _rawRobotList;

    // Sınıf kurulurken RequiredSoftwares bilgisini de alıyoruz
    public RobotListGenerator(int department, int account, List<Software> requiredSoftwares, List<Robot> rawRobotList)
    {
        _department = department;
        _account = account;
        _requiredSoftwares = requiredSoftwares ?? new List<Software>();
        _rawRobotList = rawRobotList ?? new List<Robot>();
    }

    // Yardımcı filtre metodu: 
    // Eğer gelen işin departmanı 0 ise, sadece "0" olan veya departmanı atanmamış robotları getirir.
    // Başka departmana tahsisli robotları (örn: "1") listeden saklayarak korur.
    private IEnumerable<Robot> GetUnreservedRobots()
    {
        return _rawRobotList.Where(r =>
            string.IsNullOrEmpty(r.AllocatedDepartment) || r.AllocatedDepartment == "0");
    }

    // ========================================================================
    // İlk 6 Temel Sıralama Metodu
    // ========================================================================

    public List<Robot> SortByIirCountAscending()
    {
        return GetUnreservedRobots().OrderBy(r => r.IIR?.Count ?? 0).ToList();
    }

    public List<Robot> SortByIdleWindowCountAscending()
    {
        return GetUnreservedRobots().OrderBy(r => r.LstIdleWindow?.Count ?? 0).ToList();
    }

    public List<Robot> SortByIdleWindowAverageDurationAscending()
    {
        return GetUnreservedRobots().OrderBy(r => GetAverageIdleWindowDuration(r)).ToList();
    }

    public List<Robot> SortByIirCountDescending()
    {
        return GetUnreservedRobots().OrderByDescending(r => r.IIR?.Count ?? 0).ToList();
    }

    public List<Robot> SortByIdleWindowCountDescending()
    {
        return GetUnreservedRobots().OrderByDescending(r => r.LstIdleWindow?.Count ?? 0).ToList();
    }

    public List<Robot> SortByIdleWindowAverageDurationDescending()
    {
        return GetUnreservedRobots().OrderByDescending(r => GetAverageIdleWindowDuration(r)).ToList();
    }

    // Robotun 1440 dakikalık ufuktaki doluluk oranını hesaplayan yardımcı metot
    private double CalculateUtilization(Robot r)
    {
        // Eğer boşluk listesi yoksa veya boşsa, robot %100 doludur (1.0)
        if (r.LstIdleWindow == null || r.LstIdleWindow.Count == 0)
            return 1.0;

        // Boşlukların (Idle Windows) toplam süresini bul
        double totalIdleTime = r.LstIdleWindow.Sum(w => w.End - w.Start);

        // Doluluk Oranı = (Toplam Süre - Boş Zaman) / Toplam Süre
        return (1440.0 - totalIdleTime) / 1440.0;
    }

    // Doluluk oranına göre ARTAN sırada sıralama (En boş robot ilk sırada)
    public List<Robot> SortByUtilizationAscending()
    {
        return GetUnreservedRobots().OrderBy(r => CalculateUtilization(r)).ToList();
    }

    // Doluluk oranına göre AZALAN sırada sıralama (En dolu robot ilk sırada)
    public List<Robot> SortByUtilizationDescending()
    {
        return GetUnreservedRobots().OrderByDescending(r => CalculateUtilization(r)).ToList();
    }

    // ========================================================================
    // Özel Sıralama Metotları (7 ve 8)
    // ========================================================================

    // ========================================================================
    // 7. AllocatedDepartment Tipine Göre (Kesin Eşleşme)
    // ========================================================================

    public List<Robot> SortByAllocatedDepartment(RobotSortRule sortRule)
    {
        // Sadece TaskProcess.Department (_department) ile Robot.AllocatedDepartment değeri birebir eşleşenleri filtrele
        var filteredList = _rawRobotList.Where(r =>
            string.Equals(r.AllocatedDepartment, _department.ToString(), StringComparison.OrdinalIgnoreCase));

        // Filtrelenmiş liste üzerinde istenen sıralama kuralını (sortRule) uygulayabilmek için
        // dummy (etkisiz) bir OrderBy ile IOrderedEnumerable yapısına çevirip ikincil sıralama metoduna gönderiyoruz.
        var ordered = filteredList.OrderBy(r => 1);

        return ApplySecondarySort(ordered, sortRule).ToList();
    }

    // Parametre olarak requiredSoftwares almıyor, sınıftaki (_requiredSoftwares) alanını kullanıyor
    public List<Robot> SortByLoadedSoftware(RobotSortRule secondaryRule)
    { 
        var ordered = GetUnreservedRobots().OrderByDescending(r => CountMatchingSoftware(r));

        return ApplySecondarySort(ordered, secondaryRule).ToList();
    }

    // ========================================================================
    // Ortak Yardımcı Metotlar
    // ========================================================================

    private double GetAverageIdleWindowDuration(Robot r)
    {
        if (r.LstIdleWindow == null || !r.LstIdleWindow.Any())
            return 0;

        return r.LstIdleWindow.Average(w => w.End - w.Start);
    }

    // Sınıf içerisindeki _requiredSoftwares listesi üzerinden karşılaştırma yapıyor
    private int CountMatchingSoftware(Robot r)
    {
        if (!_requiredSoftwares.Any()) return 0;
        if (r.LoadedSoftware == null || !r.LoadedSoftware.Any()) return 0;

        int matchCount = 0;
        foreach (var req in _requiredSoftwares)
        {
            if (r.LoadedSoftware.Any(ls => string.Equals(ls.Name, req.Name, StringComparison.OrdinalIgnoreCase)))
            {
                matchCount++;
            }
        }
        return matchCount;
    }

    private IOrderedEnumerable<Robot> ApplySecondarySort(IOrderedEnumerable<Robot> source, RobotSortRule rule)
    {
        switch (rule)
        {
            case RobotSortRule.IirCountAscending:
                return source.ThenBy(r => r.IIR?.Count ?? 0);

            case RobotSortRule.IdleWindowCountAscending:
                return source.ThenBy(r => r.LstIdleWindow?.Count ?? 0);

            case RobotSortRule.IdleWindowAverageDurationAscending:
                return source.ThenBy(r => GetAverageIdleWindowDuration(r));

            case RobotSortRule.IirCountDescending:
                return source.ThenByDescending(r => r.IIR?.Count ?? 0);

            case RobotSortRule.IdleWindowCountDescending:
                return source.ThenByDescending(r => r.LstIdleWindow?.Count ?? 0);

            case RobotSortRule.IdleWindowAverageDurationDescending:
                return source.ThenByDescending(r => GetAverageIdleWindowDuration(r));

            default:
                return source.ThenBy(r => r.IIR?.Count ?? 0);
        }
    }
}