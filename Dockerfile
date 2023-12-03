FROM mcr.microsoft.com/dotnet/core/aspnet:3.1-buster

WORKDIR /app

COPY . .

RUN dotnet publish -c Release

EXPOSE 80

CMD ["dotnet", "run"]