using mlstack;

double teststackscore = LearningStacks.ExampleStack();

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

app.MapGet("/api", () => $"R²:   {teststackscore:0.###}");

app.Run();
