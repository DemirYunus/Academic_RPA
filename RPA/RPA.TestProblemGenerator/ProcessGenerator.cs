using System.Data;
using System.Transactions;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace RPA.TestProblemGenerator
{
	public class ProcessGenerator
	{
		int numOfProcess;
		int planningHorizon;
		public	ProcessGenerator(int numOfProcess, int planningHorizon) { 
		this.numOfProcess = numOfProcess;
			this.planningHorizon = planningHorizon;
		}

		public DataTable GenerateProcessTable(int numOfDepartment, int numOfAccount)
		{
			DataTable dtProcesses = new DataTable();
			dtProcesses.Columns.Add("IDProcess", typeof(string));
			dtProcesses.Columns.Add("ProcessingTime", typeof(int));
			dtProcesses.Columns.Add("PriorityLevel", typeof(int));
			dtProcesses.Columns.Add("RequiredSoftware-1", typeof(string));//3
			dtProcesses.Columns.Add("RequiredSoftware-2", typeof(string));
			dtProcesses.Columns.Add("RequiredSoftware-3", typeof(string));
			dtProcesses.Columns.Add("DepartmentOfProcess", typeof(string));//6
			dtProcesses.Columns.Add("AccountInfo", typeof(string));//7
			dtProcesses.Columns.Add("NumOfWeeklyRepet", typeof(int));
			dtProcesses.Columns.Add("TransactionDay-1", typeof(int));//9
			dtProcesses.Columns.Add("TransactionDay-2", typeof(int));
			dtProcesses.Columns.Add("TransactionDay-3", typeof(int));
			dtProcesses.Columns.Add("TransactionDay-4", typeof(int));
			dtProcesses.Columns.Add("TransactionDay-5", typeof(int));
			dtProcesses.Columns.Add("TransactionDay-6", typeof(int));
			dtProcesses.Columns.Add("TransactionDay-7", typeof(int));
			dtProcesses.Columns.Add("NumOfDailyRepet", typeof(int));
			dtProcesses.Columns.Add("TransactionClock-1", typeof(string));//17
			dtProcesses.Columns.Add("TransactionClock-2", typeof(string));
			dtProcesses.Columns.Add("TransactionClock-3", typeof(string));
			dtProcesses.Columns.Add("TransactionClock-4", typeof(string));
			dtProcesses.Columns.Add("TransactionClock-5", typeof(string));
			dtProcesses.Columns.Add("TransactionTime-1", typeof(string));//22
			dtProcesses.Columns.Add("TransactionTime-2", typeof(string));
			dtProcesses.Columns.Add("TransactionTime-3", typeof(string));
			dtProcesses.Columns.Add("TransactionTime-4", typeof(string));
			dtProcesses.Columns.Add("TransactionTime-5", typeof(string));
			dtProcesses.Columns.Add("WindowLenght", typeof(int));

			Random rnd = new Random();	
			for (int i = 0; i < numOfProcess; i++)
			{
				List<int> listDay = new List<int>();
				List<int> listHour = new List<int>();
				List<int> listMinute = new List<int>();
				for (int ii = 0; ii < 7; ii++) listDay.Add(ii + 1);
				//Uygunsuzluğu engelemek için
				if (i%2==0) for (int ii = 0; ii < 22; ii+=2) listHour.Add(ii);				
				else for (int ii = 1; ii < 22; ii += 2) listHour.Add(ii);
				for (int ii = 0; ii < 4; ii++) listMinute.Add(ii * 15);

				DataRow dr = dtProcesses.NewRow();
				dr[0] = "Process" + i.ToString();
				dr[1] = rnd.Next(65, 75);
				dr[2] = rnd.Next(1, 4);//Low, Medium, High
				if (rnd.NextDouble() > 0.70) dr[3] = "1";
				else dr[3] = "0";
				if (rnd.NextDouble() > 0.75) dr[4] = "1";
				else dr[4] = "0";
				if (rnd.NextDouble() > 0.80) dr[5] = "1";
				else dr[5] = "0";
				if (rnd.NextDouble() > 0.70) dr[6] = rnd.Next(1, numOfDepartment);
				else dr[6] = 0;
				if (rnd.NextDouble() > 0.60) dr[7] = rnd.Next(1, numOfAccount);
				else dr[7] = 0;
				dr[8] = rnd.Next(1, 8);//NumOfWeeklyRepet
				for (int j = 0; j < 7; j++)
				{
					int selectedIndex = rnd.Next(0, listDay.Count);
					dr[9 + j] = listDay[selectedIndex];
					listDay.RemoveAt(selectedIndex);
				}
				dr[16]= rnd.Next(1, 6);//NumOfDailyRepet
				for (int k = 0; k < 5; k++)
				{
					int selectedHourIndex = rnd.Next(0, listHour.Count);
					int selectedMinuteIndex = rnd.Next(0, listMinute.Count);
					dr[17 + k] = listHour[selectedHourIndex] + "." + listMinute[selectedMinuteIndex];
					dr[22 + k] = listHour[selectedHourIndex] * 60 + listMinute[selectedMinuteIndex];
					listHour.RemoveAt(selectedHourIndex);
				}

				dtProcesses.Rows.Add(dr);
			}

			return dtProcesses;	
		}

		public DataTable GenerateProcessInstanceTable(DataTable dtProcess)
		{
			DataTable dtProcessInstanceTable = new DataTable();
			dtProcessInstanceTable.Columns.Add("IDProcess", typeof(string));
			dtProcessInstanceTable.Columns.Add("IDProcessInstance", typeof(string));
			dtProcessInstanceTable.Columns.Add("ReleaseDay", typeof(int));
			dtProcessInstanceTable.Columns.Add("ReleaseHour", typeof(int));
			dtProcessInstanceTable.Columns.Add("ReleaseMinute", typeof(int));//4
			dtProcessInstanceTable.Columns.Add("ReleaseTime", typeof(int));//5
			dtProcessInstanceTable.Columns.Add("DueTime", typeof(int));//6
			dtProcessInstanceTable.Columns.Add("ProcessingTime", typeof(int));//7
			dtProcessInstanceTable.Columns.Add("WindowLenght", typeof(int));//8
			dtProcessInstanceTable.Columns.Add("StartTime", typeof(string));//9
			dtProcessInstanceTable.Columns.Add("FinishTime", typeof(string));//10
			dtProcessInstanceTable.Columns.Add("Tardiness", typeof(string));//11
			dtProcessInstanceTable.Columns.Add("RobotNumber", typeof(string));//12
			dtProcessInstanceTable.Columns.Add("Department", typeof(string));//13
			dtProcessInstanceTable.Columns.Add("Account", typeof(string));//14
			dtProcessInstanceTable.Columns.Add("RequiredSoftware-1", typeof(string));//15
			dtProcessInstanceTable.Columns.Add("RequiredSoftware-2", typeof(string));
			dtProcessInstanceTable.Columns.Add("RequiredSoftware-3", typeof(string));

			int cnt = 0;
			int totalNumOfInstance = dtProcess.Rows.Count * dtProcess.Rows.Count;

			for (int i = 0; i < dtProcess.Rows.Count; i++) 
			{
				for (int j = 0; j < Convert.ToInt32(dtProcess.Rows[i][8]); j++)//NumOfWeeklyRepet
				{
					for (int k = 0; k < Convert.ToInt32(dtProcess.Rows[i][16]); k++)//NumOfDailyRepet
					{
						DataRow dr = dtProcessInstanceTable.NewRow();
						dr[0] = dtProcess.Rows[i][0].ToString();
						dr[1] = "Insance_" + i.ToString() + "_" + cnt.ToString();
						dr[2] = Convert.ToInt32(dtProcess.Rows[i][9 + j]);//TransactionDay-j
						string[] time = dtProcess.Rows[i][17 + k].ToString().Split(".");
						dr[3] = Convert.ToInt32(time[0]);//ReleaseHour
						dr[4] = Convert.ToInt32(time[1]);//ReleaseMinute
						dr[5] = (Convert.ToInt32(dtProcess.Rows[i][9 + j]) - 1) * 24 * 60 + Convert.ToInt32(time[0]) * 60 + Convert.ToInt32(time[1]);//ReleaseTime
						dr[7] = Convert.ToInt32(dtProcess.Rows[i][1]);
						dr[13] = dtProcess.Rows[i][6].ToString();
						dr[14] = dtProcess.Rows[i][7].ToString();
						dr[15] = dtProcess.Rows[i][3].ToString();
						dr[16] = dtProcess.Rows[i][4].ToString();
						dr[17] = dtProcess.Rows[i][5].ToString();


						dtProcessInstanceTable.Rows.Add(dr);
						cnt++;
					}
				}
			}
			DataTable sorteddtProcessInstanceTable= sortTable(dtProcessInstanceTable, "IDProcess", "ASC", "ReleaseTime", "ASC");
			DataTable dtInstance = AddDueTime(sorteddtProcessInstanceTable);
		
			return dtInstance;	
		}

		public DataTable sortTable(DataTable dt, string columnName1, string direction1, string columnName2, string direction2)
		{
			DataView dv = dt.DefaultView;
			dv.Sort = columnName1 + " " + direction1 + "," + " " + columnName2 + " " + direction2;
			DataTable sortedDT = dv.ToTable();
			return sortedDT;
		}

		public DataTable AddDueTime(DataTable dt)
		{
			string processName = "Process0";
			for (int i = 0; i < dt.Rows.Count; i++)
			{
				if (dt.Rows[i][0].ToString() == processName && i< dt.Rows.Count-1)
				{
					dt.Rows[i][6] = dt.Rows[i + 1][5];
					dt.Rows[i][8] = Convert.ToInt32(dt.Rows[i][6]) - Convert.ToInt32(dt.Rows[i][5]);
				}
				else
				{
					if (i < dt.Rows.Count - 1)
					{
						dt.Rows[i - 1][6] = planningHorizon;//7*24*60
						dt.Rows[i - 1][8] = Convert.ToInt32(dt.Rows[i - 1][6]) - Convert.ToInt32(dt.Rows[i - 1][5]);
						dt.Rows[i][6] = dt.Rows[i + 1][5];
						dt.Rows[i][8] = Convert.ToInt32(dt.Rows[i][6]) - Convert.ToInt32(dt.Rows[i][5]);
						processName = dt.Rows[i][0].ToString();
					}
				}
			}

			dt.Rows[dt.Rows.Count-1][6] = planningHorizon;//7*24*60
			dt.Rows[dt.Rows.Count - 1][8] = Convert.ToInt32(dt.Rows[dt.Rows.Count - 1][6]) - Convert.ToInt32(dt.Rows[dt.Rows.Count - 1][5]);

			return dt;
		}
	}
}
