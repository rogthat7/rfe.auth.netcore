FROM  mcr.microsoft.com/dotnet/sdk:3.1 AS build-env
WORKDIR /app
 
COPY RFE.Auth.Core/RFE.Auth.Core.csproj ./
COPY RFE.Auth.Infrastructure/RFE.Auth.Infrastructure ./
COPY RFE.Auth.API/RFE.Auth.API.csproj ./
RUN dotnet restore