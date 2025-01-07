var builder = DistributedApplication.CreateBuilder(args);



var seq = builder.AddSeq("seq")
    .WithDataBindMount(
        source: @"E:\projeto\seq\Data",
        isReadOnly: false)
    .ExcludeFromManifest()
    .WithLifetime(ContainerLifetime.Persistent)
    .WithEnvironment("ACCEPT_EULA", "Y");

var sqlPassword = builder.AddParameter("sqlPassword", secret: true);

var sql = builder.AddSqlServer("sql", password: sqlPassword, port: 50749)
    .WithDataBindMount(
        source: @"E:\projeto\sql\Data",
        isReadOnly: false);

var db = sql.AddDatabase("sqldata", "keycloak-db");


var adminUser= builder.AddParameter("adminUsername");
var adminPassword = builder.AddParameter("adminPassword", secret: true);

var keycloak = builder.AddKeycloak("keycloak",8080,adminUser,adminPassword)
    .WithRealmImport("../realms/")
    .WithDataBindMount(@"E:\projeto\keycloak\Data");

//var keycloak = builder
//    .AddKeycloakContainer("keycloak")
//    .WithImport("../realms/weathershop-realm.json")
//    .WithReference(sql)
//    .WaitFor(sql);


var apiService = builder.AddProject<Projects.Dmi_ApiService>("apiservice")
    .WithExternalHttpEndpoints()
    .WithReference(seq)
    .WithReference(keycloak)
    .WaitFor(keycloak);

builder.AddProject<Projects.Dmi_Web>("webfrontend")
    .WithExternalHttpEndpoints()
    .WithReference(apiService)
    .WaitFor(apiService)
    .WithReference(keycloak)
    .WaitFor(keycloak);

builder.Build().Run();
