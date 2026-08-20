FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY ["UserManagementPortal.Web/UserManagementPortal.Web.csproj", "UserManagementPortal.Web/"]
COPY ["UserManagementPortal.Core/UserManagementPortal.Core.csproj", "UserManagementPortal.Core/"]
RUN dotnet restore "UserManagementPortal.Web/UserManagementPortal.Web.csproj"

COPY . .
WORKDIR "/src/UserManagementPortal.Web"
RUN dotnet publish "UserManagementPortal.Web.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
COPY --from=build /app/publish .

ENV DOTNET_HOSTBUILDER__RELOADCONFIGONCHANGE=false
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

ENTRYPOINT ["dotnet", "UserManagementPortal.Web.dll"]
