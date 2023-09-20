namespace Rin.Core.General;

// TODO: Not sure if this class is really needed as we can move this functionality right into the component itself
public abstract class Behaviour : Component {
    public bool Enabled { get; set; }
}
