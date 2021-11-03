FROM  mcr.microsoft.com/dotnet/sdk:3.1 AS build-env
WORKDIR /app
 
COPY RFE.Auth.Core/RFE.Auth.Core.csproj ./RFE.Auth.Core/RFE.Auth.Core.csproj
COPY RFE.Auth.Infrastructure/RFE.Auth.Infrastructure.csproj ./RFE.Auth.Infrastructure/RFE.Auth.Infrastructure.csproj
COPY RFE.Auth.API/RFE.Auth.API.csproj ./RFE.Auth.API/RFE.Auth.API.csproj
COPY *.sln ./
RUN dotnet restore RFE.Auth.Netcore.sln

COPY . ./
RUN dotnet publish -c Release -o out

FROM mcr.microsoft.com/dotnet/core/aspnet:3.1
WORKDIR /app
EXPOSE 80
COPY --from=build-env /app/out .

ENTRYPOINT [ "dotnet", "RFE.Auth.API.dll" ]
