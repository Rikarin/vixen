namespace Rin.Core.Abstractions;

public interface IRenderThread : IDisposable {
    bool IsRunning { get; }
    
    void Run();
    void Terminate();
    void Pump();
    void NextFrame();
    void Kick();
    void BlockUntilRenderComplete();

    void WaitAndSet(RenderThreadState waitState, RenderThreadState setState);
    void Set(RenderThreadState state);
}


public enum RenderThreadState {
    Idle,
    Busy,
    Kick
}