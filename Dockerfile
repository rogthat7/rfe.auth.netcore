FROM  mcr.microsoft.com/dotnet/sdk:10.0 AS build-env
WORKDIR /app
 
COPY RFE.Auth.Core/RFE.Auth.Core.csproj ./RFE.Auth.Core/RFE.Auth.Core.csproj
COPY RFE.Auth.Infrastructure/RFE.Auth.Infrastructure.csproj ./RFE.Auth.Infrastructure/RFE.Auth.Infrastructure.csproj
COPY RFE.Auth.API/RFE.Auth.API.csproj ./RFE.Auth.API/RFE.Auth.API.csproj
COPY RFE.Auth.Tests/RFE.Auth.Tests.csproj ./RFE.Auth.Tests/RFE.Auth.Tests.csproj
COPY *.sln ./
RUN dotnet restore RFE.Auth.Netcore.sln

COPY . ./
RUN dotnet publish -c Release -o out

FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app
EXPOSE 5001
EXPOSE 8080
COPY --from=build-env /app/out .

ENTRYPOINT [ "dotnet", "RFE.Auth.API.dll" ]
