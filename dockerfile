FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /PURRNext

#Copy everything
COPY . ./

#Restore as disting layers
RUN dotnet restore

#Build and Publish a release
RUN dotnet publish -o out

#Build runtime image
#FROM mcr.microsoft.com/dotnet/runtime:9.0
FROM mcr.microsoft.com/dotnet/aspnet:9.0
WORKDIR /PURRNext

#(Test) Copies the Login script into the directory
COPY ./InternalScripts/Login.sh /PURRNext/log.sh

COPY --from=build /PURRNext/out .
ENTRYPOINT ["dotnet", "PURRNext.dll"]

#Expose port in case of connection to assistants
#EXPOSE 4096 #WEB-API


