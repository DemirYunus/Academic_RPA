using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RPA.LNS
{
    public class NeighborhoodOperator
    {
        /// <summary>
        /// İçindeki işlemlerin başka robotlara aktarılması planlanan (boşaltılacak) 
        /// i. sıradaki robotun Instance (görev) listesini döndürür.
        /// (LNS literatüründeki "Destroy/Ruin" adımı için kullanılır)
        /// </summary>
        /// <param name="sortedRobotList">İş yüküne göre sıralanmış ana robot listesi</param>
        /// <param name="i">Boşaltılacak robotun baştan sıra numarası (indeksi)</param>
        /// <returns>İlgili robota ait görev listesi (IIR)</returns>
        public static List<Instance> GetInstancesToRelocate(List<Robot> sortedRobotList, int i)
        {
            // Liste ve indeks geçerliliği kontrolü
            if (sortedRobotList == null || i < 0 || i >= sortedRobotList.Count)
            {
                return null;
            }

            // Robot sınıfınızda görevler 'IIR' (Instance listesi) olarak tutuluyor
            return sortedRobotList[i].IIR;
        }

        /// <summary>
        /// Görevlerin ekleneceği hedef robotu; sökülen robotun departmanına göre filtrelenmiş,
        /// sökülen robotun kendisinin hariç tutulduğu alt listenin sonundan j. indeksinden bularak döndürür.
        /// </summary>
        /// <param name="sortedRobotList">İş yüküne göre sıralanmış ana robot listesi</param>
        /// <param name="sourceRobot">İşleri sökülen (boşaltılan) kaynak robot nesnesi</param>
        /// <param name="j">Filtrelenmiş alt listenin sonundan indeks numarası (0 = en sonuncu robot)</param>
        /// <returns>Şartlara uygun hedef Robot nesnesi, uygun robot yoksa null.</returns>
        public static Robot GetTargetRobotFromEnd(List<Robot> sortedRobotList, Robot sourceRobot, int j)
        {
            // Liste ve kaynak robot geçerliliği kontrolü
            if (sortedRobotList == null || sortedRobotList.Count == 0 || sourceRobot == null)
            {
                return null;
            }

            // 1. Filtreleme: 
            // - Boşaltılan kaynak robotun kendisini listeden hariç tut (r != sourceRobot)
            // - Kaynak robot ile aynı departmana tahsis edilmiş robotları filtrele (AllocatedDepartment eşleşmesi)
            var filteredList = sortedRobotList
                .Where(r => r != sourceRobot && r.AllocatedDepartment == sourceRobot.AllocatedDepartment)
                .ToList();

            // Şartlara uygun başka robot kalmadıysa null dön
            if (filteredList.Count == 0)
            {
                return null;
            }

            // 2. Sondan j. indeksin hesaplanması (0 = filtrelenmiş listenin en son elemanı)
            int targetIndex = filteredList.Count - 1 - j;

            // Hesaplanan indeks filtrelenmiş liste sınırları dışındaysa null dön
            if (targetIndex < 0 || targetIndex >= filteredList.Count)
            {
                return null;
            }

            return filteredList[targetIndex];
        }

        /// <summary>
        /// Şartları sağlayan (kaynak robot hariç, aynı departmana sahip) 
        /// toplam hedef robot sayısını döndürür.
        /// </summary>
        /// <param name="sortedRobotList">İş yüküne göre sıralanmış ana robot listesi</param>
        /// <param name="sourceRobot">İşleri sökülen (boşaltılan) kaynak robot nesnesi</param>
        /// <returns>Uygun hedef robotların toplam sayısı</returns>
        public static int GetNumOfTargetRobot(List<Robot> sortedRobotList, Robot sourceRobot)
        {
            if (sortedRobotList == null || sortedRobotList.Count == 0 || sourceRobot == null)
            {
                return 0;
            }

            // Aynı filtreleme mantığı: Kaynak robotu hariç tut ve departman eşleşmesine bak
            return sortedRobotList
                .Where(r => r != sourceRobot && r.AllocatedDepartment == sourceRobot.AllocatedDepartment)
                .Count();
        }

        /// <summary>
        /// Sökülen instance'lar içerisindeki benzersiz ID_Process değerlerini 
        /// bir dizi (string[]) olarak döndürür.
        /// </summary>
        /// <param name="instancesToRelocate">Boşaltılan robottan sökülen instance listesi</param>
        /// <returns>Benzersiz Process ID'lerini içeren string dizi</returns>
        public static string[] GetDistinctProcessIds(List<Instance> instancesToRelocate)
        {
            if (instancesToRelocate == null || !instancesToRelocate.Any())
            {
                return new string[0]; // Boş dizi döner
            }

            // Instance'ların içindeki ID_Process alanlarını seç, 
            // tekrarlayanları (Distinct) temizle ve diziye (ToArray) çevir
            return instancesToRelocate
                .Where(inst => !string.IsNullOrEmpty(inst.ID_Process))
                .Select(inst => inst.ID_Process)
                .Distinct()
                .ToArray();
        }

        /// <summary>
        /// Sökülen instance'lar ile hedef robotun mevcut IIR listesini tek bir listede birleştirir.
        /// </summary>
        /// <param name="instancesToRelocate">Boşaltılan robottan sökülen instance listesi</param>
        /// <param name="targetRobot">Görevlerin aktarılacağı hedef robot</param>
        /// <returns>İki listenin birleşiminden oluşan tek bir Instance listesi</returns>
        public static List<Instance> MergeInstances(List<Instance> instancesToRelocate, Robot targetRobot)
        {
            var combinedList = new List<Instance>();

            // 1. Sökülen işleri ekle
            if (instancesToRelocate != null && instancesToRelocate.Any())
            {
                combinedList.AddRange(instancesToRelocate);
            }

            // 2. Hedef robotun üzerindeki mevcut işleri (IIR) ekle
            if (targetRobot != null && targetRobot.IIR != null && targetRobot.IIR.Any())
            {
                combinedList.AddRange(targetRobot.IIR);
            }

            return combinedList;
        }

        /// <summary>
        /// Sıkıştırılacak instance'lar için, diğer robotlarda çalışan aynı Account'a (Hesap) sahip 
        /// görevlerin zaman dilimlerini "Yasaklı Periyot" olarak hesaplar ve döndürür.
        /// </summary>
        /// <param name="mergedInstancesToRelocate">CP çözücüsüne girecek olan sökülmüş/hedef görevler havuzu</param>
        /// <param name="lstProcess">Account bilgisini çekmek için kullanılacak ana süreç listesi</param>
        /// <param name="rawRobotList">Sistemdeki diğer robotların güncel durumunu tutan ana liste</param>
        /// <returns>Key: ID_Process, Value: Başlangıç ve Bitiş dakikalarını tutan yasaklı zaman dilimleri listesi</returns>
        public static Dictionary<string, List<Tuple<int, int>>> GetForbiddenPeriodsByAccount(
            List<Instance> mergedInstancesToRelocate,
            List<TaskProcess> lstProcess,
            List<Robot> rawRobotList)
        {
            var forbiddenPeriods = new Dictionary<string, List<Tuple<int, int>>>();

            if (mergedInstancesToRelocate == null || lstProcess == null || rawRobotList == null)
            {
                return forbiddenPeriods;
            }

            // 1. CP'ye girecek olan görevlerin benzersiz Process ID'lerini bul
            var distinctProcessIds = mergedInstancesToRelocate
                .Where(i => !string.IsNullOrEmpty(i.ID_Process))
                .Select(i => i.ID_Process)
                .Distinct()
                .ToList();

            // CP havuzundaki tüm instance ID'lerini hızlı arama için bir HashSet'e al (Sökülenleri sabit kabul etmemek için)
            var relocatedInstanceIds = new HashSet<string>(
                mergedInstancesToRelocate.Select(i => i.ID_Process_Instance)
            );

            foreach (var processId in distinctProcessIds)
            {
                // Bu process'in Account bilgisini bul
                var processInfo = lstProcess.FirstOrDefault(p => p.ProcessID == processId);
                if (processInfo == null) continue;

                int targetAccount = processInfo.Account;
                // KRİTİK DÜZELTME: Eğer işlem bir Account'a bağlı değilse (0 veya -1 ise), 
                // diğer robotlardaki işlerle çakışma ihtimali yoktur, yasaklı periyot aranmaz!
                if (targetAccount <= 0)
                {
                    continue;
                }
                var blockedIntervals = new List<Tuple<int, int>>();

                // 2. Sistemdeki tüm robotları tara ve bu Account'u kullanan DİĞER sabit görevleri bul
                foreach (var robot in rawRobotList)
                {
                    if (robot.IIR == null) continue;

                    foreach (var scheduledInst in robot.IIR)
                    {
                        // Eğer bu instance zaten şu an LNS ile söküp yeniden planlamaya çalıştığımız bir iş ise onu atla
                        if (relocatedInstanceIds.Contains(scheduledInst.ID_Process_Instance))
                            continue;

                        // Bu sabit instance'ın Account değerini kontrol et
                        var scheduledProcessInfo = lstProcess.FirstOrDefault(p => p.ProcessID == scheduledInst.ID_Process);

                        if (scheduledProcessInfo != null && scheduledProcessInfo.Account == targetAccount)
                        {
                            // Aynı account kullanılıyor. Başlangıç ve bitiş zamanı belliyse yasaklı periyot olarak ekle.
                            if (scheduledInst.StartTime.HasValue && scheduledInst.FinishTime.HasValue)
                            {
                                blockedIntervals.Add(new Tuple<int, int>(
                                    (int)scheduledInst.StartTime.Value,
                                    (int)scheduledInst.FinishTime.Value
                                ));
                            }
                        }
                    }
                }

                // Eğer bu ID_Process için yasaklı zamanlar bulunduysa sözlüğe ekle
                if (blockedIntervals.Any())
                {
                    forbiddenPeriods[processId] = blockedIntervals;
                }
            }

            return forbiddenPeriods;
        }

        private const int HorizonStart = 0;
        private const int HorizonEnd = 1440; // 24 Saat * 60 Dakika     

        /// <summary>
        /// Robotun üzerindeki güncel görev (IIR) listesine bakarak, 
        /// 0-1440 ufku (horizon) içerisindeki boş zaman (IdleWindow) pencerelerini sıfırdan hesaplar.
        /// </summary>
        /// <param name="robot">Boşlukları hesaplanacak robot nesnesi</param>
        /// <returns>Zamansa göre sıralı boşluk pencereleri listesi</returns>
        public static List<IdleWindow> CalculateIdleWindows(Robot robot)
        {
            var newIdleWindows = new List<IdleWindow>();

            if (robot == null)
                return newIdleWindows;

            // Eğer robot tamamen boşsa (örneğin kaynak robot işleri verdikten sonra), tüm gün boştur
            if (robot.IIR == null || robot.IIR.Count == 0)
            {
                newIdleWindows.Add(new IdleWindow(HorizonStart, HorizonEnd));
                return newIdleWindows;
            }

            // İşleri başlangıç zamanlarına göre küçükten büyüğe sırala
            // (Güvenlik için StartTime ve FinishTime değeri null olmayanları alıyoruz)
            var sortedTasks = robot.IIR
                .Where(t => t.StartTime.HasValue && t.FinishTime.HasValue)
                .OrderBy(t => t.StartTime.Value)
                .ToList();

            int currentMarker = HorizonStart;

            foreach (var task in sortedTasks)
            {
                int taskStart = (int)task.StartTime.Value;
                int taskEnd = (int)task.FinishTime.Value;

                // Eğer şu anki imleç ile işin başlangıcı arasında boşluk varsa, bunu boş zaman olarak ekle
                if (taskStart > currentMarker)
                {
                    newIdleWindows.Add(new IdleWindow(currentMarker, taskStart));
                }

                // İmleci, bu işin bittiği noktaya taşı (üst üste binmelere karşı Math.Max ile güvenlik önlemi alıyoruz)
                currentMarker = Math.Max(currentMarker, taskEnd);
            }

            // Son iş bittikten sonra gün sonuna (1440) kadar hala boş vakit kaldıysa onu da ekle
            if (currentMarker < HorizonEnd)
            {
                newIdleWindows.Add(new IdleWindow(currentMarker, HorizonEnd));
            }

            return newIdleWindows;
        }
    }


}

