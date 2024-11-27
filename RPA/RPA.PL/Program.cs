// See https://aka.ms/new-console-template for more information
using RPA.MathModel;
using RPA.TestProblemGenerator;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using RPA.DAL;



//ProcessGenerator pg = new ProcessGenerator(4,10080);
//DataTable dtProcess = pg.GenerateProcessTable(4, 4);
//DataTable dtProcessInstances = pg.GenerateProcessInstanceTable(dtProcess);

GetData gd = new GetData();
DataTable dtProcess = gd.GetProcessData();
DataTable dtProcessInstances = gd.GetProcessInstanceData();

double[] costOfSoftare = new double[3];
costOfSoftare[0] = 100;
costOfSoftare[1] = 150;
costOfSoftare[2] = 200;

ILP model = new ILP(dtProcess, dtProcessInstances);
model.Solve(4, 4, 3, 7, 3000, costOfSoftare, 1, 1440); //10080
int[,] yValue = model.PrintYValue();
int[,] xValue = model.PrintXValue();
int[] hValue = model.PrintHValue();
double[,] sValue= model.PrintSValue();
double[,] ssValue = model.PrintSSValue();
int[,,] zValue = model.PrintZValue();

DataTable dtResult = model.resultTable(dtProcessInstances);

Console.ReadLine();

