using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

public class ProcessDataLoader
{
    public static List<TaskProcess> LoadProcessesFromDataTable(DataTable dt)
    {
        // Process'leri ID'ye göre gruplayarak tutmak için sözlük (Dictionary) kullanıyoruz
        var processDict = new Dictionary<string, TaskProcess>();

        foreach (DataRow row in dt.Rows)
        {
            // ID_Process hücresi boşsa o satırı atla
            if (row["ID_Process"] == DBNull.Value || string.IsNullOrWhiteSpace(row["ID_Process"].ToString()))
            {
                continue;
            }

            string processId = row["ID_Process"].ToString();

            // Eğer processDict içinde bu ProcessID henüz yoksa ana nesneyi oluştur
            if (!processDict.ContainsKey(processId))
            {
                var newProcess = new TaskProcess
                {
                    ProcessID = processId,
                    Department = row["Department"] != DBNull.Value ? Convert.ToInt32(row["Department"]) : 0,
                    Account = row["Account"] != DBNull.Value ? Convert.ToInt32(row["Account"]) : 0,
                    Software1 = row["Software-1"] != DBNull.Value ? Convert.ToInt32(row["Software-1"]) : 0,
                    Software2 = row["Software-2"] != DBNull.Value ? Convert.ToInt32(row["Software-2"]) : 0,
                    Software3 = row["Software-3"] != DBNull.Value ? Convert.ToInt32(row["Software-3"]) : 0
                };
                processDict.Add(processId, newProcess);
            }

            // İlgili satırdaki Instance bilgilerini doldur
            var newInstance = new Instance
            {
                ID_Process_Instance = row["ID_Process_Instance"] != DBNull.Value ? row["ID_Process_Instance"].ToString() : string.Empty,

                ReleaseDay = row["ReleaseDay"] != DBNull.Value ? Convert.ToDouble(row["ReleaseDay"]) : (double?)null,
                ReleaseHour = row["ReleaseHour"] != DBNull.Value ? Convert.ToInt32(row["ReleaseHour"]) : 0,
                ReleaseMinute = row["ReleaseMinute"] != DBNull.Value ? Convert.ToInt32(row["ReleaseMinute"]) : 0,
                ReleaseTime = row["ReleaseTime"] != DBNull.Value ? Convert.ToInt32(row["ReleaseTime"]) : 0,
                DueTime = row["DueTime"] != DBNull.Value ? Convert.ToInt32(row["DueTime"]) : 0,
                ProcessingTime = row["ProcessingTime"] != DBNull.Value ? Convert.ToInt32(row["ProcessingTime"]) : 0,
                WindowLenght = row["WindowLenght"] != DBNull.Value ? Convert.ToInt32(row["WindowLenght"]) : 0,

                // Boş gelebilecek diğer alanlar
                StartTime = row["StartTime"] != DBNull.Value ? Convert.ToDouble(row["StartTime"]) : (double?)null,
                FinishTime = row["FinishTime"] != DBNull.Value ? Convert.ToDouble(row["FinishTime"]) : (double?)null,
                Tardiness = row["Tardiness"] != DBNull.Value ? Convert.ToDouble(row["Tardiness"]) : (double?)null,
                RobotNumber = row["RobotNumber"] != DBNull.Value ? Convert.ToDouble(row["RobotNumber"]) : (double?)null
            };

            // İlgili Process objesinin Instances listesine yeni instance'ı ekle
            processDict[processId].InstancesOfProcess.Add(newInstance);
        }

        // Sözlükteki tüm Process nesnelerini bir liste olarak döndür
        return processDict.Values.ToList();
    }
}