using Editor.Editor;
using Editor.General;
using Serilog;
using Serilog.Exceptions;

Log.Logger = new LoggerConfiguration()
    .Enrich.FromLogContext()
    .Enrich.WithExceptionDetails()
    // .Enrich.WithProperty("Environment", builder.Configuration.GetSection("environment").Value)
    .WriteTo.Console()
    .CreateLogger();

var project = new Project("/Users/jiu/Desktop/test_rin");
var editor = new EditorManager(project);

editor.Watch();

Log.Information("Bar");

var app = new Application();
app.Run();


Log.Information("Foo");

Console.ReadLine();
