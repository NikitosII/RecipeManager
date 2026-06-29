FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /repo

COPY Directory.Packages.props ./
COPY global.json ./
COPY src/RecipeManager.Domain/RecipeManager.Domain.csproj src/RecipeManager.Domain/
COPY src/RecipeManager.Application/RecipeManager.Application.csproj src/RecipeManager.Application/
COPY src/RecipeManager.Infrastructure/RecipeManager.Infrastructure.csproj src/RecipeManager.Infrastructure/
COPY src/RecipeManager.Api/RecipeManager.Api.csproj src/RecipeManager.Api/
RUN dotnet restore src/RecipeManager.Api/RecipeManager.Api.csproj

COPY src/ src/
RUN dotnet publish src/RecipeManager.Api/RecipeManager.Api.csproj -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "RecipeManager.Api.dll"]
