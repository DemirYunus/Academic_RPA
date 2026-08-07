using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RPA.GRASP
{
    public class WindowFilter
    {
        // Belirli bir zaman penceresine (windowStart, windowEnd) düşen boşlukları filtreler ve kırpar
        public static List<TimeHorizon> GetIdleTimesInWindow(string resourceId, List<TimeHorizon> idleTimes, int windowStart, int windowEnd)
        {
            var filteredSlots = new List<TimeHorizon>();

            foreach (var slot in idleTimes)
            {
                // Eğer boşluk tamamen zaman penceresinin solunda veya sağında kalıyorsa atla
                if (slot.End <= windowStart || slot.Start >= windowEnd)
                {
                    continue;
                }

                // Kesişen bölgenin başlangıç ve bitişini hesapla (Taşan kısımları pencere sınırlarına çek)
                int clippedStart = Math.Max(slot.Start, windowStart);
                int clippedEnd = Math.Min(slot.End, windowEnd);

                filteredSlots.Add(new TimeHorizon(clippedStart, clippedEnd));
            }

            return filteredSlots;
        }
    }
}
