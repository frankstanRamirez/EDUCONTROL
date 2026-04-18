using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EDUCONTROL.Migrations
{
    /// <inheritdoc />
    public partial class AgregarSeccionAUsuario : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "SeccionAsignada",
                table: "Usuarios",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SeccionAsignada",
                table: "Usuarios");
        }
    }
}
