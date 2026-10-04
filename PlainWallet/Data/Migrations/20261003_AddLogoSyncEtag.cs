using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using PlainWallet.Models;

#nullable disable

namespace PlainWallet.Data.Migrations;

[DbContext(typeof(CardDbContext))]
[Migration("20261003_AddLogoSyncEtag")]
public partial class AddLogoSyncEtag : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<string>(
            name: nameof(Settings.LogoSyncEtag),
            table: "Settings",
            type: "TEXT",
            nullable: false,
            defaultValue: "");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(
            name: nameof(Settings.LogoSyncEtag),
            table: "Settings");
    }
}
