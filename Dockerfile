FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
COPY ["ABC.csproj", "./"]
RUN dotnet restore "ABC.csproj"
COPY . .
RUN dotnet build "ABC.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "ABC.csproj" -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app
COPY --from=publish /app/publish .
EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080
ENTRYPOINT ["dotnet", "QuanLyThucTap.dll"]
