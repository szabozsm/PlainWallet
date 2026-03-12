using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PlainWallet.Models;

public class Settings : INotifyPropertyChanged
{
    private string _dummyProperty = string.Empty;
    public event PropertyChangedEventHandler? PropertyChanged;

    [Key]
    public int Id { get; set; }

    public string DummyProperty
    {
        get => _dummyProperty;
        set
        {
            if (_dummyProperty == value) return;
            _dummyProperty = value;
            OnPropertyChanged(nameof(DummyProperty));
        }
    }

    protected void OnPropertyChanged(string name) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}
