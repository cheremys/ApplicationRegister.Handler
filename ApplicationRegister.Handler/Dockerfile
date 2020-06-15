#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/core/runtime:3.1-buster-slim AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/core/sdk:3.1-buster AS build
WORKDIR /src
COPY ["ApplicationRegister.Handler/ApplicationRegister.Handler.csproj", "ApplicationRegister.Handler/"]
RUN dotnet restore "ApplicationRegister.Handler/ApplicationRegister.Handler.csproj"
COPY . .
WORKDIR "/src/ApplicationRegister.Handler"
RUN dotnet build "ApplicationRegister.Handler.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "ApplicationRegister.Handler.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "ApplicationRegister.Handler.dll"]