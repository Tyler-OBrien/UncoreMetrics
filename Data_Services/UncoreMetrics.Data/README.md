Generate new migrations:

>  dotnet ef migrations add InitialCreate  --context ServersContext --output-dir "Migrations\ServerContext" --startup-project ../../Collector_Services/V_Rising_Collector

> dotnet ef migrations script --startup-project  ../../Collector_Services/V_Rising_Collector or  dotnet ef database update --startup-project  ../../Collector_Services/V_Rising_Collector