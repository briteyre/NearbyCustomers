IDistributedApplicationBuilder builder = DistributedApplication.CreateBuilder(args);

var cache = builder.AddRedis("cache");

var sqlPassword = builder.AddParameter("sql-password", secret: true);
var sql = builder.AddSqlServer("sql", password: sqlPassword)
    .WithDataVolume();
var database = sql.AddDatabase("CodeCamp");

builder.AddProject<Projects.CoreCodeCamp>("api")
    .WithReference(database)
    .WithReference(cache);

await builder.Build().RunAsync();