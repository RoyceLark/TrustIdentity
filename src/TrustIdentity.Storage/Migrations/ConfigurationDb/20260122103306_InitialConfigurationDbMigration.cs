using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TrustIdentity.Storage.Migrations.ConfigurationDb
{
    /// <inheritdoc />
    public partial class InitialConfigurationDbMigration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ApiResources",
                columns: table => new
                {
                    Name = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    DisplayName = table.Column<string>(type: "TEXT", maxLength: 200, nullable: true),
                    Description = table.Column<string>(type: "TEXT", nullable: true),
                    Enabled = table.Column<bool>(type: "INTEGER", nullable: false),
                    Scopes = table.Column<string>(type: "TEXT", nullable: false),
                    UserClaims = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ApiResources", x => x.Name);
                });

            migrationBuilder.CreateTable(
                name: "ApiScopes",
                columns: table => new
                {
                    Name = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    DisplayName = table.Column<string>(type: "TEXT", maxLength: 200, nullable: true),
                    Description = table.Column<string>(type: "TEXT", nullable: true),
                    Required = table.Column<bool>(type: "INTEGER", nullable: false),
                    Emphasize = table.Column<bool>(type: "INTEGER", nullable: false),
                    ShowInDiscoveryDocument = table.Column<bool>(type: "INTEGER", nullable: false),
                    UserClaims = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ApiScopes", x => x.Name);
                });

            migrationBuilder.CreateTable(
                name: "Clients",
                columns: table => new
                {
                    ClientId = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    Enabled = table.Column<bool>(type: "INTEGER", nullable: false),
                    ClientName = table.Column<string>(type: "TEXT", maxLength: 200, nullable: true),
                    Description = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: true),
                    ClientUri = table.Column<string>(type: "TEXT", nullable: true),
                    LogoUri = table.Column<string>(type: "TEXT", nullable: true),
                    RequireConsent = table.Column<bool>(type: "INTEGER", nullable: false),
                    AllowRememberConsent = table.Column<bool>(type: "INTEGER", nullable: false),
                    AlwaysIncludeUserClaimsInIdToken = table.Column<bool>(type: "INTEGER", nullable: false),
                    AllowedGrantTypes = table.Column<string>(type: "TEXT", nullable: false),
                    RequirePkce = table.Column<bool>(type: "INTEGER", nullable: false),
                    AllowPlainTextPkce = table.Column<bool>(type: "INTEGER", nullable: false),
                    RequireClientSecret = table.Column<bool>(type: "INTEGER", nullable: false),
                    AllowAccessTokensViaBrowser = table.Column<bool>(type: "INTEGER", nullable: false),
                    RedirectUris = table.Column<string>(type: "TEXT", nullable: false),
                    PostLogoutRedirectUris = table.Column<string>(type: "TEXT", nullable: false),
                    FrontChannelLogoutUri = table.Column<string>(type: "TEXT", nullable: true),
                    FrontChannelLogoutSessionRequired = table.Column<bool>(type: "INTEGER", nullable: false),
                    BackChannelLogoutUri = table.Column<string>(type: "TEXT", nullable: true),
                    BackChannelLogoutSessionRequired = table.Column<bool>(type: "INTEGER", nullable: false),
                    AllowOfflineAccess = table.Column<bool>(type: "INTEGER", nullable: false),
                    AllowedScopes = table.Column<string>(type: "TEXT", nullable: false),
                    IdentityTokenLifetime = table.Column<int>(type: "INTEGER", nullable: false),
                    AccessTokenLifetime = table.Column<int>(type: "INTEGER", nullable: false),
                    AuthorizationCodeLifetime = table.Column<int>(type: "INTEGER", nullable: false),
                    AbsoluteRefreshTokenLifetime = table.Column<int>(type: "INTEGER", nullable: false),
                    SlidingRefreshTokenLifetime = table.Column<int>(type: "INTEGER", nullable: false),
                    RefreshTokenUsage = table.Column<int>(type: "INTEGER", nullable: false),
                    UpdateAccessTokenClaimsOnRefresh = table.Column<bool>(type: "INTEGER", nullable: false),
                    RefreshTokenExpiration = table.Column<int>(type: "INTEGER", nullable: false),
                    AccessTokenType = table.Column<int>(type: "INTEGER", nullable: false),
                    EnableLocalLogin = table.Column<bool>(type: "INTEGER", nullable: false),
                    IdentityProviderRestrictions = table.Column<string>(type: "TEXT", nullable: false),
                    IncludeJwtId = table.Column<bool>(type: "INTEGER", nullable: false),
                    AlwaysSendClientClaims = table.Column<bool>(type: "INTEGER", nullable: false),
                    ClientClaimsPrefix = table.Column<string>(type: "TEXT", nullable: true),
                    PairWiseSubjectSalt = table.Column<string>(type: "TEXT", nullable: true),
                    UserSsoLifetime = table.Column<int>(type: "INTEGER", nullable: true),
                    UserCodeType = table.Column<string>(type: "TEXT", nullable: true),
                    DeviceCodeLifetime = table.Column<int>(type: "INTEGER", nullable: false),
                    AllowedCorsOrigins = table.Column<string>(type: "TEXT", nullable: false),
                    LastAccessed = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Clients", x => x.ClientId);
                });

            migrationBuilder.CreateTable(
                name: "IdentityResources",
                columns: table => new
                {
                    Name = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    DisplayName = table.Column<string>(type: "TEXT", maxLength: 200, nullable: true),
                    Description = table.Column<string>(type: "TEXT", nullable: true),
                    Enabled = table.Column<bool>(type: "INTEGER", nullable: false),
                    Required = table.Column<bool>(type: "INTEGER", nullable: false),
                    Emphasize = table.Column<bool>(type: "INTEGER", nullable: false),
                    ShowInDiscoveryDocument = table.Column<bool>(type: "INTEGER", nullable: false),
                    UserClaims = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IdentityResources", x => x.Name);
                });

            migrationBuilder.CreateTable(
                name: "ClientClaim",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Type = table.Column<string>(type: "TEXT", nullable: false),
                    Value = table.Column<string>(type: "TEXT", nullable: false),
                    ValueType = table.Column<string>(type: "TEXT", nullable: true),
                    ClientId = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ClientClaim", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ClientClaim_Clients_ClientId",
                        column: x => x.ClientId,
                        principalTable: "Clients",
                        principalColumn: "ClientId");
                });

            migrationBuilder.CreateTable(
                name: "Secret",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Value = table.Column<string>(type: "TEXT", nullable: false),
                    Type = table.Column<string>(type: "TEXT", nullable: false),
                    Description = table.Column<string>(type: "TEXT", nullable: true),
                    Expiration = table.Column<DateTime>(type: "TEXT", nullable: true),
                    ApiResourceName = table.Column<string>(type: "TEXT", nullable: true),
                    ClientId = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Secret", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Secret_ApiResources_ApiResourceName",
                        column: x => x.ApiResourceName,
                        principalTable: "ApiResources",
                        principalColumn: "Name");
                    table.ForeignKey(
                        name: "FK_Secret_Clients_ClientId",
                        column: x => x.ClientId,
                        principalTable: "Clients",
                        principalColumn: "ClientId");
                });

            migrationBuilder.CreateIndex(
                name: "IX_ClientClaim_ClientId",
                table: "ClientClaim",
                column: "ClientId");

            migrationBuilder.CreateIndex(
                name: "IX_Clients_ClientId",
                table: "Clients",
                column: "ClientId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Secret_ApiResourceName",
                table: "Secret",
                column: "ApiResourceName");

            migrationBuilder.CreateIndex(
                name: "IX_Secret_ClientId",
                table: "Secret",
                column: "ClientId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ApiScopes");

            migrationBuilder.DropTable(
                name: "ClientClaim");

            migrationBuilder.DropTable(
                name: "IdentityResources");

            migrationBuilder.DropTable(
                name: "Secret");

            migrationBuilder.DropTable(
                name: "ApiResources");

            migrationBuilder.DropTable(
                name: "Clients");
        }
    }
}
