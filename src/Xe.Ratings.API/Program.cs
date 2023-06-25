using Xe.Ratings.API;

using Xe.Ratings.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRatings(builder.Configuration.GetConnectionString("Postgres")!);
builder.Services.AddHostedService<CleanUpService>();

builder.Services.AddOutputCache(options =>
    options.AddPolicy("Ratings", b => b.Expire(TimeSpan.FromMinutes(60))));

builder.Services.AddAuthentication().AddJwtBearer();

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthentication();

app.UseOutputCache();

app.MapRatings();

app.Run();