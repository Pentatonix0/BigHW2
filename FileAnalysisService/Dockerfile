FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["FileAnalysisService.csproj", "."]
RUN dotnet restore "./FileAnalysisService.csproj"
COPY . .
RUN dotnet build "FileAnalysisService.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "FileAnalysisService.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
EXPOSE 8080
COPY --from=publish /app/publish .

USER root
RUN mkdir -p /app/analysis_storage && chown -R app:app /app/analysis_storage
USER app 

ENTRYPOINT ["dotnet", "FileAnalysisService.dll"]