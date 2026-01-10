FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# копируем csproj файлы (важно для Clean Architecture)
COPY ./src/DirectoryService.API/DirectoryService.API.csproj ./DirectoryService.API/
COPY ./src/DirectoryService.Application/DirectoryService.Application.csproj ./DirectoryService.Application/
COPY ./src/DirectoryService.Infrastructure/DirectoryService.Infrastructure.csproj ./DirectoryService.Infrastructure/
COPY ./src/DirectoryService.Domain/DirectoryService.Domain.csproj ./DirectoryService.Domain/

RUN dotnet restore ./DirectoryService.API/DirectoryService.API.csproj

# копируем остальной код
COPY . .
WORKDIR /src/src/DirectoryService.API

RUN dotnet publish -c Release -o /out

FROM base AS final
WORKDIR /app
COPY --from=build /out .
ENTRYPOINT ["dotnet", "DirectoryService.API.dll"]
