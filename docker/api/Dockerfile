FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build

WORKDIR /src

COPY apps/api/ ./apps/api/

RUN dotnet restore apps/api/src/CaseManagement.Api/CaseManagement.Api.csproj
RUN dotnet publish apps/api/src/CaseManagement.Api/CaseManagement.Api.csproj -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime

WORKDIR /app

COPY --from=build /app/publish .

ENV ASPNETCORE_URLS=http://0.0.0.0:5202

EXPOSE 5202

ENTRYPOINT ["dotnet", "CaseManagement.Api.dll"]
