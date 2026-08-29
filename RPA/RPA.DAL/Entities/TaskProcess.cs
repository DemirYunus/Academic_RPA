using System;
using System.Collections.Generic;

public class TaskProcess
{
    public string ProcessID { get; set; }
    public int Department { get; set; }
    public int Account { get; set; }

    // Yazılımları dinamik bir liste olarak tutuyoruz
    public List<Software> RequiredSoftwares { get; set; }

    public List<Instance> InstancesOfProcess { get; set; }

    public TaskProcess()
    {
        InstancesOfProcess = new List<Instance>();
        RequiredSoftwares = new List<Software>();
    }
}

public class Software
{
    // Eski integer değerler yerine string tabanlı özellik
    public string Name { get; set; }

    // İleride gerekirse buraya int LicenseCapacity, string Version vb. eklenebilir
}

public class Instance
{
    public string ID_Process { get; set; }
    public string ID_Process_Instance { get; set; }
    public double? ReleaseDay { get; set; }
    public int ReleaseHour { get; set; }
    public int ReleaseMinute { get; set; }
    public int ReleaseTime { get; set; }
    public int DueTime { get; set; }
    public int ProcessingTime { get; set; }

    // Not: Sütun adı Excel dosyasında yazıldığı şekliyle (Lenght) bırakılmıştır.
    public int WindowLenght { get; set; }

    public double? StartTime { get; set; }
    public double? FinishTime { get; set; }
    public double? Tardiness { get; set; }
    public double? RobotNumber { get; set; }
}