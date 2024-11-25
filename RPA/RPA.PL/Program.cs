// See https://aka.ms/new-console-template for more information
using RPA.MathModel;
using RPA.TestProblemGenerator;
using System.ComponentModel;
using System.Data;



ProcessGenerator pg = new ProcessGenerator(4);
DataTable dtProcess = pg.GenerateProcessTable(4, 4);
DataTable dtProcessInstances = pg.GenerateProcessInstanceTable(dtProcess);
double[] costOfSoftare = new double[3];
costOfSoftare[0] = 100;
costOfSoftare[1] = 150;
costOfSoftare[2] = 200;

ILP model = new ILP(dtProcess, dtProcessInstances);
model.Solve(4, 4, 3, 10, 3000, costOfSoftare, 1, 10080);
int[,] yValue = model.PrintYValue();
int[,] xValue = model.PrintXValue();
int[] hValue = model.PrintHValue();
double[,] sValue= model.PrintSValue();
double[,] ssValue = model.PrintSSValue();
int[,,] zValue = model.PrintZValue();

DataTable dtResult = model.resultTable(dtProcessInstances);

Console.ReadLine();

