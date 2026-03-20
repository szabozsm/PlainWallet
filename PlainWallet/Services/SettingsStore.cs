using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PlainWallet.Data;
using PlainWallet.Models;
using Microsoft.EntityFrameworkCore;

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
        }
        public static async Task CancelAsync()
        {
            if (_currentSettings == null || _services == null)
                throw new InvalidOperationException("Settings not initialized. Call Initialize first.");
            using var scope = _services.CreateScope();
            var ctx = scope.ServiceProvider.GetRequiredService<CardDbContext>();
            _currentSettings = ctx.Settings.FirstOrDefault();
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
            set => UpdateProperty(value, (s, v) => s.UseExtendsClass = v);
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

        public static void Reload()
        {
            if (_services == null)
                throw new InvalidOperationException("Settings not initialized. Call Initialize first.");

            Initialize(_services);
        }
    }
}