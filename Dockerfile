# 1. Môi trường Build
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy file solution và các file .csproj để restore packages trước (giúp tận dụng cache của Docker)
COPY ["TechGearAuction.slnx", "./"]
COPY ["TechGearAuction.API/TechGearAuction.API.csproj", "TechGearAuction.API/"]
COPY ["TechGearAuction.Application/TechGearAuction.Application.csproj", "TechGearAuction.Application/"]
COPY ["TechGearAuction.Domain/TechGearAuction.Domain.csproj", "TechGearAuction.Domain/"]
COPY ["TechGearAuction.Infrastructure/TechGearAuction.Infrastructure.csproj", "TechGearAuction.Infrastructure/"]
RUN dotnet restore

# Copy toàn bộ code còn lại vào và tiến hành build
COPY . .
WORKDIR "/src/TechGearAuction.API"
RUN dotnet build -c Release -o /app/build

# 2. Môi trường Publish
FROM build AS publish
RUN dotnet publish -c Release -o /app/publish /p:UseAppHost=false

# 3. Môi trường Runtime (Chỉ chứa môi trường chạy, giúp file image siêu nhẹ)
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
# Expose port mặc định của ASP.NET Core
EXPOSE 8080 
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "TechGearAuction.API.dll"]