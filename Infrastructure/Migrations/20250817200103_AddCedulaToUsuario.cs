using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    public partial class AddCedulaToUsuario : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Cedula",
                table: "Usuarios",
                type: "character varying(20)",
                maxLength: 20,
                nullable: true);
            
            migrationBuilder.CreateIndex(
                name: "IX_Usuario_Cedula",
                table: "Usuarios",
                column: "Cedula",
                unique: true,
                filter: "\"Cedula\" IS NOT NULL");

            migrationBuilder.AlterColumn<int>(
                name: "Estado",
                table: "Citas",
                type: "integer",
                nullable: false,
                comment: "0=Ninguno, 1=Renovacion, 2=PrimeraVez, 3=Duplicado, 4=CambioCategoria\"",
                oldClrType: typeof(int),
                oldType: "integer",
                oldComment: "1=Confirmada, 2=Cancelada, 3=Pendiente");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Usuario_Cedula",
                table: "Usuarios");

            migrationBuilder.DropColumn(
                name: "Cedula",
                table: "Usuarios");

            migrationBuilder.AlterColumn<int>(
                name: "Estado",
                table: "Citas",
                type: "integer",
                nullable: false,
                comment: "1=Confirmada, 2=Cancelada, 3=Pendiente",
                oldClrType: typeof(int),
                oldType: "integer",
                oldComment: "0=Ninguno, 1=Renovacion, 2=PrimeraVez, 3=Duplicado, 4=CambioCategoria\"");
        }
    }
}