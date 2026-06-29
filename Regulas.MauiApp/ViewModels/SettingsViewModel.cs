using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using Regulas.MauiApp.Services;

namespace Regulas.MauiApp.ViewModels;

public sealed class SettingsViewModel : INotifyPropertyChanged
{
    private string _apiBaseUrl = ApiBaseUrl.Current;
    private string _statusText = $"Default: {ApiBaseUrl.Default}";

    public event PropertyChangedEventHandler? PropertyChanged;

    public ICommand SaveCommand { get; }

    public ICommand ResetCommand { get; }

    public string ApiBaseUrlText { get => _apiBaseUrl; set => SetField(ref _apiBaseUrl, value); }

    public string StatusText { get => _statusText; private set => SetField(ref _statusText, value); }

    public SettingsViewModel()
    {
        SaveCommand = new Command(Save);
        ResetCommand = new Command(Reset);
    }

    private void Save()
    {
        var value = ApiBaseUrlText.Trim();
        if (!ValidBaseUrl(value))
        {
            StatusText = "Use an http or https URL.";
            return;
        }
        ApiBaseUrl.Current = value.TrimEnd('/');
        StatusText = "API URL saved.";
    }

    private void Reset()
    {
        ApiBaseUrl.Reset();
        ApiBaseUrlText = ApiBaseUrl.Current;
        StatusText = "API URL reset.";
    }

    private static bool ValidBaseUrl(string value)
    {
        return Uri.TryCreate(value, UriKind.Absolute, out var uri) && HttpScheme(uri);
    }

    private static bool HttpScheme(Uri uri)
    {
        return uri.Scheme is "http" or "https";
    }

    private bool SetField<T>(ref T field, T value, [CallerMemberName] string? name = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value))
        {
            return false;
        }
        field = value;
        OnPropertyChanged(name);
        return true;
    }

    private void OnPropertyChanged(string? name)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
