using System;
using System.Collections.Generic;
using System.Linq;

namespace RPA.OTC_CP
{
    // 4. CP VERİ HAZIRLAYICI (ACCOUNT RADARI)
    public class CPDataPreparer
    {
        /// <summary>
        /// Shift (Kaydırma) işlemi için CP Motoruna gönderilecek "Zamanı Esnek, Robotu Sabit" görevler listesini hazırlar.
        /// Hedef robotun kendi işlerini ve sistemdeki diğer robotlardaki çakışan Account işlerini kapsar.
        /// </summary>
        public static List<(TaskProcess Process, Robot LockedRobot)> BuildRadarForShift(
            OutlierInfo outlier,
            Robot donorRobot,
            List<Robot> allActiveRobots,
            List<TaskProcess> lstProcess)
        {
            List<(TaskProcess Process, Robot LockedRobot)> fixedProcessesWithSliding = new List<(TaskProcess, Robot)>();

            // 1. ADIM: Donör Robotun Üzerindeki Mevcut Tüm İşler (Zaman esnetilerek kaydırılabilmeli ki yeni işe yer açılsın)
            if (donorRobot.IIR != null && donorRobot.IIR.Any())
            {
                var donorProcessIds = donorRobot.IIR.Select(i => i.ID_Process).Distinct().ToList();
                var donorProcesses = lstProcess.Where(p => donorProcessIds.Contains(p.ProcessID)).ToList();

                foreach (var p in donorProcesses)
                {
                    fixedProcessesWithSliding.Add((p, donorRobot));
                }
            }

            // 2. ADIM: Account Radarı (Çapraz Kısıtlar)
            // Hem kaydırılacak (Aykırı) görevin hem de donör robotun sahip olduğu Account numaralarını bir havuzda topla
            HashSet<int> riskyAccounts = new HashSet<int>();

            if (outlier.OutlierProcess.Account > 0)
            {
                riskyAccounts.Add(outlier.OutlierProcess.Account);
            }

            if (donorRobot.IIR != null)
            {
                var donorProcessIds = donorRobot.IIR.Select(i => i.ID_Process).Distinct().ToList();
                var donorAccs = lstProcess.Where(p => donorProcessIds.Contains(p.ProcessID) && p.Account > 0)
                                          .Select(p => p.Account).ToList();

                foreach (var acc in donorAccs)
                {
                    riskyAccounts.Add(acc);
                }
            }

            // Eğer riskli account yoksa taramaya gerek yok, doğrudan çık
            if (!riskyAccounts.Any())
            {
                return fixedProcessesWithSliding;
            }

            // 3. ADIM: Dış Sistem Taraması (Donör ve Orijinal robot dışındaki robotlarda bu accountlar var mı?)
            // Orijinal (Source) robotu taramaya dahil ETMİYORUZ çünkü o zaten aykırı görevi gönderen taraf.
            var outsideRobots = allActiveRobots.Where(r => r.RobotID != donorRobot.RobotID && r.RobotID != outlier.SourceRobot.RobotID).ToList();

            foreach (var robot in outsideRobots)
            {
                if (robot.IIR != null && robot.IIR.Any())
                {
                    var robotProcessIds = robot.IIR.Select(i => i.ID_Process).Distinct().ToList();

                    // Sadece riskli accountlara sahip olan süreçleri yakala
                    var matchingProcesses = lstProcess.Where(p =>
                        robotProcessIds.Contains(p.ProcessID) &&
                        riskyAccounts.Contains(p.Account)).ToList();

                    foreach (var p in matchingProcesses)
                    {
                        // Aynı işin daha önce eklenip eklenmediğini kontrol et (Duplicate önlemi)
                        if (!fixedProcessesWithSliding.Any(f => f.Process.ProcessID == p.ProcessID && f.LockedRobot.RobotID == robot.RobotID))
                        {
                            fixedProcessesWithSliding.Add((p, robot));
                        }
                    }
                }
            }

            return fixedProcessesWithSliding;
        }
    }
}