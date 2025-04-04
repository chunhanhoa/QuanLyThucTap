FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Sao chép tệp csproj
COPY ["ABC.csproj", "./"]

# Liệt kê file hiện có để debug
RUN ls -la

# Khôi phục các gói phụ thuộc
RUN dotnet restore "ABC.csproj"

# Sao chép toàn bộ mã nguồn
COPY . .

# Hiển thị cấu trúc thư mục để debug
RUN find . -type f -name "*.csproj" | sort

# Build dự án
RUN dotnet build "ABC.csproj" -c Release -o /app/build

# Publish dự án
FROM build AS publish
RUN dotnet publish "ABC.csproj" -c Release -o /app/publish

# Sử dụng runtime để chạy ứng dụng
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app
COPY --from=publish /app/publish .

# Liệt kê các tệp trong thư mục publish để debug
RUN ls -la

# Thiết lập biến môi trường và port
EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080

# Chạy ứng dụng
ENTRYPOINT ["dotnet", "ABC.dll"]
