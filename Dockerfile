FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
COPY ["samples/QuickStart/QuickStart.csproj", "QuickStart/"]
COPY ["src/TrustIdentity.Core/TrustIdentity.Core.csproj", "TrustIdentity.Core/"]
COPY ["src/TrustIdentity.AspNetCore/TrustIdentity.AspNetCore.csproj", "TrustIdentity.AspNetCore/"]
RUN dotnet restore "QuickStart/QuickStart.csproj"
COPY . .
WORKDIR "/src/QuickStart"
RUN dotnet build "QuickStart.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "QuickStart.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "QuickStart.dll"]