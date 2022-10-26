﻿using System;
using System.Threading.Tasks;
using MvvmHelpers.Interfaces;

namespace RemoteHealthcare.GUIs.Doctor.Commands;

public abstract class BaseCommand : IAsyncCommand
{
    public event EventHandler? CanExecuteChanged;

    public virtual bool CanExecute(object? parameter)
    {
        return true;
    }

    public abstract void Execute(object? parameter);

    public abstract Task ExecuteAsync();

    protected void OnCanExecuteChanged()
    {
        CanExecuteChanged?.Invoke(this, EventArgs.Empty);
    }
}