using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RPA.GRASP
{
    public class Solver
    {
        public static List<Robot> SolveGRASP(List<TaskProcess> lstProcess)
        {
            #region Başlangıç           

            // Ana listeden aday liste oluşturulur
            // Ortalama ProcessingTime değerine göre (Büyükten Küçüğe) sıralanmış liste
            // Farklı listeleme seçenekleri de bulunuyor. Örn: SortByDueTimeAscending, SortByReleaseTimeAscending, SortByProcessingTimeDescending..  
            List<TaskProcess> lstOrderedTaskProses = ProcessListGenerator.MostConstrainedVariableFirstV4(lstProcess);

            // Başlangıç R1 tanımlanır.
            List<Robot> rawRobotList = new List<Robot>();
            rawRobotList.Add(new Robot { RobotID = 1, RobotName = "R1", AllocatedDepartment = "0" });//Hem aday robot listesine ekleme yapılıyor. 

            #endregion

            do
            {
                #region Aday TaskProses Seçimi

                // Aday listeden bir proses seçilir.
                GraspCandidateSelector slct = new GraspCandidateSelector();
                TaskProcess candidateTaskProcess = slct.SelectByMacroTierAndTightness_ValueBased(lstOrderedTaskProses, 0.3);

                #endregion

                #region Uygun Robot Listesi Oluşturma

                //Uygun robot listesi oluşturulur
                // İlgili TaskProcess'in Department ve Account bilgilerini verdiğimizi varsayıyoruz.
                RobotListGenerator robotListGenerator = new RobotListGenerator(candidateTaskProcess.Department, candidateTaskProcess.Account, candidateTaskProcess.RequiredSoftwares, rawRobotList);

                List<Robot> candidateRobots = new List<Robot>();

                if (candidateTaskProcess.Department != 0)//Departman bilgisi var ise
                {
                    //Department bilgisi olan prosesler için, department bilgisi ile eşleşen robotlar seçiliyor.   
                    candidateRobots = robotListGenerator.SortByAllocatedDepartment(RobotSortRule.SortByUtilizationDescending);
                }
                else
                {
                    candidateRobots = robotListGenerator.SortByUtilizationDescending();
                    //if (candidateTaskProcess.RequiredSoftwares.Count > 0)
                    //{
                    //    //Software bilgisi olan prosesler için, software bilgisi ile eşleşen robotlar öncelikli olarak seçiliyor.
                    //    candidateRobots = robotListGenerator.SortByLoadedSoftware(RobotSortRule.IdleWindowAverageDurationAscending);
                    //}
                    //else
                    //{
                    //    candidateRobots = robotListGenerator.SortByUtilizationAscending();
                    //}
                }

                //Şartlara uyan robot yok ise yeni bir robot oluşturuluyor
                if (candidateRobots.Count == 0)
                {
                    int id = rawRobotList.Max(r => r.RobotID);
                    string name = "R" + (id + 1).ToString();
                    Robot newRobot = new Robot { RobotID = id + 1, RobotName = name, AllocatedDepartment = "0" };

                    // KRİTİK NOKTA: Yeni robot doğrudan ana listeye eklenmeli
                    rawRobotList.Add(newRobot);

                    // O anki işlemlere devam edebilmek için aday listeye de eklenir
                    candidateRobots.Add(newRobot);
                }

                #endregion

                #region Uygun Robot Seçimi ve Zaman Penceresi Belirleme

                // 1. MAKRO SEÇİM (Küresel En İyi Robotu Bulma - Global Best-Fit)
                // Yukarıda (RobotListGenerator ile) aday robotlar doluluk oranlarına göre sıralandı, henüz kesin seçim yapılmadı.
                // SelectGlobalBestFit metodu: İlk bulduğu robotta durmaz! Tüm aday robotları ve içlerindeki tüm olası boşlukları tarar. 
                // İlgili TaskProcess'in Account ve süre kısıtlarını sağlayan, ve işi yerleştirdiğimizde geriye 
                // EN AZ BOŞLUK BIRAKAN (minimum residual slack) en mükemmel robotu tespit eder.
                // Bulduğu bu en iyi robotu ve üzerindeki olası zaman boşluklarını (bir sonraki adımda cımbızlanmak üzere) döndürür.
                RobotSelectionResult robotTimeWindowResult = RobotSelection.SelectGlobalBestFit(candidateTaskProcess, candidateRobots, lstOrderedTaskProses);

                #endregion

                #region Atama ve Güncelleme

                if (robotTimeWindowResult != null) // Eşleşme bulundu. Uygun robot ve zaman pencereleri bulundu.
                {
                    // 2. MİKRO SEÇİM (En Uygun Zaman Penceresini Bulma)
                    // Seçilen robotun üzerinde birden fazla olası boşluk olabilir.
                    // SelectBestFit: Bu boşluklar arasından (örn: artığı en aza indiren) en iyi (Best Fit) pencereyi seçer.
                    RobotSelectionResult selectedRobotTimeWindowResult = WindowSelection.SelectBestFit(robotTimeWindowResult);

                    // Zaman Çizelgeleme (Scheduling) atamaları gerçekleştirilebilir...

                    // 1. Rastgele bir hizalama stratejisi belirle
                    AlignmentStrategy randomStrategy = TaskAssigner.GetRandomAlignmentStrategy();
                    // Atamayı yap ve Nesneleri Güncelle (Örneğin: Sola dayalı olarak)
                    TaskAssigner.AssignAndUpdate(candidateTaskProcess, selectedRobotTimeWindowResult, randomStrategy);

                    lstOrderedTaskProses.Remove(candidateTaskProcess); // İşlem tamamlandıktan sonra ana listeden kaldırılır.   
                }
                else
                {
                    // Aday robotların hiçbiri bu prosesi karşılamıyor.
                    // Yeni robot oluşturuluyor ve listeye ekleniyor.

                    // 1. Yeni robotu güvenli bir şekilde oluştur (Liste boşsa hata vermemesi için Any kontrolü)
                    int newId = rawRobotList.Max(r => r.RobotID);
                    string name = "R" + (newId + 1).ToString();
                    Robot newRobot = new Robot { RobotID = newId + 1, RobotName = name, AllocatedDepartment = "0" };

                    // KRİTİK NOKTA: Yeni robot doğrudan ana listeye eklenmeli
                    rawRobotList.Add(newRobot);

                    // Oluşturulan robotu listeye ekle
                    candidateRobots.Add(newRobot);

                    // 2. Yeni robot için uygunluk kontrolü yap.
                    // DİKKAT: Robot boş olsa bile 'Account' kısıtları nedeniyle zaman çakışması olabileceğinden 
                    // filtreden geçirmek ZORUNLUDUR. (allTaskProcesses listenizin erişilebilir olduğunu varsayıyoruz)
                    ProcessFeasibilityResult newRobotResult = WindowFilter.CheckRobotFeasibilityForProcess(candidateTaskProcess, newRobot, lstOrderedTaskProses);

                    if (newRobotResult.IsFeasible)
                    {
                        // 3. Uygun boşlukları seç ve atamayı yap
                        RobotSelectionResult newRobotTimeWindowResult = new RobotSelectionResult(newRobot, newRobotResult);
                        RobotSelectionResult newSelectedRobotTimeWindowResult = WindowSelection.SelectBestFit(newRobotTimeWindowResult);

                        TaskAssigner.AssignAndUpdate(candidateTaskProcess, newSelectedRobotTimeWindowResult, AlignmentStrategy.LeftAligned);

                        lstOrderedTaskProses.Remove(candidateTaskProcess);
                    }
                    else
                    {
                        // 4. KRİTİK DURUM: Eğer proses yepyeni ve bomboş bir robota bile yerleşemiyorsa, 
                        // ya 'Account' kısıtı nedeniyle global ufukta hiç yer kalmamıştır 
                        // ya da prosesin işlem süresi izin verilen pencereden daha büyüktür.
                        // Sonsuz döngüyü önlemek için proses listeden çıkarılmalı veya hata fırlatılmalıdır.

                        //Console.WriteLine("İterasyon No:" + iteration.ToString() + "/ n" + "UYARI: " + candidateTaskProcess.ProcessID + " prosesi kapasite veya hesap çakışması nedeniyle yeni robota bile atanamadı!");
                        //lstOrderedTaskProses.Remove(candidateTaskProcess);
                    }
                }

                #endregion

            } while (lstOrderedTaskProses.Count > 0);
            return rawRobotList; // Tüm prosesler atandıktan sonra, robot listesi döndürülür.
        }
    }
}
