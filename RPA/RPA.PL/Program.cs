// See https://aka.ms/new-console-template for more information
using RPA.TestProblemGenerator;
using System.ComponentModel;



Console.WriteLine("Hello, World!");

ProcessGenerator pg = new ProcessGenerator(10);
pg.GenerateProcessTable();
Console.ReadLine();

