using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class MigrarDeHospitalARenovacionCarnet : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Citas_Turnos_TurnoId",
                table: "Citas");

            migrationBuilder.DropForeignKey(
                name: "FK_Configuraciones_Turnos_TurnoId",
                table: "Configuraciones");

            migrationBuilder.DropIndex(
                name: "IX_Citas_UsuarioId",
                table: "Citas");

            migrationBuilder.AlterColumn<string>(
                name: "Email",
                table: "Usuarios",
                type: "character varying(150)",
                maxLength: 150,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "ContrasenaHash",
                table: "Usuarios",
                type: "character varying(255)",
                maxLength: 255,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "Nombre",
                table: "Turnos",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "Descripcion",
                table: "Estaciones",
                type: "character varying(200)",
                maxLength: 200,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<DateTime>(
                name: "Fecha",
                table: "Configuraciones",
                type: "date",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone");

            migrationBuilder.AlterColumn<DateTime>(
                name: "FechaHora",
                table: "Citas",
                type: "timestamp",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone");

            migrationBuilder.AlterColumn<int>(
                name: "Estado",
                table: "Citas",
                type: "integer",
                nullable: false,
                comment: "1=Confirmada, 2=Cancelada, 3=Pendiente",
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AddColumn<int>(
                name: "TipoTramite",
                table: "Citas",
                type: "integer",
                maxLength: 50,
                nullable: false,
                defaultValue: 0,
                comment: "Renovacion, PrimeraVez, Duplicado, CambioCategoria");

            migrationBuilder.CreateIndex(
                name: "IX_Usuario_Email",
                table: "Usuarios",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Estacion_Numero",
                table: "Estaciones",
                column: "Numero",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Configuracion_Fecha_Turno",
                table: "Configuraciones",
                columns: new[] { "Fecha", "TurnoId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Cita_Estado",
                table: "Citas",
                column: "Estado");

            migrationBuilder.CreateIndex(
                name: "IX_Cita_FechaHora_Estacion",
                table: "Citas",
                columns: new[] { "FechaHora", "EstacionNumero" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Cita_Usuario_Fecha",
                table: "Citas",
                columns: new[] { "UsuarioId", "FechaHora" });

            migrationBuilder.AddForeignKey(
                name: "FK_Citas_Turnos_TurnoId",
                table: "Citas",
                column: "TurnoId",
                principalTable: "Turnos",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Configuraciones_Turnos_TurnoId",
                table: "Configuraciones",
                column: "TurnoId",
                principalTable: "Turnos",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Citas_Turnos_TurnoId",
                table: "Citas");

            migrationBuilder.DropForeignKey(
                name: "FK_Configuraciones_Turnos_TurnoId",
                table: "Configuraciones");

            migrationBuilder.DropIndex(
                name: "IX_Usuario_Email",
                table: "Usuarios");

            migrationBuilder.DropIndex(
                name: "IX_Estacion_Numero",
                table: "Estaciones");

            migrationBuilder.DropIndex(
                name: "IX_Configuracion_Fecha_Turno",
                table: "Configuraciones");

            migrationBuilder.DropIndex(
                name: "IX_Cita_Estado",
                table: "Citas");

            migrationBuilder.DropIndex(
                name: "IX_Cita_FechaHora_Estacion",
                table: "Citas");

            migrationBuilder.DropIndex(
                name: "IX_Cita_Usuario_Fecha",
                table: "Citas");

            migrationBuilder.DropColumn(
                name: "TipoTramite",
                table: "Citas");

            migrationBuilder.AlterColumn<string>(
                name: "Email",
                table: "Usuarios",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(150)",
                oldMaxLength: 150);

            migrationBuilder.AlterColumn<string>(
                name: "ContrasenaHash",
                table: "Usuarios",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(255)",
                oldMaxLength: 255);

            migrationBuilder.AlterColumn<string>(
                name: "Nombre",
                table: "Turnos",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(50)",
                oldMaxLength: 50);

            migrationBuilder.AlterColumn<string>(
                name: "Descripcion",
                table: "Estaciones",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(200)",
                oldMaxLength: 200);

            migrationBuilder.AlterColumn<DateTime>(
                name: "Fecha",
                table: "Configuraciones",
                type: "timestamp with time zone",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "date");

            migrationBuilder.AlterColumn<DateTime>(
                name: "FechaHora",
                table: "Citas",
                type: "timestamp with time zone",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "timestamp");

            migrationBuilder.AlterColumn<int>(
                name: "Estado",
                table: "Citas",
                type: "integer",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer",
                oldComment: "1=Confirmada, 2=Cancelada, 3=Pendiente");

            migrationBuilder.CreateIndex(
                name: "IX_Citas_UsuarioId",
                table: "Citas",
                column: "UsuarioId");

            migrationBuilder.AddForeignKey(
                name: "FK_Citas_Turnos_TurnoId",
                table: "Citas",
                column: "TurnoId",
                principalTable: "Turnos",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Configuraciones_Turnos_TurnoId",
                table: "Configuraciones",
                column: "TurnoId",
                principalTable: "Turnos",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
