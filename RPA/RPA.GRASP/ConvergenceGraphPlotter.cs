using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

public class ConvergenceGraphPlotter
{
    // YÖNTEM 1: Excel (Ofis Programları) ile grafik çizimi için verileri dışa aktarır
    public static void ExportToCsv(List<IterationResult> results, string filePath)
    {
        StringBuilder sb = new StringBuilder();

        // Türkçe Excel için ayırıcı olarak noktalı virgül (;) kullanılmıştır.
        sb.AppendLine("İterasyon;Toplam Maliyet");

        foreach (var res in results)
        {
            sb.AppendLine($"{res.Iteration};{res.Cost}");
        }

        File.WriteAllText(filePath, sb.ToString(), Encoding.UTF8);
    }

    // YÖNTEM 2: Kod üzerinden doğrudan .PNG formatında grafik çizdirmek için (WinForms)
    // Not: Kullanmak için projenize "System.Windows.Forms.DataVisualization" referansını 
    // ve "using System.Drawing;" kütüphanesini eklemeniz gerekir.
    /*
    public static void SaveGraphAsImage(List<IterationResult> results, string filePath)
    {
        var chart = new System.Windows.Forms.DataVisualization.Charting.Chart();
        chart.Size = new System.Drawing.Size(800, 600);
        
        var chartArea = new System.Windows.Forms.DataVisualization.Charting.ChartArea();
        chartArea.AxisX.Title = "İterasyon (Çözüm Turu)";
        chartArea.AxisY.Title = "Toplam Maliyet";
        chart.ChartAreas.Add(chartArea);

        var series = new System.Windows.Forms.DataVisualization.Charting.Series
        {
            Name = "Yakınsama Eğrisi",
            Color = System.Drawing.Color.Blue,
            IsVisibleInLegend = false,
            IsXValueIndexed = true,
            ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Line,
            BorderWidth = 2
        };

        foreach (var res in results)
        {
            series.Points.AddXY(res.Iteration, res.Cost);
        }

        chart.Series.Add(series);
        chart.SaveImage(filePath, System.Windows.Forms.DataVisualization.Charting.ChartImageFormat.Png);
    }
    */
}