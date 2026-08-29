// Gerekli Kütüphaneler:
// using System.Data;

using System.Data;

public class BestSolutionState
{
    public double MinCost { get; set; } = double.MaxValue;
    public int BestIteration { get; set; } = -1;
    public double BestTimeInSeconds { get; set; } // Yeni eklenen özellik
    public DataTable ResultTable { get; set; }

    public BestSolutionState()
    {
        // Başlangıçta boş ve şeması hazır bir tablo oluşturuluyor
        ResultTable = CreateResultTableSchema();
    }

    // Ek.xlsx formatına tam uyumlu DataTable şemasını oluşturan yardımcı metot
    private DataTable CreateResultTableSchema()
    {
        DataTable dt = new DataTable("BestResult");
        dt.Columns.Add("IDProcess", typeof(string));
        dt.Columns.Add("IDProcessInstance", typeof(string));
        dt.Columns.Add("ReleaseDay", typeof(int));       // Null gelebiliyorsa typeof(object) yapılabilir
        dt.Columns.Add("ReleaseHour", typeof(int));
        dt.Columns.Add("ReleaseMinute", typeof(int));
        dt.Columns.Add("ReleaseTime", typeof(int));
        dt.Columns.Add("DueTime", typeof(int));
        dt.Columns.Add("ProcessingTime", typeof(int));
        dt.Columns.Add("WindowLenght", typeof(int));
        dt.Columns.Add("StartTime", typeof(int));
        dt.Columns.Add("FinishTime", typeof(int));
        dt.Columns.Add("Tardiness", typeof(int));
        dt.Columns.Add("RobotNumber", typeof(int));
        dt.Columns.Add("Department", typeof(string));
        dt.Columns.Add("Account", typeof(string));
        dt.Columns.Add("Software-1", typeof(string));
        dt.Columns.Add("Software-2", typeof(string));
        dt.Columns.Add("Software-3", typeof(string));

        return dt;
    }
}