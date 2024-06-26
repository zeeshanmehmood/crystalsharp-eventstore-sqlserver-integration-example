using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using CrystalSharp;
using CrystalSharp.MsSql.Extensions;
using CrystalSharp.MsSql.Migrator;
using CrystalSharpEventStoreSqlServerIntegrationExample.Application.CommandHandlers;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

string eventStoreConnectionString = builder.Configuration.GetConnectionString("AppEventStoreConnectionString");
MsSqlSettings eventStoreDbSettings = new(eventStoreConnectionString);

IResolver resolver = CrystalSharpAdapter.New(builder.Services)
    .AddCqrs(typeof(CreateProductCommandHandler))
    .AddMsSqlEventStoreDb<int>(eventStoreDbSettings)
    .CreateResolver();

IMsSqlDatabaseMigrator databaseMigrator = resolver.Resolve<IMsSqlDatabaseMigrator>();

MsSqlEventStoreSetup.Run(databaseMigrator, eventStoreDbSettings.ConnectionString).Wait();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
