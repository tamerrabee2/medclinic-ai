FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
EXPOSE 5000
ENV ASPNETCORE_URLS=http://+:5000

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY ["src/MedClinic.API/MedClinic.API.csproj", "src/MedClinic.API/"]
COPY ["src/MedClinic.Application/MedClinic.Application.csproj", "src/MedClinic.Application/"]
COPY ["src/MedClinic.Domain/MedClinic.Domain.csproj", "src/MedClinic.Domain/"]
COPY ["src/MedClinic.Infrastructure/MedClinic.Infrastructure.csproj", "src/MedClinic.Infrastructure/"]
COPY ["src/MedClinic.Shared/MedClinic.Shared.csproj", "src/MedClinic.Shared/"]
RUN dotnet restore "src/MedClinic.API/MedClinic.API.csproj"
COPY . .
WORKDIR "/src/src/MedClinic.API"
RUN dotnet build "MedClinic.API.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "MedClinic.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "MedClinic.API.dll"]
