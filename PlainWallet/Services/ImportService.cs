using System;
using System.Text.Json;
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
                                var tracked = db.Cards.Local.FirstOrDefault(x => x.Id == card.Id) ?? db.Cards.Find(card.Id);
                                if (tracked == null)
                                {
                                        db.Cards.Attach(card);
                                        db.Entry(card).State = Microsoft.EntityFrameworkCore.EntityState.Added;
                                        CardCount++;
                                }
                                else
                                {
                                        if (card.JsonLastChanged > tracked.LastChanged)
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

        public void UploadData()
        {
                var data = GetDataToExport();

                string BucketId = GetBucketId(); // Implement this method to retrieve the appropriate bucket ID

                var cli = _services.GetRequiredService<IExtendsClassClient>();

                cli.BinPUTAsync(data, BucketId);

        }

        public async Task DownloadData()
        {
                string BucketId = GetBucketId(); // Implement this method to retrieve the appropriate bucket ID

                var cli = _services.GetRequiredService<IExtendsClassClient>();

                var data = await cli.BinGETAsync(BucketId);

                if (data != null)
                {
                        string json = data.ToString();
                        await ImportData(json);
                }
        }

        private string GetBucketId()
        {
                return "822f3c9057b3";
        }

        private class ImportedData
        {
                public List<MembershipCard>? Cards { get; set; }
        }
}
