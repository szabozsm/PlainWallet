using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PlainWallet.Models;

public class Settings : INotifyPropertyChanged
{
    private string _dummyProperty = string.Empty;
    private string _bucketId = "";
    private string _apikey = "";
    private string _securityKey = "";
    private bool _useExtendsClass=false;

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

    public bool UseExtendsClass
    {
        get => _useExtendsClass;
        set
        {
            if (_useExtendsClass == value) return;
            _useExtendsClass = value;
            OnPropertyChanged(nameof(UseExtendsClass));
        }
    }

  public string BucketId
    {
        get => _bucketId;
        set
        {
            if (_bucketId == value) return;
            _bucketId = value;
            OnPropertyChanged(nameof(BucketId));
        }
    }

    public string Apikey
    {
        get => _apikey;
        set
        {
            if (_apikey == value) return;
            _apikey = value;
            OnPropertyChanged(nameof(Apikey));
        }
    }
 
    public string SecurityKey
    {
        get => _securityKey;
        set
        {
            if (_securityKey == value) return;
            _securityKey = value;
            OnPropertyChanged(nameof(SecurityKey));
        }
    }

    protected void OnPropertyChanged(string name) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}
