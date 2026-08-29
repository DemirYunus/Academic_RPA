using System;
using System.Collections.Generic;
using System.Linq;

namespace RPA.OTC_CP
{
    // 5. MİKRO KAYDIRMA OPERATÖRÜ (CP MOTORU TETİKLEYİCİSİ)
    public class MicroPerturbationOperator
    {
        /// <summary>
        /// Aykırı görevi donör robota kaydırmayı dener. Başarılı olursa orijinal robottan işi siler.
        /// </summary>
        public static bool TryShiftOutlier(
            OutlierInfo outlier,
            Robot donorRobot,
            List<Robot> allActiveRobots,
            List<TaskProcess> lstProcess)
        {
            // 1. CP Motoru için "Zamanı Esnek / Robotu Sabit" radar paketini hazırla (Account radarı dahil)
            var fixedProcesses = CPDataPreparer.BuildRadarForShift(outlier, donorRobot, allActiveRobots, lstProcess);

            // 2. CP Motoru liste beklediği için tekil aykırı görevi listeye sar (Tam Esnek olarak çalışacak)
            var freeProcesses = new List<TaskProcess> { outlier.OutlierProcess };

            // 3. Atama yapılabilecek tek adayımız Donör Robot
            var availableRobots = new List<Robot> { donorRobot };

            // 4. MEVCUT CP MOTORUNUZU ÇAĞIRIYORUZ
            // (Sisteminizdeki CPSolver.SolveWithPartialFlexibility metodu çağrılır)
            bool isSuccess = CPSolver.SolveWithPartialFlexibility(
                freeProcesses,
                fixedProcesses,
                availableRobots
            );

            // 5. Eğer CP çözücü bu işi donör robota sığdırmayı başardıysa:
            if (isSuccess)
            {
                // CPSolver işi yeni robota ekler ancak eski robottan silmez. 
                // Bu yüzden eski (source) robottan bu TaskProcess'e ait Instance'ları fiziksel olarak temizliyoruz.
                RemoveProcessFromRobot(outlier.SourceRobot, outlier.OutlierProcess);

                return true;
            }

            return false;
        }

        // Başarılı kaydırma sonrası eski robottaki kalıntıları temizleyen yardımcı metot
        private static void RemoveProcessFromRobot(Robot sourceRobot, TaskProcess processToRemove)
        {
            if (sourceRobot.IIR != null)
            {
                // string tabanlı ProcessID eşleşmesine göre Instance'ları uçur
                sourceRobot.IIR.RemoveAll(i => i.ID_Process == processToRemove.ProcessID);
            }
        }
    }
}