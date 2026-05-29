using System.Text;
using PremixOrderAPI.Data;

LoadDotEnvFile();
var builder = WebApplication.CreateBuilder(args);
builder.Configuration.AddEnvironmentVariables();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddScoped<DatabaseConnection>();
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("AllowAll");
app.UseDefaultFiles();
app.UseStaticFiles(new StaticFileOptions
{
    OnPrepareResponse = context =>
    {
        context.Context.Response.Headers["Cache-Control"] = "no-store, no-cache, must-revalidate, max-age=0";
        context.Context.Response.Headers["Pragma"] = "no-cache";
        context.Context.Response.Headers["Expires"] = "0";
    }
});
app.UseAuthorization();
app.MapControllers();

app.Run();

static void LoadDotEnvFile()
{
    var searchDirectory = new DirectoryInfo(AppContext.BaseDirectory);
    for (var i = 0; i < 6 && searchDirectory != null; i++, searchDirectory = searchDirectory.Parent)
    {
        var path = Path.Combine(searchDirectory.FullName, ".env");
        if (!File.Exists(path)) continue;

        foreach (var rawLine in File.ReadAllLines(path, Encoding.UTF8))
        {
            var line = rawLine.Trim();
            if (string.IsNullOrWhiteSpace(line) || line.StartsWith('#')) continue;
            if (line.StartsWith("export ", StringComparison.OrdinalIgnoreCase))
            {
                line = line[7..].Trim();
            }

            var equalsIndex = line.IndexOf('=');
            if (equalsIndex <= 0) continue;
            var key = line[..equalsIndex].Trim();
            var value = line[(equalsIndex + 1)..].Trim().Trim('"').Trim('\'');
            if (!string.IsNullOrWhiteSpace(key))
            {
                Environment.SetEnvironmentVariable(key, value);
            }
        }

        return;
    }
}