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

		public DataTable GenerateProcessTable()
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
			List<int> listDay = new List<int>();
			List<int> listHour = new List<int>();
			List<int> listMinute = new List<int>();
			for (int i = 0; i < 7; i++) listDay.Add(i + 1);
			for (int i = 0; i < 24; i++) listHour.Add(i);
			for (int i = 0; i < 4; i++) listMinute.Add(i * 15);

			for (int i = 0; i < numOfProcess; i++)
			{
				DataRow dr = dtProcesses.NewRow();
				dr[0] = "Process" + i.ToString();
				dr[1] = rnd.Next(5, 80);
				dr[2] = rnd.Next(1, 3);//Low, Medium, High
				if (rnd.NextDouble() > 0.7) dr[3] = "Yes";
				else dr[3] = "No";
				if (rnd.NextDouble() > 0.80) dr[4] = "Yes";
				else dr[4] = "No";
				if (rnd.NextDouble() > 0.90) dr[5] = "Yes";
				else dr[5] = "No";
				if (rnd.NextDouble() > 0.70) dr[6] = rnd.Next(1,3);
				else dr[6] = 0;
				if (rnd.NextDouble() > 0.80) dr[7] = rnd.Next(1, 3);
				else dr[7] = 0;
				dr[8] = rnd.Next(1, 7);//NumOfWeeklyRepet
				for (int j = 0; j < 7; j++)
				{
					int selectedIndex = rnd.Next(0, listDay.Count);
					dr[9+j] = listDay[selectedIndex];
					listDay.RemoveAt(selectedIndex);
				}
				dr[16]= rnd.Next(1, 5);//NumOfDailyRepet
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
	}
}
