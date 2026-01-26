#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:NETCOREPAL_DOCKER_FRAMEWORK AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:NETCOREPAL_DOCKER_FRAMEWORK AS build
WORKDIR /src
COPY . .
RUN dotnet restore "src/ABC.Template.Web/ABC.Template.Web.csproj"
WORKDIR "/src/src/ABC.Template.Web"
RUN dotnet build "ABC.Template.Web.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "ABC.Template.Web.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "ABC.Template.Web.dll"]
