using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace RPA.LNS_MD
{
    public class Solver
    {
        public static List<Robot> SolveLNS_MD(List<Robot> rawRobotList, List<TaskProcess> lstProcess)
        {
            ALNSManager alnsManager = new ALNSManager();

            int maxNonImprovingIterations = 10;
            int nonImprovingCount = 0;

            int bestCost = CalculateObjectiveCost(rawRobotList);
            List<Robot> bestRobotList = DeepCopy(rawRobotList);

            while (nonImprovingCount < maxNonImprovingIterations)
            {
                List<Robot> currentRobotList = DeepCopy(rawRobotList);
                int currentCost = CalculateObjectiveCost(currentRobotList);

                // --- FAZ 1: DEPARTMAN KISITLI İŞLERİN OPTİMİZASYONU (Dept > 0) ---
                // Dün karar verdiğimiz gibi: Önce departman bazlı söküp tamir ediyoruz.
                bool deptPhaseSuccess = RunDepartmentPhase(ref currentRobotList, lstProcess);

                if (!deptPhaseSuccess)
                {
                    // Eğer departman fazı tıkandıysa bu iterasyonu es geç
                    nonImprovingCount++;
                    continue;
                }

                // --- FAZ 2: EVRENSEL İŞLERİN OPTİMİZASYONU (Dept = 0) ---
                // En az yüklü 2 + 1 rastgele robot sökülür, 7'li alt küme ile başlanıp 12'ye çıkılır.
                bool universalPhaseSuccess = RunUniversalPhase(ref currentRobotList, lstProcess);

                if (!universalPhaseSuccess)
                {
                    nonImprovingCount++;
                    continue;
                }

                // --- MALİYET VE KABUL DEĞERLENDİRMESİ ---
                int newCost = CalculateObjectiveCost(currentRobotList);
                int operatorScore = 0;

                if (newCost < bestCost)
                {
                    bestCost = newCost;
                    bestRobotList = DeepCopy(currentRobotList);
                    rawRobotList = currentRobotList;
                    operatorScore = 3;
                    nonImprovingCount = 0; // İyileşme var, sayaç sıfırlandı
                }
                else if (newCost <= currentCost)
                {
                    rawRobotList = currentRobotList;
                    operatorScore = 1;
                    nonImprovingCount++;
                }
                else
                {
                    operatorScore = 0;
                    nonImprovingCount++;
                }
            }

            return bestRobotList;
        }

        private static int CalculateObjectiveCost(List<Robot> robots)
        {
            return robots.Count(r => r.IIR != null && r.IIR.Count > 0);
        }

        private static List<Robot> DeepCopy(List<Robot> originalList)
        {
            string json = JsonSerializer.Serialize(originalList);
            return JsonSerializer.Deserialize<List<Robot>>(json);
        }

        // --- FAZ METOTLARI İSKELETİ ---
        private static bool RunDepartmentPhase(ref List<Robot> robotList, List<TaskProcess> lstProcess)
        {
            // 1. Söküm (Destruction)
            List<Robot> emptiedRobots;
            List<TaskProcess> relocatedProcesses = DestructionOperators.Phase1_DepartmentRemoval(robotList, lstProcess, out emptiedRobots);

            // Sökülecek bir şey yoksa başarılı say, Faz-2'ye geç
            if (relocatedProcesses == null || !relocatedProcesses.Any())
            {
                return true;
            }

            // 2. Onarım (Repair - Account Radarı ve Kademeli Sıkıştırma)
            bool isRepaired = RepairOperators.Phase1_DepartmentRepair(
                relocatedProcesses,
                emptiedRobots,
                robotList,
                lstProcess
            );

            return isRepaired;
        }

        private static bool RunUniversalPhase(ref List<Robot> robotList, List<TaskProcess> lstProcess)
        {
            List<Robot> emptiedRobots;

            // 1. Yıkım
            List<TaskProcess> relocatedProcesses = DestructionOperators.Phase2_UniversalRemoval(robotList, lstProcess, out emptiedRobots);

            if (relocatedProcesses == null || !relocatedProcesses.Any())
            {
                return true; // Sökülecek evrensel iş yok
            }

            // 2. Onarım (Alt Küme / Fanus Mantığı)
            bool isRepaired = RepairOperators.Phase2_UniversalRepair(
                relocatedProcesses,
                emptiedRobots,
                robotList,
                lstProcess,
                5 // Başlangıç alt küme boyutu 5 olarak ayarlandı. Burası parametre olarak değiştirilebilir. !!
            );

            return isRepaired;
        }
    }
}
