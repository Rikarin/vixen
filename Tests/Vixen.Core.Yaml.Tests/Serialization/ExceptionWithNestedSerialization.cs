using Vixen.Core.Yaml.Serialization;
using Xunit;

namespace Vixen.Core.Yaml.Tests.Serialization;

public class ExceptionWithNestedSerialization {
    [Fact]
    public void NestedDocumentShouldDeserializeProperly() {
        var serializer = new Serializer(new() { EmitDefaultValues = true });

        // serialize AMessage
        var tw = new StringWriter();
        serializer.Serialize(tw, new AMessage { Payload = new() { X = 5, Y = 6 } });
        Dump.WriteLine(tw);

        // stick serialized AMessage in envelope and serialize it
        var e = new Env { Type = "some-type", Payload = tw.ToString() };

        tw = new();
        serializer.Serialize(tw, e);
        Dump.WriteLine(tw);

        Dump.WriteLine("${0}$", e.Payload);

        var settings = new SerializerSettings();
        settings.RegisterAssembly(typeof(Env).Assembly);
        var deserializer = new Serializer(settings);
        // deserialize envelope
        var e2 = deserializer.Deserialize<Env>(new StringReader(tw.ToString()));

        Dump.WriteLine("${0}$", e2.Payload);

        // deserialize payload - fails if EmitDefaults is set
        var message = deserializer.Deserialize<AMessage>(e2.Payload);
        Assert.NotNull(message.Payload);
        Assert.Equal(5, message.Payload.X);
        Assert.Equal(6, message.Payload.Y);
    }

    public class Env {
        public string Type { get; set; }
        public string Payload { get; set; }
    }

    public class Message<TPayload> {
        public int id { get; set; }
        public TPayload Payload { get; set; }
    }

    public class PayloadA {
        public int X { get; set; }
        public int Y { get; set; }
    }

    public class AMessage : Message<PayloadA> { }
}
