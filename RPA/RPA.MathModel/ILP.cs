using Gurobi;
using System.Data;
using System.Runtime.ConstrainedExecution;

namespace RPA.MathModel
{
	public class ILP
	{
		DataTable dtProcess;
		DataTable dtProcessInstance;

		GRBEnv environment;
		GRBModel model;

		GRBVar[] T;
		GRBVar[,] x, y, s, u;
		GRBVar[,,] z, v;

		public ILP(DataTable dtProcess, DataTable dtProcessInstance) 
		{
			this.dtProcess = dtProcess;
			this.dtProcessInstance = dtProcessInstance;

			environment = new GRBEnv();
			model = new GRBModel(environment);
		}

		public void Solve(int numOfDepartment, int numOfAccount)
		{
			int numOfProc= dtProcess.Rows.Count;
			int numOfProcInstance= dtProcessInstance.Rows.Count;
			int numOfRobot= dtProcess.Rows.Count;	

			#region Değişken Tanımlama

			x = new GRBVar[numOfProcInstance, numOfRobot];
			y = new GRBVar[numOfProc, numOfRobot];
			z = new GRBVar[numOfProcInstance, numOfProcInstance, numOfRobot];
			v = new GRBVar[numOfProcInstance, numOfProcInstance, numOfAccount];

			s = new GRBVar[numOfProcInstance, numOfRobot];
			u = new GRBVar[3, numOfRobot];
			T = new GRBVar[numOfProcInstance];

			for (int i = 0; i < numOfProcInstance; i++)
			{
				for (int j = 0; j < numOfRobot; j++)
				{
					x[i, j] = model.AddVar(0, 1, 0, GRB.BINARY, "x(" + (i).ToString() + "," + (j).ToString() + ")");

					s[i, j] = model.AddVar(0, 43200, 0, GRB.CONTINUOUS, "s(" + (i).ToString() + "," + (j).ToString() + ")");//24*60*30
				}

				for (int f = 0; f < numOfProcInstance; f++)
				{
					for (int j = 0; j < numOfRobot; j++)
					{
						z[i, f, j] = model.AddVar(0, 1, 0, GRB.BINARY, "z(" + (i).ToString() + "," + (f).ToString() + "," + (j).ToString() + ")");
					}

					for (int l = 0; l < numOfAccount; l++)
					{
						v[i, f, l] = model.AddVar(0, 1, 0, GRB.BINARY, "v(" + (i).ToString() + "," + (f).ToString() + "," + (l).ToString() + ")");
					}
				}
			}

			for (int n = 0; n < 3; n++)
			{
				for (int j = 0; j < numOfRobot; j++)
				{
					u[n, j] = model.AddVar(0, 1, 0, GRB.BINARY, "u(" + (n).ToString() + "," + (j).ToString() + ")");					
				}
			}
		
			#endregion
		}
	}
}
