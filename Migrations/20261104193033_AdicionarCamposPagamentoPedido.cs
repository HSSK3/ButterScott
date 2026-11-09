
using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ButterScott.Migrations
{
    public partial class AdicionarCamposPagamentoPedido : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "forma_pagamento",
                table: "pedidos",
                type: "longtext",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "status_pagamento",
                table: "pedidos",
                type: "longtext",
                nullable: true);

            migrationBuilder.Sql(
                "UPDATE `pedidos` SET " +
                "`forma_pagamento` = '', " +
                "`status_pagamento` = 'Aguardando Pagamento'");

            migrationBuilder.AlterColumn<string>(
                name: "forma_pagamento",
                table: "pedidos",
                type: "longtext",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "longtext",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "status_pagamento",
                table: "pedidos",
                type: "longtext",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "longtext",
                oldNullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "forma_pagamento",
                table: "pedidos");

            migrationBuilder.DropColumn(
                name: "status_pagamento",
                table: "pedidos");
        }
    }
}
