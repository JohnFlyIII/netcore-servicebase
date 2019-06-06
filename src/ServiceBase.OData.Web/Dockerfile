FROM microsoft/dotnet:2.1-aspnetcore-runtime
ENV ASPNETCORE_URLS http://+:80
COPY . /app
WORKDIR /app
EXPOSE 80
ENTRYPOINT ["dotnet", "ServiceBase.OData.Web.dll"]
