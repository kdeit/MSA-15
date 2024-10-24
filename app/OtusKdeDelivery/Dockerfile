#RUN FROM ../
#docker build -f OtusKdeDelivery/Dockerfile --tag kdeit/asp-delivery --platform linux/amd64 .

FROM --platform=$BUILDPLATFORM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
ARG TARGETPLATFORM
ARG BUILDPLATFORM
RUN echo "I am running on $BUILDPLATFORM, building for $TARGETPLATFORM" > /log
WORKDIR /app

RUN apt-get update
RUN apt-get install realmd gss-ntlmssp -y --no-install-recommends

FROM --platform=$BUILDPLATFORM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY . .

RUN dotnet restore "OtusKdeDelivery/OtusKdeDelivery.csproj"
RUN dotnet build "OtusKdeDelivery/OtusKdeDelivery.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "OtusKdeDelivery/OtusKdeDelivery.csproj" -c Release -o /app/publish



FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
    
ENTRYPOINT ["dotnet", "OtusKdeDelivery.dll"]

