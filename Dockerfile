FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Tìm và hiển thị các tệp csproj để debug
RUN find /src -name "*.csproj" || echo "Không tìm thấy file csproj nào"

# Sao chép tệp csproj trước (giả sử là ABC.csproj, nếu không tìm thấy sẽ thử QuanLyThucTap.csproj)
COPY ["ABC.csproj", "./"] 2>/dev/null || echo "Không tìm thấy ABC.csproj, thử tìm QuanLyThucTap.csproj..."
COPY ["QuanLyThucTap.csproj", "./"] 2>/dev/null || echo "Không tìm thấy QuanLyThucTap.csproj"

# Liệt kê file hiện có để debug
RUN ls -la

# Khôi phục các gói phụ thuộc
RUN dotnet restore "*.csproj" || echo "Không thể khôi phục dependencies"

# Sao chép toàn bộ mã nguồn
COPY . .

# Hiển thị cấu trúc thư mục để debug
RUN find . -type f -name "*.csproj" | sort

# Build dự án
RUN dotnet build "*.csproj" -c Release -o /app/build || echo "Không thể build dự án"

# Publish dự án
FROM build AS publish
RUN dotnet publish "*.csproj" -c Release -o /app/publish || echo "Không thể publish dự án"

# Sử dụng runtime để chạy ứng dụng
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app
COPY --from=publish /app/publish .

# Liệt kê các tệp trong thư mục publish để debug
RUN ls -la

# Thiết lập biến môi trường và port
EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080

# Tìm tệp dll chính để chạy
RUN for file in *.dll; do echo "Tìm thấy: $file"; done
ENTRYPOINT ["dotnet", "ABC.dll"]
