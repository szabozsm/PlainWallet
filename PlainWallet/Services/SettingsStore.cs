using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PlainWallet.Data;
using PlainWallet.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Maui.Storage;

namespace PlainWallet.Services
{
    public static class SettingsStore
    {
        private static Settings? _currentSettings;
        private static IServiceProvider? _services;

        public static Settings Current => _currentSettings ?? throw new InvalidOperationException("Settings not initialized. Call Initialize first.");

        public static void Initialize(IServiceProvider services)
        {
            _services = services;
            using var scope = services.CreateScope();
            var ctx = scope.ServiceProvider.GetRequiredService<CardDbContext>();

            // Load settings from database
            _currentSettings = ctx.Settings.FirstOrDefault();

            if (_currentSettings == null)
            {
                // Create default settings if none exist
                _currentSettings = new Settings();
                ctx.Settings.Add(_currentSettings);
                ctx.SaveChanges();
            }

            _currentSettings.WifiOnlyForExtendsClass = Preferences.Default.Get(
                nameof(Settings.WifiOnlyForExtendsClass), false);
        }
        public static async Task CancelAsync()
        {
            if (_currentSettings == null || _services == null)
                throw new InvalidOperationException("Settings not initialized. Call Initialize first.");
            using var scope = _services.CreateScope();
            var ctx = scope.ServiceProvider.GetRequiredService<CardDbContext>();
            _currentSettings = ctx.Settings.FirstOrDefault();
            if (_currentSettings is not null)
            {
                _currentSettings.WifiOnlyForExtendsClass = Preferences.Default.Get(
                    nameof(Settings.WifiOnlyForExtendsClass), false);
            }
        }

        public static async Task SaveAsync()
        {
            if (_currentSettings == null || _services == null)
                throw new InvalidOperationException("Settings not initialized. Call Initialize first.");

            using var scope = _services.CreateScope();
            var ctx = scope.ServiceProvider.GetRequiredService<CardDbContext>();

            var existingSettings = await ctx.Settings.FirstOrDefaultAsync();
            if (existingSettings != null)
            {
                ctx.Entry(existingSettings).CurrentValues.SetValues(_currentSettings);
            }
            else
            {
                ctx.Settings.Add(_currentSettings);
            }

            await ctx.SaveChangesAsync();
            Preferences.Default.Set(
                nameof(Settings.WifiOnlyForExtendsClass),
                _currentSettings.WifiOnlyForExtendsClass);
        }

        public static void UpdateProperty<T>(T value, Action<Settings, T> setter)
        {
            if (_currentSettings == null)
                throw new InvalidOperationException("Settings not initialized. Call Initialize first.");

            setter(_currentSettings, value);
        }

        // Convenience properties
        public static string DummyProperty
        {
            get => Current.DummyProperty;
            set => UpdateProperty(value, (s, v) => s.DummyProperty = v);
        }

        public static bool UseExtendsClass
        {
            get => Current.UseExtendsClass;
            set
            {
                if (value)
                    Current.UseJsonBin = false;

                UpdateProperty(value, (settings, enabled) => settings.UseExtendsClass = enabled);
            }
        }

        public static bool UseJsonBin
        {
            get => Current.UseJsonBin;
            set
            {
                if (value)
                    Current.UseExtendsClass = false;

                UpdateProperty(value, (settings, enabled) => settings.UseJsonBin = enabled);
            }
        }

        public static string BucketId
        {
            get => Current.BucketId;
            set => UpdateProperty(value, (s, v) => s.BucketId = v);
        }

        public static string SecurityKey
        {
            get => Current.SecurityKey;
            set => UpdateProperty(value, (s, v) => s.SecurityKey = v);
        }
        public static string Apikey
        {
            get => Current.Apikey;
            set => UpdateProperty(value, (s, v) => s.Apikey = v);
        }

        public static string JsonBinAccessKey
        {
            get => Current.JsonBinAccessKey;
            set => UpdateProperty(value, (settings, accessKey) => settings.JsonBinAccessKey = accessKey);
        }

        public static string JsonBinBinId
        {
            get => Current.JsonBinBinId;
            set => UpdateProperty(value, (settings, binId) => settings.JsonBinBinId = binId);
        }

  public static bool TwoColumnMode
        {
            get => Current.TwoColumnMode;
            set => UpdateProperty(value, (s, v) => s.TwoColumnMode = v);
        }

        public static bool WifiOnlyForExtendsClass
        {
            get => Current.WifiOnlyForExtendsClass;
            set => UpdateProperty(value, (settings, wifiOnly) => settings.WifiOnlyForExtendsClass = wifiOnly);
        }

        public static string LogoSyncEtag
        {
            get => Current.LogoSyncEtag;
            set => UpdateProperty(value, (settings, etag) => settings.LogoSyncEtag = etag);
        }

        public static void Reload()
        {
            if (_services == null)
                throw new InvalidOperationException("Settings not initialized. Call Initialize first.");

            Initialize(_services);
        }
    }
}