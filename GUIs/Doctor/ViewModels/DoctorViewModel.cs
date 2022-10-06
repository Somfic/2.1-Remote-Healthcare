using System;

namespace RemoteHealthcare.GUIs.Doctor.ViewModels;

public class DoctorViewModel
{
    public DoctorViewModel() 
    {
        
    }

    private string _doctorName;
    private string _chartTitle = "Hello World";
    private string _xAxisTitle = "XAxis";
    private string _yAxisTitle = "YAxis";

    public string DoctorName
    {
        get => _doctorName;
        set => _doctorName = value;
    }

    public string ChartTitle
    {
        get => _chartTitle;
        set => _chartTitle = value ?? throw new ArgumentNullException(nameof(value));
    }

    public string XAxisTitle
    {
        get => _xAxisTitle;
        set => _xAxisTitle = value ?? throw new ArgumentNullException(nameof(value));
    }

    public string YAxisTitle
    {
        get => _yAxisTitle;
        set => _yAxisTitle = value ?? throw new ArgumentNullException(nameof(value));
    }
}