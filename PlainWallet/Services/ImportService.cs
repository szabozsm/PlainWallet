using System;
using System.Text.Json;
using CommunityToolkit.Maui.Alerts;
using CommunityToolkit.Maui.Core;
using PlainWallet.Data;
using PlainWallet.Models;

namespace PlainWallet.Services;

public class ImportService
{

        private readonly IServiceProvider _services;
        public ImportService(IServiceProvider services)
        {
                this._services = services;

        }

        public object GetDataToExport()
        {
                using var innerScope = _services.CreateScope();
                using var db = innerScope.ServiceProvider.GetRequiredService<CardDbContext>();
                var cards = db.Cards.ToList();

                cards.ForEach(c => c.JsonLastChanged = c.LastChanged);

                var exportData = new
                {
                        Version = "1.0",
                        ExportDate = DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ss"),
                        Cards = cards
                };

                return exportData;
        }

        public async Task<int> ImportData(string json)
        {
                if (string.IsNullOrWhiteSpace(json))
                        return -1;

                var importData = JsonSerializer.Deserialize<ImportedData>(json, new JsonSerializerOptions
                {
                        PropertyNameCaseInsensitive = true,
                        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                });

                int CardCount = 0;
                if (importData?.Cards != null)
                {
                        using var innerScope0 = _services.CreateScope();
                        using var db = innerScope0.ServiceProvider.GetRequiredService<CardDbContext>();

                        foreach (var card in importData.Cards)
                        {
                                card.SetLastChangedFromSync(card.JsonLastChanged);
                                var tracked = db.Cards.Local.FirstOrDefault(x => x.Id == card.Id) ?? db.Cards.Find(card.Id);
                                if (tracked == null)
                                {
                                        db.Cards.Attach(card);
                                        db.Entry(card).State = Microsoft.EntityFrameworkCore.EntityState.Added;
                                        CardCount++;
                                }
                                else
                                {
                                        if (card.JsonLastChanged > tracked.LastChanged ||
                                            (card.JsonLastChanged == tracked.LastChanged && HasCardDataChanged(card, tracked)))
                                        {
                                                db.Entry(tracked).CurrentValues.SetValues(card);
                                                CardCount++;
                                        }
                                }
                        }

                        await db.SaveChangesAsync();

                        // Refresh the CardStore to update the UI
                        CardStore.RefreshFromDatabase();

                        return CardCount;
                }
                return -1;
        }

        private static bool HasCardDataChanged(MembershipCard incoming, MembershipCard current)
        {
                return !incoming.BackgroundColor.Equals(current.BackgroundColor) ||
                       incoming.LogoKind != current.LogoKind ||
                       incoming.BarcodeTypeValue != current.BarcodeTypeValue ||
                       incoming.CardNumber != current.CardNumber ||
                       incoming.Id != current.Id ||
                       !ByteArraysEqual(incoming.LogoData, current.LogoData) ||
                       incoming.LogoUri != current.LogoUri ||
                       incoming.LogoUrl != current.LogoUrl ||
                       incoming.Name != current.Name ||
                       incoming.Notes != current.Notes;
        }

        private static bool ByteArraysEqual(byte[]? first, byte[]? second)
        {
                return first is null
                        ? second is null
                        : second is not null && first.AsSpan().SequenceEqual(second);
        }

        public async Task UploadData()
        {
                if (SettingsStore.UseJsonBin)
                {
                        await UploadJsonBinData();
                        return;
                }

                try
                {
                        var data = GetDataToExport();
                        string bucketId = await GetBucketId();
                        var cli = _services.GetRequiredService<IExtendsClassClient>();
                        await cli.BinPUTAsync(data, bucketId);
                }
                catch (WifiOnlySyncSkippedException)
                {
                }
                catch (Exception ex)
                {
                                await Toast.Make($"Failed to upload data to extendsclass.com: {ex.Message}", ToastDuration.Long).Show();
                }
        }

        public async Task DownloadData()
        {
                if (SettingsStore.UseJsonBin)
                {
                        await DownloadJsonBinData();
                        return;
                }

                try
                {
                        string bucketId = await GetBucketId();
                        var cli = _services.GetRequiredService<IExtendsClassClient>();
                        var data = await cli.BinGETAsync(bucketId);
                        if (data is not null)
                        {
                                string json = data.ToString() ?? string.Empty;
                                if (!string.IsNullOrEmpty(json))
                                {
                                        await ImportData(json);
                                }
                        }
                }
                catch (WifiOnlySyncSkippedException)
                {
                }
                catch (Exception ex)
                {
                                await Toast.Make($"Failed to download data from extendsclass.com: {ex.Message}", ToastDuration.Long).Show();
                }
        }

        private async Task UploadJsonBinData()
        {
                try
                {
                        var client = _services.GetRequiredService<JsonBinClient>();
                        var data = GetDataToExport();
                        if (string.IsNullOrWhiteSpace(SettingsStore.JsonBinBinId))
                        {
                                SettingsStore.JsonBinBinId = await client.CreateBinAsync(
                                        data,
                                        SettingsStore.JsonBinAccessKey);
                                await SettingsStore.SaveAsync();
                        }
                        else
                        {
                                await client.UpdateBinAsync(
                                        data,
                                        SettingsStore.JsonBinBinId,
                                        SettingsStore.JsonBinAccessKey);
                        }
                }
                catch (WifiOnlySyncSkippedException)
                {
                }
                catch (Exception ex)
                {
                        await Toast.Make($"Failed to upload data to jsonbin.io: {ex.Message}", ToastDuration.Long).Show();
                }
        }

        private async Task DownloadJsonBinData()
        {
                try
                {
                        var client = _services.GetRequiredService<JsonBinClient>();
                        if (string.IsNullOrWhiteSpace(SettingsStore.JsonBinBinId))
                        {
                                SettingsStore.JsonBinBinId = await client.CreateBinAsync(
                                        GetDataToExport(),
                                        SettingsStore.JsonBinAccessKey);
                                await SettingsStore.SaveAsync();
                                return;
                        }

                        var json = await client.GetBinRecordAsync(
                                SettingsStore.JsonBinBinId,
                                SettingsStore.JsonBinAccessKey);
                        await ImportData(json);
                }
                catch (WifiOnlySyncSkippedException)
                {
                }
                catch (Exception ex)
                {
                        await Toast.Make($"Failed to download data from jsonbin.io: {ex.Message}", ToastDuration.Long).Show();
                }
        }

        private async Task<string> GetBucketId()
        {
                if (string.IsNullOrEmpty(SettingsStore.BucketId))
                {
                        var cli = _services.GetRequiredService<IExtendsClassClient>();
                        var res = await cli.BinPOSTAsync();
                        SettingsStore.BucketId = res.Id;

                        await SettingsStore.SaveAsync();
                }

                return SettingsStore.BucketId ?? string.Empty;
        }

        private class ImportedData
        {
                public List<MembershipCard>? Cards { get; set; }
        }
}