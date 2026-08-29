using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RPA.LNS
{
    public class Solve
    {
        public static List<Robot> SolveLNS(List<Robot> rawRobotList, List<TaskProcess> lstProcess)
        {
            List<Robot> sortedRobotList = RobotWorkloadSorter.SortByTotalProcessingTimeAscending(rawRobotList);
            // Döngüye girmeden önce yıkım limitini belirleyin (Örn: En zayıf %30'luk dilim)
            int destructionLimit = (int)Math.Ceiling(sortedRobotList.Count * 0.30);


            int i = 0;
            do
            {
                // Boşaltılacak robotun görevlerini al        
                List<Instance> instancesToRelocate = NeighborhoodOperator.GetInstancesToRelocate(sortedRobotList, i);
                string[] uniqueProcessIds = NeighborhoodOperator.GetDistinctProcessIds(instancesToRelocate);

                // Görevlerin ekleneceği hedef robotu al
                int j = 0;
                int targetCount = NeighborhoodOperator.GetNumOfTargetRobot(sortedRobotList, sortedRobotList[i]);

                // i'nin artıp artmayacağını kontrol etmek için bayrak (Index Shifting Kontrolü)
                bool isRobotDeleted = false;

                do
                {
                    Robot targetRobot = NeighborhoodOperator.GetTargetRobotFromEnd(sortedRobotList, sortedRobotList[i], j);
                    if (targetRobot == null)
                    {
                        //i++;
                        break;
                    }
                    else
                    {
                        //İnstance listesi içindeki her bir unique process için hedef robota yerleştirilmeye çalışılır.
                        for (int p = 0; p < uniqueProcessIds.Length; p++)
                        {
                            // O anki process ID'sine ait işleri filtrele (Sadece o process'i taşıyacağız)
                            List<Instance> subsetToRelocate = instancesToRelocate.Where(inst => inst.ID_Process == uniqueProcessIds[p]).ToList();
                            // KRİTİK DÜZELTME: Bu process daha önceki bir j (hedef robot) adımında başarıyla taşındıysa atla!
                            if (subsetToRelocate.Count == 0)
                                continue;

                            List<Instance> mergedInstancesToRelocate = NeighborhoodOperator.MergeInstances(subsetToRelocate, targetRobot);
                            // Yasaklı periyotları NeighborhoodOperator üzerinden hazırla
                            var forbiddenPeriods = NeighborhoodOperator.GetForbiddenPeriodsByAccount(mergedInstancesToRelocate, lstProcess, rawRobotList);

                            // Hazırlanan veriyi CP sınıfına çözmesi için gönder
                            bool isSuccess = CP.SolveZeroTardiness(mergedInstancesToRelocate, forbiddenPeriods);

                            if (isSuccess)
                            {
                                //Console.WriteLine($"   -> BAŞARILI: {subsetToRelocate.Count} adet iş {targetRobot.RobotName} robotuna aktarıldı!");
                                // ÇÖZÜM BULUNDU! Şimdi nesneler arası transferi (Commit) yapalım.

                                // 1. Sökülen instance'ların "RobotNumber" özelliğini yeni hedef robotun ID'si ile güncelle
                                foreach (var inst in subsetToRelocate)
                                {
                                    inst.RobotNumber = targetRobot.RobotID;
                                }

                                // 2. Sökülen işleri kaynak robotun (Source Robot) IIR listesinden tamamen temizle
                                sortedRobotList[i].IIR.RemoveAll(x => subsetToRelocate.Contains(x));

                                // 3. Hedef robotun (Target Robot) IIR listesini, CP tarafından zamanları ayarlanmış yeni havuz ile değiştir
                                targetRobot.IIR = mergedInstancesToRelocate;

                                // instancesToRelocate ana listesinden de taşıdığımız bu elemanları silelim ki
                                // bir sonraki hedef robota geçersek (j artarsa) aynı işleri tekrar aktarmaya çalışmasın
                                instancesToRelocate.RemoveAll(x => subsetToRelocate.Contains(x));

                                // 4. (Eğer metodunuz varsa) Hedef ve Kaynak robotun boşluk (IdleWindow) pencerelerini yeniden hesapla
                                targetRobot.LstIdleWindow = NeighborhoodOperator.CalculateIdleWindows(targetRobot);
                                sortedRobotList[i].LstIdleWindow = NeighborhoodOperator.CalculateIdleWindows(sortedRobotList[i]);
                            }
                            else
                            {
                                // İşlem başarısız olduğu için nesnelerde hiçbir değişiklik yapmıyoruz.                     
                            }
                        }

                        // Kaynak robot (Sökülen robot) tamamen boşalmış ise
                        if (sortedRobotList[i].IIR == null || sortedRobotList[i].IIR.Count == 0)
                        {
                            // 1. Ana (orijinal) listeden sil
                            rawRobotList.Remove(sortedRobotList[i]);
                            // 2. Üzerinde çalıştığınız sıralı listeden sil
                            sortedRobotList.Remove(sortedRobotList[i]);

                            isRobotDeleted = true; // Robotun silindiğini işaretle

                            break; // Bu robot boşaldığı için artık hedef robot aramaya gerek yok, bir sonraki kaynak robota geç.
                        }
                        else
                        {
                            j++; // Hedef robot aramaya devam et. 
                        }

                    }
                } while (j < targetCount);

                // İNDEKS YÖNETİMİ: 
                // EĞER robot silindiyse, i'yi ARTTIRMIYORUZ (Çünkü sağdaki robot listesi sola, yani 'i' indeksine kaydı).
                // EĞER robot silinmediyse (hedef kalmadı veya işler sığmadı), bir sonraki zayıf robota geçmek için i'yi ARTTIRIYORUZ.
                if (!isRobotDeleted)
                {
                    i++; // Robot silinmediyse (başarısız olduysa), sıradaki robota geç!
                }

            } while (i < destructionLimit);
            return rawRobotList;
        }   
    }
}
