// See https://aka.ms/new-console-template for more information
using RPA.MathModel;
using RPA.TestProblemGenerator;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using RPA.DAL;
using RPA.GRASP;




//ProcessGenerator pg = new ProcessGenerator(4, 10080);
//DataTable dtProcess = pg.GenerateProcessTable(4, 4);
//DataTable dtProcessInstances = pg.GenerateProcessInstanceTable(dtProcess);

GetData gd = new GetData();
//DataTable dtProcess = gd.GetProcessData();
DataTable dtProcessInstances = gd.GetProcessInstanceData();
List<TaskProcess> lstProcess = ProcessDataLoader.LoadProcessesFromDataTable(dtProcessInstances);

// 1. Rastgele sıralanmış liste
List<TaskProcess> randomList = ProcessListGenerator.GetRandomSorted(lstProcess);

// 2. Instance sayısına göre (Büyükten Küçüğe) sıralanmış liste
List<TaskProcess> mostInstancesFirstList = ProcessListGenerator.GetSortedByInstanceCountDesc(lstProcess);

// 3. Ortalama WindowLenght değerine göre (Küçükten Büyüğe) sıralanmış liste
List<TaskProcess> shortestWindowAvgFirstList = ProcessListGenerator.GetSortedByAvgWindowLengthAsc(lstProcess);

// Ortalama ProcessingTime değerine göre (Büyükten Küçüğe) sıralanmış liste
List<TaskProcess> longestProcessingAvgFirstList = ProcessListGenerator.GetSortedByAvgProcessingTimeDesc(lstProcess);


double[] costOfSoftare = new double[3];
costOfSoftare[0] = 100;
costOfSoftare[1] = 150;
costOfSoftare[2] = 200;

//ILP model = new ILP(dtProcess, dtProcessInstances);
//model.Solve(4, 4, 3, 7, 3000, costOfSoftare, 1, 1440); //10080
//int[,] yValue = model.PrintYValue();
//int[,] xValue = model.PrintXValue();
//int[] hValue = model.PrintHValue();
//double[,] sValue= model.PrintSValue();
//double[,] ssValue = model.PrintSSValue();
//int[,,] zValue = model.PrintZValue();

//DataTable dtResult = model.resultTable(dtProcessInstances);

// Başlangıç R1 tanımlanır.
Robot rbt = new Robot();
rbt.KaynakAdi = "R1";
List<Robot> lstRobot=new List<Robot>();
lstRobot.Add(rbt);

do
{
// Ana listeden aday liste oluşturulur
// Aday listeden bir proses seçilir.

    //Uygun robot seçilir

    //foreach (Instance item in process.InstancesOfProcess)
    //{
        //Seçilen robota yerleşebilmesi kontrol edilir

        //Eğer yerleşebilirse
            //instance değerleri güncellenir
            //robot değerleri güncellenir
            
        //Eğeryerleşemez ise
            //Bir sonraki robot kontrol edilir

        //Eğer hiçbirine yerleşemez ise yeni robot oluşturularak yerleştirilir.
    //}
} while (lstRobot.Count>0);



// Kaynakları tanımla ve planlayıcıyı başlat
var resources = new List<string> { "R1", "R2", "R3" };
var scheduler = new ResourceScheduler(resources);

// R1 için ilk operasyonu ekle (360 - 420)
scheduler.AddOperation("R1", 360, 420);

// R1 için ikinci operasyonu ekle (690 - 810)
scheduler.AddOperation("R1", 690, 810);


// R1 için üçüncü operasyonu ekle (690 - 810)
scheduler.AddOperation("R1", 930, 1080);

// R1 için dördüncü operasyonu ekle (690 - 810)
scheduler.AddOperation("R1", 30, 120);

// Sonuçları al
var r1IdleTimes = scheduler.GetIdleTimes("R1");

// Çıktı Kontrolü
Console.WriteLine("R1 Boş Zamanları:");
foreach (var slot in r1IdleTimes)
{
    Console.WriteLine(slot.ToString());
}
Console.ReadLine();

// Zaman penceresi sınırları
int windowStart = 800;
int windowEnd = 1440;
string selectedResource = "R1";

// Metodu çağır
var results = WindowFilter.GetIdleTimesInWindow(selectedResource, r1IdleTimes, windowStart, windowEnd);

// Çıktıyı yazdır
Console.WriteLine($"{selectedResource} kaynağı için {windowStart}-{windowEnd} penceresindeki boşluklar:");
foreach (var result in results)
{
    Console.WriteLine(result.ToString());
}

Console.ReadLine();

