using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RecipeManager.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddIngredientNutrition : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "ManualCalories",
                table: "Recipes",
                type: "numeric",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "ManualCarbohydrates",
                table: "Recipes",
                type: "numeric",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "ManualFat",
                table: "Recipes",
                type: "numeric",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "ManualFiber",
                table: "Recipes",
                type: "numeric",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "ManualProtein",
                table: "Recipes",
                type: "numeric",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "NutritionMode",
                table: "Recipes",
                type: "integer",
                nullable: false,
                // NutritionMode.Auto (enum starts at 1); backfills existing recipes to Auto.
                defaultValue: 1);

            migrationBuilder.AddColumn<decimal>(
                name: "CaloriesPer100g",
                table: "Ingredients",
                type: "numeric(8,2)",
                precision: 8,
                scale: 2,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "CarbsPer100g",
                table: "Ingredients",
                type: "numeric(8,2)",
                precision: 8,
                scale: 2,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "DensityGramsPerMl",
                table: "Ingredients",
                type: "numeric(8,4)",
                precision: 8,
                scale: 4,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "FatPer100g",
                table: "Ingredients",
                type: "numeric(8,2)",
                precision: 8,
                scale: 2,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "FiberPer100g",
                table: "Ingredients",
                type: "numeric(8,2)",
                precision: 8,
                scale: 2,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "GramsPerPiece",
                table: "Ingredients",
                type: "numeric(8,2)",
                precision: 8,
                scale: 2,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "ProteinPer100g",
                table: "Ingredients",
                type: "numeric(8,2)",
                precision: 8,
                scale: 2,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ManualCalories",
                table: "Recipes");

            migrationBuilder.DropColumn(
                name: "ManualCarbohydrates",
                table: "Recipes");

            migrationBuilder.DropColumn(
                name: "ManualFat",
                table: "Recipes");

            migrationBuilder.DropColumn(
                name: "ManualFiber",
                table: "Recipes");

            migrationBuilder.DropColumn(
                name: "ManualProtein",
                table: "Recipes");

            migrationBuilder.DropColumn(
                name: "NutritionMode",
                table: "Recipes");

            migrationBuilder.DropColumn(
                name: "CaloriesPer100g",
                table: "Ingredients");

            migrationBuilder.DropColumn(
                name: "CarbsPer100g",
                table: "Ingredients");

            migrationBuilder.DropColumn(
                name: "DensityGramsPerMl",
                table: "Ingredients");

            migrationBuilder.DropColumn(
                name: "FatPer100g",
                table: "Ingredients");

            migrationBuilder.DropColumn(
                name: "FiberPer100g",
                table: "Ingredients");

            migrationBuilder.DropColumn(
                name: "GramsPerPiece",
                table: "Ingredients");

            migrationBuilder.DropColumn(
                name: "ProteinPer100g",
                table: "Ingredients");
        }
    }
}
