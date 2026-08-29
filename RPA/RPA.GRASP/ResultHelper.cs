using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

public class ResultHelper
{
    public static DataTable ConvertToDataTable(List<Robot> rawRobotList, List<TaskProcess> lstProcess)
    {
        DataTable dt = new DataTable("BestResult");

        dt.Columns.Add("ID_Process_Instance", typeof(string));
        dt.Columns.Add("ReleaseDay", typeof(double));
        dt.Columns.Add("ReleaseHour", typeof(int));
        dt.Columns.Add("ReleaseMinute", typeof(int));
        dt.Columns.Add("ReleaseTime", typeof(int));
        dt.Columns.Add("DueTime", typeof(int));
        dt.Columns.Add("ProcessingTime", typeof(int));
        dt.Columns.Add("WindowLenght", typeof(int));
        dt.Columns.Add("StartTime", typeof(double));
        dt.Columns.Add("FinishTime", typeof(double));
        dt.Columns.Add("Tardiness", typeof(double));
        dt.Columns.Add("RobotNumber", typeof(double));
        dt.Columns.Add("AllocatedDepartment", typeof(string));

        // Yeni eklenen sütunlar
        dt.Columns.Add("Account", typeof(int));
        dt.Columns.Add("RequiredSoftware", typeof(string));
        dt.Columns.Add("RobotSoftware", typeof(string));

        foreach (var robot in rawRobotList)
        {
            // Robot üzerindeki yazılımları virgülle ayırarak tek satır metne çeviriyoruz
            string robotSoftwareStr = robot.LoadedSoftware != null && robot.LoadedSoftware.Any()
                ? string.Join(", ", robot.LoadedSoftware.Select(s => s.Name))
                : string.Empty;

            foreach (var instance in robot.IIR)
            {
                DataRow row = dt.NewRow();

                // İlgili Instance'ın bağlı olduğu TaskProcess'i buluyoruz (Account ve Gereken Yazılımlar için)
                var process = lstProcess.FirstOrDefault(p => p.ProcessID == instance.ID_Process);

                row["ID_Process_Instance"] = !string.IsNullOrEmpty(instance.ID_Process_Instance) ? (object)instance.ID_Process_Instance : DBNull.Value;
                row["ReleaseDay"] = instance.ReleaseDay.HasValue ? (object)instance.ReleaseDay.Value : DBNull.Value;
                row["ReleaseHour"] = instance.ReleaseHour;
                row["ReleaseMinute"] = instance.ReleaseMinute;
                row["ReleaseTime"] = instance.ReleaseTime;
                row["DueTime"] = instance.DueTime;
                row["ProcessingTime"] = instance.ProcessingTime;
                row["WindowLenght"] = instance.WindowLenght;
                row["StartTime"] = instance.StartTime.HasValue ? (object)instance.StartTime.Value : DBNull.Value;
                row["FinishTime"] = instance.FinishTime.HasValue ? (object)instance.FinishTime.Value : DBNull.Value;
                row["Tardiness"] = instance.Tardiness.HasValue ? (object)instance.Tardiness.Value : DBNull.Value;
                row["RobotNumber"] = instance.RobotNumber.HasValue ? (object)instance.RobotNumber.Value : (object)(double)robot.RobotID;
                row["AllocatedDepartment"] = !string.IsNullOrEmpty(robot.AllocatedDepartment) ? (object)robot.AllocatedDepartment : DBNull.Value;

                // Yeni eklenen Account ve Software verilerinin atanması
                if (process != null)
                {
                    row["Account"] = process.Account;

                    row["RequiredSoftware"] = process.RequiredSoftwares != null && process.RequiredSoftwares.Any()
                        ? (object)string.Join(", ", process.RequiredSoftwares.Select(s => s.Name))
                        : DBNull.Value;
                }
                else
                {
                    row["Account"] = DBNull.Value;
                    row["RequiredSoftware"] = DBNull.Value;
                }

                row["RobotSoftware"] = !string.IsNullOrEmpty(robotSoftwareStr) ? (object)robotSoftwareStr : DBNull.Value;

                dt.Rows.Add(row);
            }
        }

        return dt;
    }
}