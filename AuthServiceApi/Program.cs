using Application.Interfaces.RoleInterfaces;
using Application.Interfaces.UserInterfaces;
using Application.UseCase.RoleUseCase;
using Application.UseCase.UserUseCase;
using Infrastructure.Commands.UserCommand;
using Infrastructure.Persistence;
using Infrastructure.Querys.RoleQuery;
using Infrastructure.Querys.UserQuery;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<AppDbContext>(options => options.UseSqlServer(connectionString));

builder.Services.AddScoped<IUserCommand, UserCommand>();
builder.Services.AddScoped<IUserQuery, UserQuery>();
builder.Services.AddScoped<IUserService, UserService>();

builder.Services.AddScoped<IRoleQuery, RoleQuery>();
builder.Services.AddScoped<IRoleService, RoleService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
