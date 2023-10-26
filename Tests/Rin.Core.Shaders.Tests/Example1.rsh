package Rin.Test

import Rin.Core
import Rin.BaseShaders

// Test class
shader TestShader : ExampleBase, CustomShader {
    const val Multiplier = 42
    
    val len: int
    val test: FooBar = [1, 2, 3, 4]
    val test = [1, 2, 3, 4]
    
    var test: FooBar = [
        "string",
        'c',
        'a'
    ]
    
    init() {
        Test()
        
        for (i in 1..10) {
            FooBar(i)
        }
        
        if (a > 42) {
            Call()
        } else {
            Not()
        }
    }
    
    init(test: string?) {
        long.SomeMethod()
        
        func test(): int { }
        len = CoreClass.GetLength<int, Class>(test)
        
        len = p[42]
        len = p[1..12]
    }
    
    func Generic<int>.Test<Asd>() { }
    
    func GetLength2(): int {
        return len
    }
    
    func GetLength() => len

    func VSMain() {
        var bar = 7 + 4 * "test" / 42f
        var foo = 7 + 4 * "test" / 42f
        var tst = 7 + 4 * "test" / 42f
        //Test();
    }
    
    func TestMethod(name: string, count: int = 42): float4 {
       // var test = "string";
      //  val res = name + test;
        
        //val hash = res.GetHashCode();
    
      //  return 42.3f;
    }
}


