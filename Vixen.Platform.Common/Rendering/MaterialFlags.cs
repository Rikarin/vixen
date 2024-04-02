namespace Vixen.Platform.Common.Rendering;

[Flags]
public enum MaterialFlags {
    None = 0,
    DepthText = 1 << 0,
    Blend = 1 << 1,
    TwoSided = 1 << 2,
    DisableShadowCasting = 1 << 4
}
