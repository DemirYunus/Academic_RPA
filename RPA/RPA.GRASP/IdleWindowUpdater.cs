using System;
using System.Collections.Generic;
using System.Linq;

namespace RPA.GRASP // Kendi projenizin namespace'ini buraya uygun şekilde ayarlayabilirsiniz
{
    public class IdleWindowUpdater
    {
        private const int HorizonStart = 0;
        private const int HorizonEnd = 1440;

        // Yeni operasyon ekleyen ve boş zamanları güncelleyen metot
        public static List<IdleWindow> UpdateIdleTimes(List<IdleWindow> currentIdleTimes, int opStart, int opEnd)
        {
            // Liste boş veya null ise tüm ufuk (0-1440) boş kabul edilir
            if (currentIdleTimes == null || !currentIdleTimes.Any())
            {
                currentIdleTimes = new List<IdleWindow> { new IdleWindow(HorizonStart, HorizonEnd) };
            }

            var newIdleTimes = new List<IdleWindow>();

            foreach (var idle in currentIdleTimes)
            {
                // Eğer operasyon mevcut boşlukla kesişmiyorsa, boşluğu aynen koru
                if (opEnd <= idle.Start || opStart >= idle.End)
                {
                    newIdleTimes.Add(idle);
                }
                else
                {
                    // Kesişme durumu: Boşluğu parçala (öncesi ve sonrası olarak)
                    if (idle.Start < opStart)
                    {
                        newIdleTimes.Add(new IdleWindow(idle.Start, opStart));
                    }
                    if (idle.End > opEnd)
                    {
                        newIdleTimes.Add(new IdleWindow(opEnd, idle.End));
                    }
                }
            }

            // Güncellenmiş boşlukları zamana göre sıralayarak geri döndür
            return newIdleTimes.OrderBy(x => x.Start).ToList();
        }
    }
}