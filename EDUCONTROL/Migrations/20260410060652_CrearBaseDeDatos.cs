using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EDUCONTROL.Migrations
{
    /// <inheritdoc />
    public partial class CrearBaseDeDatos : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Notas_AlumnoId",
                table: "Notas");

            migrationBuilder.DropColumn(
                name: "Materia",
                table: "Notas");

            migrationBuilder.AlterColumn<string>(
                name: "NombreCompleto",
                table: "Usuarios",
                type: "nvarchar(150)",
                maxLength: 150,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100);

            migrationBuilder.AddColumn<int>(
                name: "AsignaturaId",
                table: "Notas",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "Seccion",
                table: "Alumnos",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateTable(
                name: "Asignaturas",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nombre = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Grado = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    Activa = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Asignaturas", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Notas_AlumnoId_AsignaturaId",
                table: "Notas",
                columns: new[] { "AlumnoId", "AsignaturaId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Notas_AsignaturaId",
                table: "Notas",
                column: "AsignaturaId");

            migrationBuilder.AddForeignKey(
                name: "FK_Notas_Asignaturas_AsignaturaId",
                table: "Notas",
                column: "AsignaturaId",
                principalTable: "Asignaturas",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Notas_Asignaturas_AsignaturaId",
                table: "Notas");

            migrationBuilder.DropTable(
                name: "Asignaturas");

            migrationBuilder.DropIndex(
                name: "IX_Notas_AlumnoId_AsignaturaId",
                table: "Notas");

            migrationBuilder.DropIndex(
                name: "IX_Notas_AsignaturaId",
                table: "Notas");

            migrationBuilder.DropColumn(
                name: "AsignaturaId",
                table: "Notas");

            migrationBuilder.DropColumn(
                name: "Seccion",
                table: "Alumnos");

            migrationBuilder.AlterColumn<string>(
                name: "NombreCompleto",
                table: "Usuarios",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(150)",
                oldMaxLength: 150);

            migrationBuilder.AddColumn<string>(
                name: "Materia",
                table: "Notas",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_Notas_AlumnoId",
                table: "Notas",
                column: "AlumnoId");
        }
    }
}
