// See https://aka.ms/new-console-template for more information
using RPA.DAL;
using RPA.GRASP;
using RPA.MathModel;
using RPA.TestProblemGenerator;
using RPA.LNS;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using static System.Runtime.InteropServices.JavaScript.JSType;
using Google.OrTools.ConstraintSolver;



#region Veri Çekme

//ProcessGenerator pg = new ProcessGenerator(4, 10080);
//DataTable dtProcess = pg.GenerateProcessTable(4, 4);
//DataTable dtProcessInstances = pg.GenerateProcessInstanceTable(dtProcess);

GetData gd = new GetData();
//DataTable dtProcess = gd.GetProcessData();
DataTable dtProcessInstances = gd.GetProcessInstanceData();


double[] costOfSoftare = new double[3];
costOfSoftare[0] = 100;
costOfSoftare[1] = 150;
costOfSoftare[2] = 200;

#endregion

Console.WriteLine("Çözüm Yöntemi (1:MIP, 2: Hibrit Sezgisel)");  
int solutionMethod = int.Parse(Console.ReadLine());

if (solutionMethod == 1)
{
    #region MIP
    ILP model = new ILP(dtProcessInstances);
    model.Solve(4, 4, 3, 15, 3000, costOfSoftare, 1, 1440); //10080
    int[,] yValue = model.PrintYValue();
    int[,] xValue = model.PrintXValue();
    int[] hValue = model.PrintHValue();
    double[,] sValue = model.PrintSValue();
    double[,] ssValue = model.PrintSSValue();
    int[,,] zValue = model.PrintZValue();

    DataTable dtResult = model.resultTable(dtProcessInstances);
    #endregion
}
else
{
    #region Hibrit Sezgisel

    // Ana başlangıç verileri
    int maxIterations = 30; // Algoritmanın kaç tur çalışacağı
    BestSolutionState bestSolution = new BestSolutionState();
    bestSolution.MinCost = double.MaxValue; // Başlangıçta sonsuz kabul edelim

    Stopwatch stopwatch = new Stopwatch();
    // Grafiğe basılacak X (İterasyon) ve Y (En İyi Maliyet) eksenleri
    List<double> iterationX = new List<double>();
    List<double> bestCostY = new List<double>();

    //TaskProses Sıralama: GetSortedByAvgWindowLengthAsc
    // Aday TaskProses Seçimi: SelectByProcessingTime_ValueBased
    // CandidateListe Alpha:0.5
    // Aday Robot Sıralama Tipi: 7+1
    // Aday Robot Seçimi: 1
    // Robot Boşluk Seçimi: 3
    // Robot Boşluk Hizalama: 3

    // Hibrit Sezgisel Ana Döngüsü
    stopwatch.Start(); // Kronometreyi başlat
    for (int iteration = 1; iteration <= maxIterations; iteration++)
    {
        List<TaskProcess> lstProcess = ProcessDataLoader.LoadProcessesFromDataTable(dtProcessInstances);

        //GRASP
        List<Robot> rawRobotList = RPA.GRASP.Solver.SolveGRASP(lstProcess);
        double currentCostBeforeLNS = SolutionEvaluator.CalculateCost(rawRobotList, lstProcess);

        // CP-based Large Neighborhood Search (LNS)
        //LNS uygulanacak mı?
        //var usageReport = RobotWorkloadSorter.GetRobotUtilizationRates(rawRobotList);


        //RPA.LNS.Solve.SolveLNS(rawRobotList, lstProcess);
        List<Robot> robotListAfterLNS = RPA.LNS_MD.Solver.SolveLNS_MD(rawRobotList, lstProcess);

        // ==========================================
        // YENİ OTC-CP (YAZILIM KONSOLİDASYONU) ÇAĞRISI
        // ==========================================
        RPA.OTC_CP.Solver.SolveOTC_CP(robotListAfterLNS, lstProcess);

        // 3. LNS SONU: Maliyeti Hesapla ve Kaydet
        double currentCostAfterLNS = SolutionEvaluator.CalculateCost(robotListAfterLNS, lstProcess);


        // Yeni bir en düşük maliyet bulunduysa state güncellenir
        if (currentCostAfterLNS < bestSolution.MinCost)
        {
            bestSolution.MinCost = currentCostAfterLNS;
            bestSolution.BestIteration = iteration;
            // EN İYİ ÇÖZÜMÜN BULUNDUĞU SANIYEYİ YAKALA
            bestSolution.BestTimeInSeconds = stopwatch.Elapsed.TotalSeconds;

            // DİKKAT: rawRobotList objesinden türetilen DataTable'ın .Copy() metodu ile 
            // kopyalanması şarttır. Aksi halde sonraki iterasyonlar bu tabloyu bozar.
            DataTable currentResultTable = ResultHelper.ConvertToDataTable(rawRobotList, lstProcess); // Kendi dönüşüm metodunuz
            bestSolution.ResultTable = currentResultTable.Copy();
        }

        // GRAFİK İÇİN KAYIT (Döngünün en sonu):
        // Dikkat: currentCostAfterLNS değil, o ana kadar ulaşılan EN İYİ maliyeti kaydediyoruz!
        iterationX.Add(iteration);
        bestCostY.Add(bestSolution.MinCost);

        Console.WriteLine($"İterasyon: {iteration} | Zaman: {stopwatch.Elapsed.TotalSeconds:F2} sn | En İyi Maliyet: {bestSolution.MinCost}");
    }
    // Döngü bitti, kronometreyi durdur
    stopwatch.Stop();

    // Döngü bittikten sonra eklenecek kod:
    Console.WriteLine("Yakınsama grafiği çiziliyor...");

    ScottPlot.Plot plt = new ScottPlot.Plot();

    // Çizgi grafiğini oluştur (X ve Y listelerini diziye çevirerek)
    var scatter = plt.Add.Scatter(iterationX.ToArray(), bestCostY.ToArray());
    scatter.LineWidth = 2; // Çizgi kalınlığı

    // Eksen İsimlendirmeleri
    plt.Title("LNS Hibrit Algoritması Yakınsama Eğrisi");
    plt.XLabel("İterasyon Sayısı");
    plt.YLabel("En İyi Maliyet (Cost)");

    // Grafiği PNG olarak kaydet
    string chartPath = "YakinsamaGrafigi.png";
    plt.SavePng(chartPath, 800, 500);


    Console.WriteLine($"\nTAMAMLANDI!");
    Console.WriteLine($"Minimum maliyet: {bestSolution.MinCost}");
    Console.WriteLine($"En iyi çözüme {bestSolution.BestIteration}. iterasyonda, tam {bestSolution.BestTimeInSeconds:F2} saniyede ulaşıldı.");
    Console.WriteLine($"Grafik kaydedildi: {chartPath}");

    #endregion
}








