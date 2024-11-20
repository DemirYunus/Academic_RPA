// See https://aka.ms/new-console-template for more information
using RPA.TestProblemGenerator;
using System.ComponentModel;
using System.Data;



Console.WriteLine("Hello, World!");

ProcessGenerator pg = new ProcessGenerator(10);
DataTable dtProcess = pg.GenerateProcessTable(4, 4);
DataTable dtProcessInstances = pg.GenerateProcessInstanceTable(dtProcess);
Console.ReadLine();

