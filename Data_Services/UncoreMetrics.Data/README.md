Generate new migrations:

> dotnet ef migrations add InitialCreate  --context ServersContext --output-dir "Migrations\ServerContext" --startup-project ../V_Rising_Collector

> dotnet ef migrations script --startup-project ../V_Rising_Collector or  dotnet ef database update --startup-project ../V_Rising_Collector