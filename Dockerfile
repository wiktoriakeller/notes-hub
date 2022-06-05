#Build stage
FROM mcr.microsoft.com/dotnet/sdk:6.0-focal AS build
WORKDIR /source
COPY . .
RUN dotnet restore "./NotesApp/NotesApp.sln" --disable-parallel
RUN dotnet publish "./NotesApp/NotesApp.sln" -c release -o /build-app --no-restore

#Serve stage
FROM mcr.microsoft.com/dotnet/sdk:6.0-focal AS build
WORKDIR /app
COPY --from=build  /build-app ./

EXPOSE 5000

ENTRYPOINT ["dotnet", "NotesApp.WebAPI.dll"]
