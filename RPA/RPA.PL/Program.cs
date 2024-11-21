// See https://aka.ms/new-console-template for more information
using RPA.MathModel;
using RPA.TestProblemGenerator;
using System.ComponentModel;
using System.Data;



ProcessGenerator pg = new ProcessGenerator(10);
DataTable dtProcess = pg.GenerateProcessTable(4, 4);
DataTable dtProcessInstances = pg.GenerateProcessInstanceTable(dtProcess);

ILP model = new ILP(dtProcess, dtProcessInstances);
model.Solve(4, 4, 100);
int[,] yValue = model.PrintYValue();
int[,] xValue = model.PrintXValue();
int[] hValue = model.PrintHValue();
double[,] sValue= model.PrintSValue();
int[,,] zValue = model.PrintZValue();

Console.ReadLine();

