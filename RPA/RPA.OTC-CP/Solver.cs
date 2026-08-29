using System;
using System.Collections.Generic;
using System.Linq;

namespace RPA.OTC_CP
{
    // EN GENEL KATMAN: ANA ORKESTRATÖR
    public class Solver
    {
        /// <summary>
        /// İteratif Yazılım Konsolidasyon (OTC-CP) algoritmasını başlatır.
        /// Herhangi bir iyileşme (lisans düşüşü) yaparsa true döndürür.
        /// </summary>
        public static bool SolveOTC_CP(List<Robot> robotList, List<TaskProcess> lstProcess)
        {
            bool globalImprovement = false;
            bool isImproved = true;

            // GLOBAL TABU HAFIZASI: While dışına alındı! 
            // Aynı (Process, KaynakRobot, HedefRobot) ikilisi tüm optimizasyon boyunca EN FAZLA BİR KEZ denenir. 
            // Bu sayede ping-pong (git-gel) sonsuz döngüleri kesin olarak engellenir.
            HashSet<string> attemptedMoves = new HashSet<string>();

            // Sistemde yapılabilecek tüm kârlı hamleler bitene kadar döner
            while (isImproved)
            {
                isImproved = false;

                // 1. ADIM: Sızıntı Radarı (En maliyetli aykırı görevler en üstte gelir)
                List<OutlierInfo> outliers = OutlierDetector.DetectOutliers(robotList, lstProcess);

                if (!outliers.Any())
                    break; // Sızıntı kalmadıysa çık

                // 2. ADIM: Aykırı Görevleri Sırayla Gez
                foreach (var outlier in outliers)
                {
                    // 3. ADIM: Bu spesifik görev için uygun Donör Robotları getir (Sıralı halde)
                    List<Robot> donors = DonorMatcher.FindDonors(outlier, robotList, lstProcess);

                    // 4. ADIM: Donörleri sırayla dene
                    foreach (var donor in donors)
                    {
                        // Benzersiz hamle imzası (Hangi süreç, hangi robottan, hangi robota gidiyor?)
                        string moveKey = $"{outlier.OutlierProcess.ProcessID}_{outlier.SourceRobot.RobotID}_{donor.RobotID}";

                        // Eğer bu hamle daha önce (tüm süreç boyunca) denendiyse KESİNLİKLE atla!
                        if (attemptedMoves.Contains(moveKey))
                            continue;

                        // Hamleyi global hafızaya kaydet
                        attemptedMoves.Add(moveKey);

                        // MİKRO-SHIFT (Kaydırma) Denemesi
                        bool shiftSuccess = MicroPerturbationOperator.TryShiftOutlier(outlier, donor, robotList, lstProcess);

                        if (shiftSuccess)
                        {
                            isImproved = true;
                            globalImprovement = true;

                            break; // İşlem başarılı! Donör döngüsünü kır, while başa dönsün.
                        }
                    }

                    // KRİTİK NOKTA: Eğer bir görev başarıyla kaydırıldıysa, sistemin tüm boşlukları 
                    // ve hesap (account) radarı değişmiş demektir. Listeleri tazelemek için 
                    // Outlier döngüsünü de kırıp while'ın en başına dönüyoruz.
                    if (isImproved)
                        break;
                }
            }

            // Döngü bittiğinde robotların güncel yazılımlarını son bir kez netleştir
            RepairOperators.UpdateRobotSoftwares(robotList, lstProcess);

            return globalImprovement;
        }
    }
}