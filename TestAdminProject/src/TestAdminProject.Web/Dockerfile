#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY . .
RUN dotnet restore "src/TestAdminProject.Web/TestAdminProject.Web.csproj"
WORKDIR "/src/src/TestAdminProject.Web"
RUN dotnet build "TestAdminProject.Web.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "TestAdminProject.Web.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "TestAdminProject.Web.dll"]
