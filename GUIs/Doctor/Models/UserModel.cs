using System;
using MvvmHelpers;

namespace RemoteHealthcare.GUIs.Doctor.Models;

public class UserModel : ObservableObject
{
    private string _name = "Richard Elean";
    private float _speed = 100;
    private float _distance = 250;
    private TimeSpan _totalElapsed;
    private int _bpm = 200;

    public string Name
    {
        get => _name;
        set => _name = value ?? throw new ArgumentNullException(nameof(value));
    }

    public float Speed
    {
        get => _speed;
        set => _speed = value;
    }

    public float Distance
    {
        get => _distance;
        set => _distance = value;
    }

    public TimeSpan TotalElapsed
    {
        get => _totalElapsed;
        set => _totalElapsed = value;
    }

    public int Bpm
    {
        get => _bpm;
        set => _bpm = value;
    }
}