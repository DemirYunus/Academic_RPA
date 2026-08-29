using System;
using System.Collections.Generic;
using System.Linq;

namespace RPA.OTC_CP
{
    // 1. TAŞIYICI VERİ MODELİ
    public class OutlierInfo
    {
        public Robot SourceRobot { get; set; }
        public TaskProcess OutlierProcess { get; set; }
        public string WastefulSoftwareName { get; set; }
        public double SoftwareCost { get; set; }
    }

    // 2. SIZINTI RADARI
    public class OutlierDetector
    {
        /// <summary>
        /// Robotları tarar, %20 ve altı frekansa sahip yazılımları kullanan "Aykırı TaskProcess'leri" tespit eder.
        /// En pahalı yazılım sızıntısından başlayarak (Azalan sırada) listeler.
        /// </summary>
        public static List<OutlierInfo> DetectOutliers(List<Robot> robotList, List<TaskProcess> lstProcess, double thresholdRatio = 0.20)
        {
            List<OutlierInfo> outlierTasks = new List<OutlierInfo>();

            // Sadece içine iş atanmış aktif robotları tara
            var activeRobots = robotList.Where(r => r.IIR != null && r.IIR.Count > 0).ToList();

            foreach (var robot in activeRobots)
            {
                var robotProcessIds = robot.IIR.Select(i => i.ID_Process).Distinct().ToList();
                var processesOnRobot = lstProcess.Where(p => robotProcessIds.Contains(p.ProcessID)).ToList();

                if (!processesOnRobot.Any()) continue;

                // Adım 1: Bu robotta hangi yazılım toplam kaç farklı TaskProcess tarafından kullanılıyor?
                Dictionary<string, int> softwareFrequencies = new Dictionary<string, int>();

                foreach (var process in processesOnRobot)
                {
                    if (process.RequiredSoftwares != null)
                    {
                        foreach (var sw in process.RequiredSoftwares)
                        {
                            string swName = sw.Name.Trim().ToLower();
                            if (!softwareFrequencies.ContainsKey(swName))
                            {
                                softwareFrequencies[swName] = 0;
                            }
                            softwareFrequencies[swName]++;
                        }
                    }
                }

                // Adım 2: Frekans kontrolü ve Aykırı Görevlerin (Outlier TaskProcess) işaretlenmesi
                foreach (var swFreq in softwareFrequencies)
                {
                    string currentSwName = swFreq.Key;
                    int usageCount = swFreq.Value;

                    // Toplam işlem çeşidi (TaskProcess) içindeki kullanım oranı hesabı
                    double usageRatio = (double)usageCount / processesOnRobot.Count;

                    // Eğer kullanım oranı eşik değerinden küçük veya eşitse (Örn: %20)
                    if (usageRatio <= thresholdRatio)
                    {
                        var outlierProcesses = processesOnRobot
                            .Where(p => p.RequiredSoftwares != null &&
                                        p.RequiredSoftwares.Any(s => s.Name.Trim().ToLower() == currentSwName))
                            .ToList();

                        foreach (var outProcess in outlierProcesses)
                        {
                            // Aynı TaskProcess daha önce başka bir yazılım yüzünden listeye eklendiyse tekrar ekleme
                            if (!outlierTasks.Any(o => o.OutlierProcess.ProcessID == outProcess.ProcessID && o.SourceRobot.RobotID == robot.RobotID))
                            {
                                outlierTasks.Add(new OutlierInfo
                                {
                                    SourceRobot = robot,
                                    OutlierProcess = outProcess,
                                    WastefulSoftwareName = currentSwName,
                                    SoftwareCost = GetSoftwareCost(currentSwName)
                                });
                            }
                        }
                    }
                }
            }

            // En pahalı yazılım lisansından kurtarmak için Descending (Azalan) sırala
            return outlierTasks.OrderByDescending(o => o.SoftwareCost).ToList();
        }

        // Yazılım lisans maliyetlerini döndüren statik metot 
        private static double GetSoftwareCost(string softwareName)
        {
            if (softwareName == "sw1") return 100;
            if (softwareName == "sw2") return 150;
            if (softwareName == "sw3") return 200;
            return 0;
        }
    }
}