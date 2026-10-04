FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG SERVICE
WORKDIR /source
COPY Directory.Build.props Directory.Packages.props ./
COPY src/ src/
RUN dotnet publish src/${SERVICE}/CinemaBooking.${SERVICE}.Api -c Release -o /app

FROM mcr.microsoft.com/dotnet/aspnet:10.0
ARG SERVICE
WORKDIR /app
COPY --from=build /app ./
RUN mkdir logs && chown $APP_UID logs
ENV ASPNETCORE_HTTP_PORTS=8080
ENV SERVICE_ASSEMBLY=CinemaBooking.${SERVICE}.Api.dll
USER $APP_UID
ENTRYPOINT ["sh", "-c", "exec dotnet $SERVICE_ASSEMBLY"]
