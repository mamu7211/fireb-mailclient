var builder = DistributedApplication.CreateBuilder(args);

var postgres = builder.AddPostgres("postgres")
    .WithDataVolume()
    .WithPgAdmin()
    .AddDatabase("mailclientdb");

var ollama = builder.AddOllama("ollama")
    .WithDataVolume()
    .AddModel("qwen3:4b");

var mailpit = builder.AddMailPit("mailpit");

builder.AddProject<Projects.Feirb_Api>("api")
    .WithReference(postgres)
    .WithReference(ollama)
    .WithReference(mailpit)
    .WaitFor(postgres);

builder.Build().Run();
