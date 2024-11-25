using System;
using System.Collections.Generic;

namespace RPA.Entities;

public partial class Process
{
    public string? IdProcess { get; set; }

    public int? ProcessingTime { get; set; }

    public int? PriorityLevel { get; set; }

    public int? RequiredSoftware1 { get; set; }

    public int? RequiredSoftware2 { get; set; }

    public int? RequiredSoftware3 { get; set; }

    public int? DepartmentOfProcess { get; set; }

    public int? AccountInfo { get; set; }

    public int? NumOfWeeklyRepet { get; set; }

    public int? TransactionDay1 { get; set; }

    public int? TransactionDay2 { get; set; }

    public int? TransactionDay3 { get; set; }

    public int? TransactionDay4 { get; set; }

    public int? TransactionDay5 { get; set; }

    public int? TransactionDay6 { get; set; }

    public int? TransactionDay7 { get; set; }

    public int? NumOfDailyRepet { get; set; }

    public string? TransactionClock1 { get; set; }

    public string? TransactionClock2 { get; set; }

    public string? TransactionClock3 { get; set; }

    public string? TransactionClock4 { get; set; }

    public string? TransactionClock5 { get; set; }

    public int? TransactionTime1 { get; set; }

    public int? TransactionTime2 { get; set; }

    public int? TransactionTime3 { get; set; }

    public int? TransactionTime4 { get; set; }

    public int? TransactionTime5 { get; set; }

    public int? WindowLenght { get; set; }
}
