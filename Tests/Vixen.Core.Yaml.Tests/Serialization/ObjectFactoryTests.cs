using Vixen.Core.Yaml.Serialization;
using Xunit;

namespace Vixen.Core.Yaml.Tests.Serialization;

public class ObjectFactoryTests {
    [Fact]
    public void NotSpecifyingObjectFactoryUsesDefault() {
        var settings = new SerializerSettings();
        settings.RegisterTagMapping("!foo", typeof(FooBase));
        var serializer = new Serializer(settings);
        var result = serializer.Deserialize(new StringReader("!foo {}"));

        Assert.True(result is FooBase);
    }

    [Fact]
    public void ObjectFactoryIsInvoked() {
        var settings = new SerializerSettings {
            ObjectFactory = new LambdaObjectFactory(t => new FooDerived(), new DefaultObjectFactory())
        };
        settings.RegisterTagMapping("!foo", typeof(FooBase));

        var serializer = new Serializer(settings);

        var result = serializer.Deserialize(new StringReader("!foo {}"));

        Assert.True(result is FooDerived);
    }

    public class FooBase { }

    public class FooDerived : FooBase { }
}
