using System.Data;
using System.Transactions;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace RPA.TestProblemGenerator
{
	public class ProcessGenerator
	{
		int numOfProcess;
		public	ProcessGenerator(int numOfProcess) { 
		this.numOfProcess = numOfProcess;	
		}

		public DataTable GenerateProcessTable(int numOfDepartment, int numOfAccount)
		{
			DataTable dtProcesses = new DataTable();
			dtProcesses.Columns.Add("IDProcess", typeof(string));
			dtProcesses.Columns.Add("ProcessingTime", typeof(int));
			dtProcesses.Columns.Add("PriorityLevel", typeof(int));
			dtProcesses.Columns.Add("RequiredSoftware-1", typeof(string));
			dtProcesses.Columns.Add("RequiredSoftware-2", typeof(string));
			dtProcesses.Columns.Add("RequiredSoftware-3", typeof(string));
			dtProcesses.Columns.Add("DepartmentOfProcess", typeof(string));
			dtProcesses.Columns.Add("AccountInfo", typeof(string));
			dtProcesses.Columns.Add("NumOfWeeklyRepet", typeof(int));
			dtProcesses.Columns.Add("TransactionDay-1", typeof(int));
			dtProcesses.Columns.Add("TransactionDay-2", typeof(int));
			dtProcesses.Columns.Add("TransactionDay-3", typeof(int));
			dtProcesses.Columns.Add("TransactionDay-4", typeof(int));
			dtProcesses.Columns.Add("TransactionDay-5", typeof(int));
			dtProcesses.Columns.Add("TransactionDay-6", typeof(int));
			dtProcesses.Columns.Add("TransactionDay-7", typeof(int));
			dtProcesses.Columns.Add("NumOfDailyRepet", typeof(int));
			dtProcesses.Columns.Add("TransactionTime-1", typeof(string));
			dtProcesses.Columns.Add("TransactionTime-2", typeof(string));
			dtProcesses.Columns.Add("TransactionTime-3", typeof(string));
			dtProcesses.Columns.Add("TransactionTime-4", typeof(string));
			dtProcesses.Columns.Add("TransactionTime-5", typeof(string));

			Random rnd = new Random();
			

			for (int i = 0; i < numOfProcess; i++)
			{
				List<int> listDay = new List<int>();
				List<int> listHour = new List<int>();
				List<int> listMinute = new List<int>();
				for (int ii = 0; ii < 7; ii++) listDay.Add(ii + 1);
				for (int ii = 0; ii < 24; ii++) listHour.Add(ii);
				for (int ii = 0; ii < 4; ii++) listMinute.Add(ii * 15);

				DataRow dr = dtProcesses.NewRow();
				dr[0] = "Process" + i.ToString();
				dr[1] = rnd.Next(5, 81);
				dr[2] = rnd.Next(1, 4);//Low, Medium, High
				if (rnd.NextDouble() > 0.7) dr[3] = "Yes";
				else dr[3] = "No";
				if (rnd.NextDouble() > 0.80) dr[4] = "Yes";
				else dr[4] = "No";
				if (rnd.NextDouble() > 0.90) dr[5] = "Yes";
				else dr[5] = "No";
				if (rnd.NextDouble() > 0.70) dr[6] = rnd.Next(1, numOfDepartment);
				else dr[6] = 0;
				if (rnd.NextDouble() > 0.80) dr[7] = rnd.Next(1, numOfAccount);
				else dr[7] = 0;
				dr[8] = rnd.Next(1, 8);//NumOfWeeklyRepet
				for (int j = 0; j < 7; j++)
				{
					int selectedIndex = rnd.Next(0, listDay.Count);
					dr[9+j] = listDay[selectedIndex];
					listDay.RemoveAt(selectedIndex);
				}
				dr[16]= rnd.Next(1, 6);//NumOfDailyRepet
				for (int k = 0; k < 5; k++)
				{
					int selectedHourIndex = rnd.Next(0, listHour.Count);
					int selectedMinuteIndex = rnd.Next(0, listMinute.Count);
					dr[17 + k] = listHour[selectedHourIndex] + "." + listMinute[selectedMinuteIndex];
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
			dtProcessInstanceTable.Columns.Add("TransactionDay", typeof(int));
			dtProcessInstanceTable.Columns.Add("TransactionTime", typeof(string));

			int cnt = 0;
			for (int i = 0; i < dtProcess.Rows.Count; i++) 
			{
				for (int j = 0; j < Convert.ToInt32(dtProcess.Rows[i][8]); j++)
				{
					for (int k = 0; k < Convert.ToInt32(dtProcess.Rows[i][16]); k++)
					{
						DataRow dr = dtProcessInstanceTable.NewRow();
						dr[0] = dtProcess.Rows[i][0].ToString();
						dr[1] = "Insance_" + i.ToString() + "_" + cnt.ToString();
						dr[2] = Convert.ToInt32(dtProcess.Rows[i][9 + j]);
						dr[3] = dtProcess.Rows[i][17 + k].ToString();

						dtProcessInstanceTable.Rows.Add(dr);
						cnt++;
					}
				}
			}

			return dtProcessInstanceTable;

		}
	}
}
