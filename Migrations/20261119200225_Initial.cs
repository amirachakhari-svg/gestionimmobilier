using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace gestion_immo.Migrations;

/// <inheritdoc />
public partial class _20261119200225_Initial : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "Localisations",
            columns: table => new
            {
                Id = table.Column<int>(type: "INTEGER", nullable: false)
                    .Annotation("Sqlite:Autoincrement", true),
                Adresse = table.Column<string>(type: "TEXT", nullable: false),
                Ville = table.Column<string>(type: "TEXT", nullable: false),
                CodePostal = table.Column<string>(type: "TEXT", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Localisations", x => x.Id);
            });

        migrationBuilder.CreateTable(
            name: "Locataires",
            columns: table => new
            {
                Id = table.Column<int>(type: "INTEGER", nullable: false)
                    .Annotation("Sqlite:Autoincrement", true),
                Nom = table.Column<string>(type: "TEXT", nullable: false),
                Prenom = table.Column<string>(type: "TEXT", nullable: false),
                Telephone = table.Column<string>(type: "TEXT", nullable: false),
                Email = table.Column<string>(type: "TEXT", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Locataires", x => x.Id);
            });

        migrationBuilder.CreateTable(
            name: "Biens",
            columns: table => new
            {
                Id = table.Column<int>(type: "INTEGER", nullable: false)
                    .Annotation("Sqlite:Autoincrement", true),
                Reference = table.Column<string>(type: "TEXT", nullable: false),
                Type = table.Column<int>(type: "INTEGER", nullable: false),
                Surface = table.Column<double>(type: "REAL", nullable: false),
                NbPieces = table.Column<int>(type: "INTEGER", nullable: false),
                LoyerBase = table.Column<double>(type: "REAL", nullable: false),
                Statut = table.Column<int>(type: "INTEGER", nullable: false),
                LocalisationId = table.Column<int>(type: "INTEGER", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Biens", x => x.Id);
                table.ForeignKey(
                    name: "FK_Biens_Localisations_LocalisationId",
                    column: x => x.LocalisationId,
                    principalTable: "Localisations",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "Contrats",
            columns: table => new
            {
                Id = table.Column<int>(type: "INTEGER", nullable: false)
                    .Annotation("Sqlite:Autoincrement", true),
                DateDebut = table.Column<DateTime>(type: "TEXT", nullable: false),
                DateFin = table.Column<DateTime>(type: "TEXT", nullable: false),
                MontantLoyer = table.Column<double>(type: "REAL", nullable: false),
                Caution = table.Column<double>(type: "REAL", nullable: false),
                BienId = table.Column<int>(type: "INTEGER", nullable: false),
                LocataireId = table.Column<int>(type: "INTEGER", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Contrats", x => x.Id);
                table.ForeignKey(
                    name: "FK_Contrats_Biens_BienId",
                    column: x => x.BienId,
                    principalTable: "Biens",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
                table.ForeignKey(
                    name: "FK_Contrats_Locataires_LocataireId",
                    column: x => x.LocataireId,
                    principalTable: "Locataires",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "Maintenances",
            columns: table => new
            {
                Id = table.Column<int>(type: "INTEGER", nullable: false)
                    .Annotation("Sqlite:Autoincrement", true),
                Description = table.Column<string>(type: "TEXT", nullable: false),
                DateDemande = table.Column<DateTime>(type: "TEXT", nullable: false),
                DateRealisation = table.Column<DateTime>(type: "TEXT", nullable: true),
                Cout = table.Column<double>(type: "REAL", nullable: false),
                Statut = table.Column<int>(type: "INTEGER", nullable: false),
                BienId = table.Column<int>(type: "INTEGER", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Maintenances", x => x.Id);
                table.ForeignKey(
                    name: "FK_Maintenances_Biens_BienId",
                    column: x => x.BienId,
                    principalTable: "Biens",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "Loyers",
            columns: table => new
            {
                Id = table.Column<int>(type: "INTEGER", nullable: false)
                    .Annotation("Sqlite:Autoincrement", true),
                Mois = table.Column<int>(type: "INTEGER", nullable: false),
                Annee = table.Column<int>(type: "INTEGER", nullable: false),
                MontantDu = table.Column<double>(type: "REAL", nullable: false),
                MontantPaye = table.Column<double>(type: "REAL", nullable: false),
                DatePaiement = table.Column<DateTime>(type: "TEXT", nullable: true),
                Statut = table.Column<int>(type: "INTEGER", nullable: false),
                ContratId = table.Column<int>(type: "INTEGER", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Loyers", x => x.Id);
                table.ForeignKey(
                    name: "FK_Loyers_Contrats_ContratId",
                    column: x => x.ContratId,
                    principalTable: "Contrats",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateIndex(
            name: "IX_Biens_LocalisationId",
            table: "Biens",
            column: "LocalisationId");

        migrationBuilder.CreateIndex(
            name: "IX_Biens_Reference",
            table: "Biens",
            column: "Reference",
            unique: true);

        migrationBuilder.CreateIndex(
            name: "IX_Contrats_BienId",
            table: "Contrats",
            column: "BienId");

        migrationBuilder.CreateIndex(
            name: "IX_Contrats_LocataireId",
            table: "Contrats",
            column: "LocataireId");

        migrationBuilder.CreateIndex(
            name: "IX_Loyers_ContratId",
            table: "Loyers",
            column: "ContratId");

        migrationBuilder.CreateIndex(
            name: "IX_Maintenances_BienId",
            table: "Maintenances",
            column: "BienId");
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "Loyers");

        migrationBuilder.DropTable(
            name: "Maintenances");

        migrationBuilder.DropTable(
            name: "Contrats");

        migrationBuilder.DropTable(
            name: "Biens");

        migrationBuilder.DropTable(
            name: "Locataires");

        migrationBuilder.DropTable(
            name: "Localisations");
    }
}
