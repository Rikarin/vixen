using Rin.Core.General;
using Rin.Editor;
using Serilog;
using Serilog.Exceptions;

Log.Logger = new LoggerConfiguration()
    .Enrich.FromLogContext()
    .Enrich.WithExceptionDetails()
    // .Enrich.WithProperty("Environment", builder.Configuration.GetSection("environment").Value)
    .WriteTo.Console()
    .CreateLogger();

var project = new Project("../Examples/Project1");
var editor = new EditorManager(project);

editor.Watch();

Log.Information("Bar");

var app = new Application();
app.Run();


Log.Information("Foo");

Console.ReadLine();
