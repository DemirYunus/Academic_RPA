using System;
using System.Collections.Generic;
using System.Linq;

// Filtreleme sonucunu ve uygun boşlukları taşıyacak özel dönüş sınıfları
public class ProcessFeasibilityResult
{
    public bool IsFeasible { get; set; }
    public List<InstanceWindowResult> InstanceWindows { get; set; }

    public ProcessFeasibilityResult()
    {
        InstanceWindows = new List<InstanceWindowResult>();
    }
}

public class InstanceWindowResult
{
    public string ID_Process_Instance { get; set; }
    public List<IdleWindow> AvailableWindows { get; set; }

    public InstanceWindowResult()
    {
        AvailableWindows = new List<IdleWindow>();
    }
}

public class WindowFilter
{    
    public static ProcessFeasibilityResult CheckRobotFeasibilityForProcess(TaskProcess taskProcess, Robot robot, List<TaskProcess> allTaskProcesses)
    {
        var result = new ProcessFeasibilityResult { IsFeasible = true };

        if (taskProcess.InstancesOfProcess == null || taskProcess.InstancesOfProcess.Count == 0)
        {
            result.IsFeasible = false;
            return result;
        }

        // 1. Adım: Eğer Account 0 değilse, bu hesabın sistem genelindeki tüm "bloke edilmiş" zamanlarını bul.
        List<IdleWindow> blockedAccountWindows = GetBlockedWindowsForAccount(taskProcess.Account, allTaskProcesses);

        foreach (var instance in taskProcess.InstancesOfProcess)
        {
            int windowStart = instance.ReleaseTime;
            int windowEnd = instance.DueTime;
            int requiredTime = instance.ProcessingTime;

            var validWindows = new List<IdleWindow>();

            // Liste boş veya null ise tüm ufuk (0-1440) boş kabul edilir
            if (robot.LstIdleWindow == null || !robot.LstIdleWindow.Any())
            {
                robot.LstIdleWindow = new List<IdleWindow> { new IdleWindow(0, 1440) };
            }

            if (robot.LstIdleWindow != null)
            {
                foreach (var slot in robot.LstIdleWindow)
                {
                    if (slot.End <= windowStart || slot.Start >= windowEnd) continue;

                    // Kesişen bölgeyi hesapla (Kırpma)
                    int clippedStart = Math.Max(slot.Start, windowStart);
                    int clippedEnd = Math.Min(slot.End, windowEnd);

                    if (clippedEnd <= clippedStart) continue; // Geçersiz aralık

                    // 2. Adım: Kırpılmış robot boşluğundan, hesaba ait bloke zamanları çıkar (Fark Kümesi)
                    // Başlangıçta tek parça olan boşluğumuz, bloke zamanlara denk gelirse bölünebilir.
                    var fragments = new List<IdleWindow> { new IdleWindow(clippedStart, clippedEnd) };

                    foreach (var block in blockedAccountWindows)
                    {
                        var nextFragments = new List<IdleWindow>();
                        foreach (var frag in fragments)
                        {
                            // Eğer çakışma yoksa parçayı aynen koru
                            if (block.End <= frag.Start || block.Start >= frag.End)
                            {
                                nextFragments.Add(frag);
                            }
                            else
                            {
                                // Çakışma var, boşluk parçasını bloke alanın soluna ve/veya sağına göre ikiye böl
                                if (frag.Start < block.Start)
                                {
                                    nextFragments.Add(new IdleWindow(frag.Start, block.Start));
                                }
                                if (frag.End > block.End)
                                {
                                    nextFragments.Add(new IdleWindow(block.End, frag.End));
                                }
                            }
                        }
                        fragments = nextFragments; // Güncellenmiş parçalarla yola devam et
                    }

                    // 3. Adım: Bloke zamanlar çıkarıldıktan sonra elde kalan temiz parçaları kontrol et
                    foreach (var frag in fragments)
                    {
                        if ((frag.End - frag.Start) >= requiredTime)
                        {
                            validWindows.Add(frag);
                        }
                    }
                }
            }

            if (validWindows.Count == 0)
            {
                result.IsFeasible = false;
                result.InstanceWindows.Clear();
                return result;
            }

            result.InstanceWindows.Add(new InstanceWindowResult
            {
                ID_Process_Instance = instance.ID_Process_Instance,
                AvailableWindows = validWindows
            });
        }

        return result;
    }

    // Sisteme atanmış (StartTime ve FinishTime'ı belli olan) aynı Account'a sahip işlemlerin aralıklarını getirir.
    private static List<IdleWindow> GetBlockedWindowsForAccount(int accountId, List<TaskProcess> allTaskProcesses)
    {
        var blocked = new List<IdleWindow>();

        // Account 0 ise veya liste yoksa hesap bazlı kısıt yoktur.
        if (accountId == 0 || allTaskProcesses == null) return blocked;

        // Tüm süreçlerdeki ilgili account'a ait ve atanmış/başlamış instance'ları bul
        foreach (var tp in allTaskProcesses.Where(t => t.Account == accountId))
        {
            foreach (var inst in tp.InstancesOfProcess)
            {
                if (inst.StartTime.HasValue && inst.FinishTime.HasValue)
                {
                    blocked.Add(new IdleWindow((int)inst.StartTime.Value, (int)inst.FinishTime.Value));
                }
            }
        }

        if (!blocked.Any()) return blocked;

        // Optimizasyon: Üst üste binen bloke zamanları tek bir blok haline getirerek hesaplama yükünü azaltırız.
        var sortedBlocked = blocked.OrderBy(b => b.Start).ToList();
        var mergedBlocked = new List<IdleWindow>();
        var current = sortedBlocked.First();

        foreach (var b in sortedBlocked.Skip(1))
        {
            if (b.Start <= current.End) // Kesişim var, bloğu genişlet
            {
                current.End = Math.Max(current.End, b.End);
            }
            else // Kesişim yok, yeni bloğa geç
            {
                mergedBlocked.Add(current);
                current = b;
            }
        }
        mergedBlocked.Add(current);

        return mergedBlocked;
    }
}