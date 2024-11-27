using Gurobi;
using System.Data;
using System.Linq;
using System.Runtime.ConstrainedExecution;
using System.Runtime.ExceptionServices;
using System.Security.Cryptography;

namespace RPA.MathModel
{
	public class ILP
	{
		DataTable dtProcess;
		DataTable dtProcessInstance;

		GRBEnv environment;
		GRBModel model;

		GRBVar[] T, h;
		GRBVar[,] x, y, s, ss, u;
		GRBVar[,,] z, v, alpha;

		double[] Td;
		int[] hd;
		int[,] xd, yd, ud;
		int[,,] zd, vd, alphad;
		double[,] sd, ssd;
		double obj;

		public ILP(DataTable dtProcess, DataTable dtProcessInstance)
		{
			this.dtProcess = dtProcess;
			this.dtProcessInstance = dtProcessInstance;

			environment = new GRBEnv();
			model = new GRBModel(environment);
		}

		public void Solve(int numOfDepartment, int numOfAccount, int numOfSoftware, int numOfRobot, double costOfRobot, double[] costOfSoftsare, double penaltyCost, int planningHorizon)
		{
			int numOfProc = dtProcess.Rows.Count;
			int numOfProcInstance = dtProcessInstance.Rows.Count;			
			double M = 10000000000000;

			#region Değişken Tanımlama

			x = new GRBVar[numOfProcInstance, numOfRobot];
			y = new GRBVar[numOfProc, numOfRobot];
			alpha = new GRBVar[numOfProcInstance, numOfProcInstance, numOfRobot];
			z = new GRBVar[numOfProcInstance, numOfProcInstance, numOfRobot];
			v = new GRBVar[numOfProcInstance, numOfProcInstance, numOfAccount];

			s = new GRBVar[numOfProcInstance, numOfRobot];
			ss = new GRBVar[numOfProcInstance, numOfAccount];
			u = new GRBVar[numOfSoftware, numOfRobot];
			T = new GRBVar[numOfProcInstance];
			h = new GRBVar[numOfRobot];



			for (int k = 0; k < numOfProc; k++)
			{
				for (int j = 0; j < numOfRobot; j++)
				{
					y[k, j] = model.AddVar(0, 1, 0, GRB.BINARY, "y(" + (k).ToString() + "," + (j).ToString() + ")");
				}
			}

			for (int i = 0; i < numOfProcInstance; i++)
			{
				T[i] = model.AddVar(0, planningHorizon, 0, GRB.CONTINUOUS, "T(" + (i).ToString() + ")");//24*60*30			

				for (int l = 0; l < numOfAccount; l++)
				{
					ss[i, l] = model.AddVar(0, planningHorizon, 0, GRB.CONTINUOUS, "ss(" + (i).ToString() + "," + (l).ToString() + ")");//24*60*30
				}

				for (int j = 0; j < numOfRobot; j++)
				{
					x[i, j] = model.AddVar(0, 1, 0, GRB.BINARY, "x(" + (i).ToString() + "," + (j).ToString() + ")");

					s[i, j] = model.AddVar(0, planningHorizon, 0, GRB.CONTINUOUS, "s(" + (i).ToString() + "," + (j).ToString() + ")");//24*60*30
				}

				for (int f = 0; f < numOfProcInstance; f++)
				{
					for (int j = 0; j < numOfRobot; j++)
					{
						alpha[i, f, j] = model.AddVar(0, 1, 0, GRB.BINARY, "alpha(" + (i).ToString() + "," + (f).ToString() + "," + (j).ToString() + ")");

						z[i, f, j] = model.AddVar(0, 1, 0, GRB.BINARY, "z(" + (i).ToString() + "," + (f).ToString() + "," + (j).ToString() + ")");

					}

					for (int l = 0; l < numOfAccount; l++)
					{
						v[i, f, l] = model.AddVar(0, 1, 0, GRB.BINARY, "v(" + (i).ToString() + "," + (f).ToString() + "," + (l).ToString() + ")");
					}
				}
			}

			for (int n = 0; n < numOfSoftware; n++)
			{
				double cost = costOfSoftsare[n];
				for (int j = 0; j < numOfRobot; j++)
				{
					u[n, j] = model.AddVar(0, 1, cost, GRB.BINARY, "u(" + (n).ToString() + "," + (j).ToString() + ")");
				}
			}

			for (int j = 0; j < numOfRobot; j++)
			{
				h[j] = model.AddVar(0, 1, costOfRobot, GRB.BINARY, "h(" + (h).ToString() + "," + (j).ToString() + ")");
			}

			#endregion

			model.Update();

			#region Kısıt Tanımlama

			#region Kısıt-1: Her k işlem türünün sadece bir j robotunda yürütülebilir 

			for (int k = 0; k < numOfProc; k++)//Each
			{
				GRBLinExpr k1 = 0;
				for (int j = 0; j < numOfRobot; j++)//Sum
				{
					k1 += y[k, j];
				}
				model.AddConstr(k1 == 1, "k1(" + (k).ToString() + " Process)");
			}

			#endregion

			#region Kısıt-2: s[i,j] sıfırlama 

			for (int i = 0; i < numOfProcInstance; i++)//Each
			{
				for (int j = 0; j < numOfRobot; j++)//Each
				{
					model.AddConstr(s[i, j] <= x[i, j] * M, "k2(" + (i).ToString() + " ProcessInst" + j.ToString() + " Robot)");
				}
			}

			#endregion

			#region Kısıt-3: Aynı robot içinde sıralama 

			for (int i = 0; i < numOfProcInstance; i++)//Each
			{
				for (int f = 0; f < numOfProcInstance; f++)//Each
				{
					for (int j = 0; j < numOfRobot; j++)//Each
					{
						if (i!=f)
						{						
							//DataTable dt = dtProcess.Select("IDProcess LIKE '%" + dtProcessInstance.Rows[i][0].ToString() + "%'").CopyToDataTable();
							int processingTime = Convert.ToInt32(dtProcessInstance.Rows[i][7]);

							model.AddConstr(s[f, j] >= s[i, j] + processingTime - (1 - z[i, f, j]) * M, "k2(" + (i).ToString() + " ProcessInst" + j.ToString() + " Robot)");
						}					
					}
				}
			}

			#endregion

			#region Kısıt-5: Özel bölüm özel robot 

			for (int m = 0; m < numOfDepartment; m++)
			{
				for (int k = 0; k < numOfProc; k++)//Each
				{
					if (dtProcess.Rows[k][6].ToString() == m.ToString())
					{
						for (int j = 0; j < numOfRobot; j++)//Each
						{
							GRBLinExpr k5 = 0;
							for (int g = 0; g < numOfProc; g++)//Each
							{
								if (dtProcess.Rows[g][6].ToString() != m.ToString())
								{
									k5 += y[g, j];
								}
							}

							model.AddConstr(k5 <= (1 - y[k, j]) * M, "k5(" + (k).ToString() + " Process)");
						}
					}
				}
			}

			#endregion

			#region Kısıt-7: k işlem türünün tüm işlemleri atanmalı

			for (int j = 0; j < numOfRobot; j++)//Each
			{
				for (int k = 0; k < numOfProc; k++)//Each
				{
					GRBLinExpr k7 = 0;
					int numOfInstanceOfThisProc = (int)dtProcess.Rows[k][8] * (int)dtProcess.Rows[k][16];
					for (int i = 0; i < numOfProcInstance; i++)//Sum
					{
						if (dtProcessInstance.Rows[i][0].ToString() == dtProcess.Rows[k][0].ToString())
						{
							k7 += x[i, j];
						}
					}
					model.AddConstr(k7 == numOfInstanceOfThisProc * y[k, j], "k7(" + (j).ToString() + " Robot" + k.ToString() + " Process)");
				}


			}

			#endregion

			#region Kısıt-8: robot yazılım eşleşmesi 

			for (int i = 0; i < numOfProcInstance; i++)//Each
			{
				for (int j = 0; j < numOfRobot; j++)//Each
				{
					for (int n = 0; n < numOfSoftware; n++)//Each
					{
						model.AddConstr(u[n, j] >= x[i, j] * Convert.ToInt32(dtProcessInstance.Rows[i][15 + n]), "k8(" + (i).ToString() + " ProcessInst" + j.ToString() + " Robot" + n.ToString() + " Software)");
					}
				}

			}

			#endregion

			#region Kısıt-9: En erken başlama 

			for (int i = 0; i < numOfProcInstance; i++)//Each
			{
				int earliestStart = Convert.ToInt32(dtProcessInstance.Rows[i][5]);
				GRBLinExpr k9 = 0;
				for (int j = 0; j < numOfRobot; j++)//Sum
				{
					k9 += s[i, j];
				}
				model.AddConstr(k9 >= earliestStart, "k9(" + (i).ToString() + " ProcessInst)");
			}

			#endregion

			#region Kısıt-10: En geç tamamlanma 

			for (int i = 0; i < numOfProcInstance; i++)//Each
			{
				//DataTable dt = dtProcess.Select("IDProcess LIKE '%" + dtProcessInstance.Rows[i][0].ToString() + "%'").CopyToDataTable();
				int processingTime = Convert.ToInt32(dtProcessInstance.Rows[i][7]);
				int latestStart = Convert.ToInt32(dtProcessInstance.Rows[i][6]) - processingTime;

				GRBLinExpr k10 = 0;
				for (int j = 0; j < numOfRobot; j++)//Sum
				{
					k10 += s[i, j];
				}
				model.AddConstr(k10 <= latestStart, "k10(" + (i).ToString() + " ProcessInst)");
			}

			#endregion

			#region Kısıt-11: Gecikme hesabı

			for (int i = 0; i < numOfProcInstance; i++)//Each
			{
				GRBLinExpr k11 = 0;
				for (int j = 0; j < numOfRobot; j++)//Sum
				{
					k11 += s[i, j];
				}
				model.AddConstr(T[i] >= k11 - Convert.ToInt32(dtProcessInstance.Rows[i][5]), "k11(" + (i).ToString() + " ProcessInst)");
			}

			#endregion

			#region Kısıt-12: j robotu kullanıyorsa 1 			

			for (int j = 0; j < numOfRobot; j++)//Each
			{
				GRBLinExpr k12 = 0;
				for (int k = 0; k < numOfProc; k++)//Sum
				{
					k12 += y[k, j];
				}
				model.AddConstr(k12 <= h[j] * M, "k12(" + (j).ToString() + " Process)");
			}

			#endregion

			#region Kısıt-13: s[i,j] robot kapasitesi/planlama ufku 

			for (int i = 0; i < numOfProcInstance; i++)//Each
			{
				for (int j = 0; j < numOfRobot; j++)//Each
				{
					model.AddConstr(s[i, j] <= planningHorizon - Convert.ToInt32(dtProcessInstance.Rows[i][7]), "k13(" + (i).ToString() + " ProcessInst" + j.ToString() + " Robot)");
				}

			}

			#endregion

			#region Kısıt-14: Doğrusallaştırma-1 

			for (int i = 0; i < numOfProcInstance; i++)//Each
			{
				for (int f = 0; f < numOfProcInstance; f++)//Each
				{
					for (int j = 0; j < numOfRobot; j++)//Each
					{
						if (i != f)
						{
							model.AddConstr(alpha[i, f, j] <= x[i, j], "k14(" + (i).ToString() + " ProcessInst" + f.ToString() + " ProcessInst" + j.ToString() + " Robot)");
						}
					}
				}
			}

			#endregion

			#region Kısıt-15: Doğrusallaştırma-2

			for (int i = 0; i < numOfProcInstance; i++)//Each
			{
				for (int f = 0; f < numOfProcInstance; f++)//Each
				{
					for (int j = 0; j < numOfRobot; j++)//Each
					{
						if (i != f)
						{
							model.AddConstr(alpha[i, f, j] <= x[f, j], "k15(" + (i).ToString() + " ProcessInst" + f.ToString() + " ProcessInst" + j.ToString() + " Robot)");
						}
					}
				}
			}

			#endregion

			#region Kısıt-16: Doğrusallaştırma-3

			for (int i = 0; i < numOfProcInstance; i++)//Each
			{
				for (int f = 0; f < numOfProcInstance; f++)//Each
				{
					for (int j = 0; j < numOfRobot; j++)//Each
					{
						if (i > f)
						{
							model.AddConstr(alpha[i, f, j] >= x[i, j] + x[f, j] - 1, "k16(" + (i).ToString() + " ProcessInst" + f.ToString() + " ProcessInst" + j.ToString() + " Robot)");
						}
					}
				}
			}

			#endregion

			#region Kısıt-17: Ya i-j den önce ya da j-i den önce (Robot için))

			for (int i = 0; i < numOfProcInstance; i++)//Each
			{
				for (int f = 0; f < numOfProcInstance; f++)//Each
				{
					for (int j = 0; j < numOfRobot; j++)//Each
					{
						if (i > f)
						{
							model.AddConstr(z[i, f, j] + z[f, i, j] == alpha[i, f, j], "k17(" + (i).ToString() + " ProcessInst" + f.ToString() + " ProcessInst" + j.ToString() + " Robot)");
						}
					}
				}
			}

			#endregion

			#region Kısıt-18: s[i,j] =s[i,l] robot hesap eşleşmesi 

			for (int l = 1; l < numOfAccount; l++)//Each l=0 hesap gerektirmez !
			{
				for (int i = 0; i < numOfProcInstance; i++)//Each
				{
					if (dtProcessInstance.Rows[i][14].ToString() == l.ToString())
					{
						GRBLinExpr k18 = 0;
						for (int j = 0; j < numOfRobot; j++)//Sum
						{
							k18 += s[i, j];
						}
						model.AddConstr(k18 == ss[i, l], "k18(" + (l).ToString() + " Process" + (i).ToString() + " ProcessInst)");
					}
				}

			}

			#endregion

			#region Kısıt-19: Aynı hesap içinde sıralama 
			for (int l = 1; l < numOfAccount; l++)//Each l=0 hesap gerektirmez !
			{
				for (int i = 0; i < numOfProcInstance; i++)//Each
				{
					if (dtProcessInstance.Rows[i][14].ToString() == l.ToString())
					{
						for (int f = 0; f < numOfProcInstance; f++)//Each
						{
							if (dtProcessInstance.Rows[f][14].ToString() == l.ToString())
							{
								if (i != f)
								{
									int processingTime = Convert.ToInt32(dtProcessInstance.Rows[i][7]);

									model.AddConstr(ss[f, l] >= ss[i, l] + processingTime - (1 - v[i, f, l]) * M, "k2(" + (i).ToString() + " ProcessInst" + l.ToString() + " Account)");
								}
							}
						}
					}
				}
			}

			#endregion

			#region Kısıt-20: Ya i-j den önce ya da j-i den önce (Hesap için)

			for (int l = 1; l < numOfAccount; l++)//Each l=0 hesap gerektirmez !
			{
				for (int i = 0; i < numOfProcInstance; i++)//Each
				{
					if (dtProcessInstance.Rows[i][14].ToString() == l.ToString())
					{
						for (int f = 0; f < numOfProcInstance; f++)//Each
						{
							if (dtProcessInstance.Rows[f][14].ToString() == l.ToString())
							{
								if (i > f)
								{
									model.AddConstr(v[i, f, l] + v[f, i, l] == 1, "k20(" + (i).ToString() + " ProcessInst" + f.ToString() + " ProcessInst" + l.ToString() + " Account)");
								}
							}
						}
					}
				}
			}

			#endregion

			//#region Kısıt-6: x[i,j] ve y[k,j] eşleştirme 

			//for (int k = 0; k < numOfProc; k++)// Each
			//{
			//	for (int j = 0; j < numOfRobot; j++)//Each
			//	{
			//		GRBLinExpr k6 = 0;
			//		for (int i = 0; i < numOfProcInstance; i++)//Sum
			//		{
			//			if (dtProcessInstance.Rows[i][0].ToString() == dtProcess.Rows[j][0].ToString())
			//			{

			//				k6 += x[i, j];
			//			}
			//		}
			//		model.AddConstr(k6 <= y[k, j] * M, "k6(" + (k).ToString() + " Process" + j.ToString() + " Robot)");

			//	}
			//}
			//#endregion






			#endregion

			model.Update();

			#region Modelin Çözümü

			model.Set(GRB.IntAttr.ModelSense, 1);
			//Default minimization, -1 maximize, +1 minimize
			model.GetEnv().Set(GRB.DoubleParam.NodefileStart, 0.9);
			model.GetEnv().Set(GRB.DoubleParam.MIPGap, 0);
			model.GetEnv().Set(GRB.DoubleParam.TimeLimit, 3600);
			model.GetEnv().Set(GRB.DoubleParam.IterationLimit, 100000000000);
			model.GetEnv().Set(GRB.IntParam.Threads, 1);
			model.GetEnv().Set(GRB.IntParam.Method, 0);
			//model.GetEnv().Set(GRB.IntParam.OutputFlag, 0);
			model.Write("modelRPA.lp");
			//Console.WriteLine("model yazildi");
			//Console.ReadKey();
			model.Optimize();

			#endregion

			#region Sonuçlar

			xd = new int[numOfProcInstance, numOfRobot];
			yd = new int[numOfProc, numOfRobot];
			alphad = new int[numOfProcInstance, numOfProcInstance, numOfRobot];
			zd = new int[numOfProcInstance, numOfProcInstance, numOfRobot];
			vd = new int[numOfProcInstance, numOfProcInstance, numOfAccount];
			sd = new double[numOfProcInstance, numOfRobot];
			ssd = new double[numOfProcInstance, numOfAccount];
			ud = new int[numOfSoftware, numOfRobot];
			Td = new double[numOfProcInstance];
			hd = new int[numOfRobot];

			int status = model.Status;
			obj = model.Get(GRB.DoubleAttr.ObjVal);			

			for (int k = 0; k < numOfProc; k++)
			{
				for (int j = 0; j < numOfRobot; j++)
				{
					if (y[k, j].Get(GRB.DoubleAttr.X) > 0.5) yd[k, j] = 1;
					else yd[k, j] = 0;
				}
			}

			for (int i = 0; i < numOfProcInstance; i++)
			{
				for (int l = 0; l < numOfAccount; l++)
				{
					ssd[i, l] = ss[i, l].Get(GRB.DoubleAttr.X);
				}
				for (int j = 0; j < numOfRobot; j++)
				{
					if (x[i, j].Get(GRB.DoubleAttr.X) > 0.5) xd[i, j] = 1;
					else xd[i, j] = 0;

					sd[i, j] = s[i, j].Get(GRB.DoubleAttr.X);
				}

				for (int f = 0; f < numOfProcInstance; f++)
				{
					for (int j = 0; j < numOfRobot; j++)
					{
						if (z[i, f, j].Get(GRB.DoubleAttr.X) > 0.5) zd[i, f, j] = 1;
						else zd[i, f, j] = 0;

						if (alpha[i, f, j].Get(GRB.DoubleAttr.X) > 0.5) alphad[i, f, j] = 1;
						else alphad[i, f, j] = 0;
					}

					for (int l = 0; l < numOfAccount; l++)
					{
						if (v[i, f, l].Get(GRB.DoubleAttr.X) > 0.5) vd[i, f, l] = 1;
						else vd[i, f, l] = 0;
					}
				}
			}

			for (int n = 0; n < 3; n++)
			{
				for (int j = 0; j < numOfRobot; j++)
				{
					if (u[n, j].Get(GRB.DoubleAttr.X) > 0.5) ud[n, j] = 1;
					else ud[n, j] = 0;
				}
			}

			for (int j = 0; j < numOfRobot; j++)
			{
				if (h[j].Get(GRB.DoubleAttr.X) > 0.5) hd[j] = 1;
				else hd[j] = 0;
			}


			#endregion
		}

