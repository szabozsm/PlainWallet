using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using PlainWallet.Models;

#nullable disable

namespace PlainWallet.Data.Migrations;

[DbContext(typeof(CardDbContext))]
[Migration("20261111_AddJsonBinSettings")]
public partial class AddJsonBinSettings : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<bool>(
            name: nameof(Settings.UseJsonBin),
            table: "Settings",
            type: "INTEGER",
            nullable: false,
            defaultValue: false);

        migrationBuilder.AddColumn<string>(
            name: nameof(Settings.JsonBinAccessKey),
            table: "Settings",
            type: "TEXT",
            nullable: false,
            defaultValue: "");

        migrationBuilder.AddColumn<string>(
            name: nameof(Settings.JsonBinBinId),
            table: "Settings",
            type: "TEXT",
            nullable: false,
            defaultValue: "");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(
            name: nameof(Settings.UseJsonBin),
            table: "Settings");

        migrationBuilder.DropColumn(
            name: nameof(Settings.JsonBinAccessKey),
            table: "Settings");

        migrationBuilder.DropColumn(
            name: nameof(Settings.JsonBinBinId),
            table: "Settings");
    }
}
