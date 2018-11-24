using System.Collections.Generic;

public interface IDevice {
    string ID { get; set; }
    string Name { get; set; }
}

public interface ILight : IDevice {
    void ToggleLight();
}

public interface ILightHub : IDevice {
    List<ILight> Lights { get; set; }
    List<ILight> FindLights();
}

public interface IOperation {
    string OperationName { get; }
    void Run();
}