		public int[,] PrintXValue()
		{
			return xd;
		}

		public int[,] PrintYValue()
		{
			return yd;
		}

		public int[] PrintHValue()
		{
			return hd;
		}

		public double[,] PrintSValue()
		{
			return sd;
		}

		public double[,] PrintSSValue()
		{
			return ssd;
		}

		public int[,,] PrintZValue()
		{
			return zd;
		}

		public int[,,] PrintALPHAValue()
		{
			return alphad;
		}

		public DataTable resultTable(DataTable dtInstance)
		{
			for (int i = 0; i < sd.GetLength(0); i++)
			{
				for (int j = 0; j < sd.GetLength(1); j++)
				{
					if (sd[i,j]>0)
					{
						dtInstance.Rows[i][9]=sd[i,j];
						dtInstance.Rows[i][10] = sd[i, j] + Convert.ToInt32(dtInstance.Rows[i][7]);

						int tardiness = Convert.ToInt32(dtInstance.Rows[i][9]) - Convert.ToInt32(dtInstance.Rows[i][5]);
						if (tardiness > 0) dtInstance.Rows[i][11] = tardiness;
						else dtInstance.Rows[i][11]	= 0;

						dtInstance.Rows[i][12] = j;
					}
				}
			}

			return dtInstance;
		}
	}
}
