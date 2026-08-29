using System;
using System.Collections.Generic;
using System.Linq;

public enum DestructionType
{
    WeakestRobotRemoval,  // En Zayıf Robotları Sökme
    AccountBasedRemoval,  // Hesap (Account) Çakışmalarını Sökme
    RandomRemoval         // Rastgele Sökme (Sarsıntı)
}

public class ALNSManager
{
    // Operatörlerin seçilme ağırlıklarını tutan sözlük
    private Dictionary<DestructionType, double> _weights;
    private static readonly Random _rnd = new Random();

    // Öğrenme Katsayısı (Genellikle 0.5 ile 0.8 arası alınır)
    private readonly double _rho = 0.5;

    public ALNSManager()
    {
        // Başlangıçta tüm yıkım operatörlerinin şansı eşittir (Örn: 10.0)
        _weights = new Dictionary<DestructionType, double>
        {
            { DestructionType.WeakestRobotRemoval, 10.0 },
            { DestructionType.AccountBasedRemoval, 10.0 },
            { DestructionType.RandomRemoval, 10.0 }
        };
    }

    /// <summary>
    /// Rulet tekerleği (Roulette Wheel) mantığıyla ağırlıklara göre rastgele bir yıkım operatörü seçer.
    /// Ağırlığı yüksek olanın seçilme ihtimali daha fazladır.
    /// </summary>
    public DestructionType SelectDestructionOperator()
    {
        double totalWeight = _weights.Values.Sum();
        double randomValue = _rnd.NextDouble() * totalWeight;
        double cumulative = 0.0;

        foreach (var kvp in _weights)
        {
            cumulative += kvp.Value;
            if (randomValue <= cumulative)
            {
                return kvp.Key;
            }
        }

        // Fallback (Güvenlik amaçlı)
        return DestructionType.WeakestRobotRemoval;
    }

    /// <summary>
    /// İterasyon sonunda, kullanılan operatörün başarısına göre ağırlığını günceller.
    /// </summary>
    /// <param name="usedOperator">Kullanılan yıkım operatörü</param>
    /// <param name="score">Operatörün getirdiği başarı puanı (Örn: İyileşme varsa 5, yoksa 0)</param>
    public void UpdateWeight(DestructionType usedOperator, double score)
    {
        double oldWeight = _weights[usedOperator];
        // ALNS formülü: W_yeni = (1 - rho) * W_eski + (rho * Puan)
        _weights[usedOperator] = ((1.0 - _rho) * oldWeight) + (_rho * score);

        // Ağırlığın sıfıra düşüp operatörün tamamen ölmesini engellemek için minimum bir sınır (Örn: 1.0) koyabiliriz
        if (_weights[usedOperator] < 1.0)
        {
            _weights[usedOperator] = 1.0;
        }
    }
}