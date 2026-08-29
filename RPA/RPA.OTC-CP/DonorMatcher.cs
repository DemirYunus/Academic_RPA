using System;
using System.Collections.Generic;
using System.Linq;

namespace RPA.OTC_CP
{
    // 3. DONÖR EŞLEŞTİRİCİ
    public class DonorMatcher
    {
        /// <summary>
        /// Aykırı görev için ilave yazılım maliyeti yaratmayacak ve departmanı uygun donör robotları bulur.
        /// Toplam boşluk süresine (azalan) ve account çakışma riskine (artan) göre sıralar.
        /// </summary>
        public static List<Robot> FindDonors(OutlierInfo outlier, List<Robot> robotList, List<TaskProcess> lstProcess)
        {
            List<Robot> candidates = new List<Robot>();

            // Aykırı görevin ihtiyaç duyduğu yazılımların listesi (küçük harfe çevrilmiş)
            var requiredSoftwares = outlier.OutlierProcess.RequiredSoftwares?
                .Select(s => s.Name.Trim().ToLower()).ToList() ?? new List<string>();

            int targetAccount = outlier.OutlierProcess.Account;
            string targetDept = outlier.OutlierProcess.Department.ToString();

            foreach (var robot in robotList)
            {
                // 1. Kendi robotuna tekrar atama yapma
                if (robot.RobotID == outlier.SourceRobot.RobotID) continue;

                // 2. Departman Kısıtı: Robotun tahsis departmanı işin departmanına uymalı 
                // (veya robot evrensel '0' departmanına sahipse kabul et)
                if (robot.AllocatedDepartment != targetDept && robot.AllocatedDepartment != "0") continue;

                // 3. Yazılım Maliyeti Kısıtı: Donör robot, aykırı görevin istediği TÜM yazılımları zaten kullanıyor olmalı.
                // HATA DÜZELTİLDİ: ID_Process string olduğu için new List<string>() kullanıldı.
                var robotProcessIds = robot.IIR?.Select(i => i.ID_Process).Distinct().ToList() ?? new List<string>();
                var processesOnRobot = lstProcess.Where(p => robotProcessIds.Contains(p.ProcessID)).ToList();

                HashSet<string> existingSoftwares = new HashSet<string>();
                foreach (var p in processesOnRobot)
                {
                    if (p.RequiredSoftwares != null)
                    {
                        foreach (var sw in p.RequiredSoftwares)
                        {
                            existingSoftwares.Add(sw.Name.Trim().ToLower());
                        }
                    }
                }

                // Aykırı görevin gerektirdiği tüm yazılımlar donörde halihazırda var mı?
                bool hasAllSoftwares = true;
                foreach (var reqSw in requiredSoftwares)
                {
                    if (!existingSoftwares.Contains(reqSw))
                    {
                        hasAllSoftwares = false;
                        break;
                    }
                }

                // Eğer ilave maliyet yaratmayacaksa aday listesine ekle
                if (hasAllSoftwares)
                {
                    candidates.Add(robot);
                }
            }

            // 4. SIRALAMA (Toplam Boşluk Süresi ve Account Riski)
            var sortedDonors = candidates.Select(robot =>
            {
                // A. Toplam İşlem Yükü (Mevcut IIR listesindeki toplam ProcessingTime)
                // Yük ne kadar azsa, boşluk süresi (Idle Time) o kadar fazladır.
                int totalProcessingTime = robot.IIR?.Sum(i => i.ProcessingTime) ?? 0;

                // B. Account Çakışma Riski (Aynı account numarasına sahip kaç süreç var?)
                int accountRisk = 0;
                if (targetAccount > 0 && robot.IIR != null)
                {
                    var robotProcessIds = robot.IIR.Select(i => i.ID_Process).Distinct().ToList();
                    accountRisk = lstProcess.Count(p => robotProcessIds.Contains(p.ProcessID) && p.Account == targetAccount);
                }

                return new { Robot = robot, Load = totalProcessingTime, Risk = accountRisk };
            })
            // Önce iş yüküne göre Artan sırala (Yani Boşluk Süresine göre Azalan),
            // Eşitlik durumunda Account Riskine göre Artan sırala.
            .OrderBy(x => x.Load)
            .ThenBy(x => x.Risk)
            .Select(x => x.Robot)
            .ToList();

            return sortedDonors;
        }
    }
}