﻿using System;
using System.Collections.Generic;

namespace RPA.Entities;

public partial class Instance
{
    public string? IdProcess { get; set; }

    public string? IdProcessInstance { get; set; }

    public int? ReleaseDay { get; set; }

    public int? ReleaseHour { get; set; }

    public int? ReleaseMinute { get; set; }

    public int? ReleaseTime { get; set; }

    public int? DueTime { get; set; }

    public int? ProcessingTime { get; set; }

    public int? WindowLenght { get; set; }

    public int? StartTime { get; set; }

    public int? FinishTime { get; set; }

    public int? Tardiness { get; set; }

    public int? RobotNumber { get; set; }

    public int? Department { get; set; }

    public int? Account { get; set; }

    public int? Software1 { get; set; }

    public int? Software2 { get; set; }

    public int? Software3 { get; set; }
}
