namespace RPA.LNS
{
    // 1. Sonuçları tutacak olan yeni nesnemiz
    public class RobotUsageStats
    {
        public string RobotName { get; set; }

        public string AllocatedDepartment { get; set; }
        public int NumOfIns { get; set; }

        // Yüzdelik doluluk oranı (Örn: %85.5)
        public double RateOfUsage { get; set; }
    }

    public class RobotWorkloadSorter
    {
        /// <summary>
        /// Robotları, üzerlerine atanmış Instance (işlem) sayısına göre küçükten büyüğe sıralar.
        /// Boşaltılması/Kapatılması en kolay robotları (en az işi olanları) bulmak için kullanılır.
        /// </summary>
        public static List<Robot> SortByInstanceCountAscending(List<Robot> rawRobotList)
        {
            if (rawRobotList == null || !rawRobotList.Any())
                return new List<Robot>();

            return rawRobotList
                .OrderBy(r => r.IIR != null ? r.IIR.Count : 0)
                .ToList();
        }

        /// <summary>
        /// Robotları, üzerlerine atanmış işlemlerin toplam işlem süresine (ProcessingTime) göre küçükten büyüğe sıralar.
        /// Adet olarak çok ama süre olarak çok kısa işleri olan robotları tespit etmek için kullanılır.
        /// </summary>
        public static List<Robot> SortByTotalProcessingTimeAscending(List<Robot> rawRobotList)
        {
            if (rawRobotList == null || !rawRobotList.Any())
                return new List<Robot>();

            return rawRobotList
                .OrderBy(r => r.IIR != null ? r.IIR.Sum(instance => instance.ProcessingTime) : 0)
                .ToList();
        }

        /// <summary>
        /// Her bir robotun üzerindeki işlem sayısını, tahsis bilgisini ve kapasite kullanım oranını hesaplayarak raporlar.
        /// </summary>
        /// <param name="rawRobotList">Analiz edilecek robot listesi</param>
        /// <param name="totalAvailableTime">Toplam kullanılabilir zaman (Dakika cinsinden). Varsayılan: 24 saat = 1440 dk</param>
        /// <returns>Robotların doluluk istatistiklerini içeren liste</returns>
        public static List<RobotUsageStats> GetRobotUtilizationRates(List<Robot> rawRobotList, double totalAvailableTime = 1440)
        {
            if (rawRobotList == null || !rawRobotList.Any())
                return new List<RobotUsageStats>();

            return rawRobotList.Select(r =>
            {
                int instanceCount = r.IIR != null ? r.IIR.Count : 0;
                double totalProcessingTime = r.IIR != null ? r.IIR.Sum(i => i.ProcessingTime) : 0;

                // Kullanım oranını yüzdelik olarak hesapla (0-100 arası)
                double usageRate = totalAvailableTime > 0
                    ? (totalProcessingTime / totalAvailableTime) * 100
                    : 0;

                return new RobotUsageStats
                {
                    RobotName = r.RobotName,
                    AllocatedDepartment = r.AllocatedDepartment, // Yeni eklenen eşleştirme
                    NumOfIns = instanceCount,
                    RateOfUsage = Math.Round(usageRate, 2) // Virgülden sonra 2 hane ile sınırlandır
                };
            }).ToList();
        }
    }
}
