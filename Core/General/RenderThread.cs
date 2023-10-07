using Rin.Core.Abstractions;
using Rin.Rendering;

namespace Rin.Core.General;

public sealed class RenderThread : IRenderThread, IDisposable {
    readonly ThreadingPolicy threadingPolicy;
    readonly Thread thread;

    RenderThreadState currentState = RenderThreadState.Idle;

    public bool IsRunning { get; private set; }

    public RenderThread(ThreadingPolicy threadingPolicy) {
        this.threadingPolicy = threadingPolicy;
        
        if (threadingPolicy == ThreadingPolicy.MultiThreaded) {
            thread = new(() => RenderThreadFunc(this));
            thread.Name = "Render Thread";
        }
    }

    public void Run() {
        IsRunning = true;

        if (threadingPolicy == ThreadingPolicy.MultiThreaded) {
            thread.Start();
        }
    }

    public void Terminate() {
        IsRunning = false;
        Pump();

        if (threadingPolicy == ThreadingPolicy.MultiThreaded) {
            thread.Join();
        }
    }

    public void Pump() {
        NextFrame();
        Kick();
        BlockUntilRenderComplete();
    }

    public void NextFrame() => Renderer.SwapQueues();

    public void Kick() {
        if (threadingPolicy == ThreadingPolicy.MultiThreaded) {
            Set(RenderThreadState.Kick);
        } else {
            Renderer.WaitAndRender(this);
        }
    }

    public void BlockUntilRenderComplete() {
        if (threadingPolicy == ThreadingPolicy.SingleThreaded) {
            return;
        }

        lock (this) {
            while (currentState != RenderThreadState.Idle) {
                Monitor.Wait(this);
            }
        }
    }

    public void Set(RenderThreadState state) {
        if (threadingPolicy == ThreadingPolicy.SingleThreaded) {
            return;
        }

        lock (this) {
            currentState = state;
            Monitor.Pulse(this);
        }
    }
    
    public void WaitAndSet(RenderThreadState waitState, RenderThreadState setState) {
        if (threadingPolicy == ThreadingPolicy.SingleThreaded) {
            return;
        }

        lock (this) {
            while (currentState != waitState) {
                Monitor.Wait(this);
            }
            
            currentState = setState;
            Monitor.Pulse(this);
        }
    }

    static void RenderThreadFunc(IRenderThread thread) {
        while (thread.IsRunning) {
            Renderer.WaitAndRender(thread);
        }
    }

    public void Dispose() {
        Terminate();
    }
}
