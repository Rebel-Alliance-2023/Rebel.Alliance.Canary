var builder = DistributedApplication.CreateBuilder(args);

builder.AddProject<Projects.BlazorCanary>("blazorcanary");

builder.Build().Run();
