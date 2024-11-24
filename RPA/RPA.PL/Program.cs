// See https://aka.ms/new-console-template for more information
using RPA.MathModel;
using RPA.TestProblemGenerator;
using System.ComponentModel;
using System.Data;



ProcessGenerator pg = new ProcessGenerator(4);
DataTable dtProcess = pg.GenerateProcessTable(4, 4);
DataTable dtProcessInstances = pg.GenerateProcessInstanceTable(dtProcess);

ILP model = new ILP(dtProcess, dtProcessInstances);
model.Solve(4, 4, 10, 0, 1, 10080);
int[,] yValue = model.PrintYValue();
int[,] xValue = model.PrintXValue();
int[] hValue = model.PrintHValue();
double[,] sValue= model.PrintSValue();
double[,] ssValue = model.PrintSSValue();
int[,,] zValue = model.PrintZValue();

DataTable dtResult = model.resultTable(dtProcessInstances);

Console.ReadLine();

