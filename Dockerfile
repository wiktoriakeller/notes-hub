#Build stage
FROM mcr.microsoft.com/dotnet/sdk:6.0-focal AS build
WORKDIR /source

COPY ./NotesApp/ .
RUN rm -rf notes-ui
RUN dotnet restore
RUN dotnet publish -c Release -o /app --no-restore

FROM mcr.microsoft.com/dotnet/sdk:6.0-focal
WORKDIR /app
COPY --from=build  /app ./

EXPOSE 5000

ENTRYPOINT ["dotnet", "NotesApp.WebAPI.dll"]
