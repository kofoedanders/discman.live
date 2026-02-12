# Build image
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS builder

RUN apt-get update -yq 
RUN apt-get install curl gnupg -yq 

RUN curl -sL https://deb.nodesource.com/setup_22.x |  bash -
RUN apt-get install -y nodejs build-essential

# Copy files
WORKDIR /app
COPY . .

# Build backend
WORKDIR /app/src/Web
RUN dotnet restore
RUN dotnet publish --configuration Release -o /app/out

# Build frontend
WORKDIR /app/src/Web/ClientApp
RUN npm install
RUN npm run build
RUN mkdir -p /app/out/wwwroot
RUN cp -r build/* /app/out/wwwroot/

# Build runtime image
FROM mcr.microsoft.com/dotnet/aspnet:9.0
WORKDIR /app
COPY --from=builder /app/out .
ENV ASPNETCORE_URLS="http://*:80"
ENV ASPNETCORE_ENVIRONMENT="Production"
ENTRYPOINT ["dotnet", "Web.dll"]
