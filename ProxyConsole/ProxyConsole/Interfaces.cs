using System;
using System.Collections.Generic;

public interface IService {
    string Name { get; set; }
    List<IDevice> Devices { get; set; }
}

public interface IDevice {
    string ID { get; set; }
    string Name { get; set; }

    List<Operation> Operations { get; set; }
    List<Operation> GetAvailableOperations();
}

public class Operation {
    public object[] ParamTypes { get; set; }
    public string Name { get; set; }
    public ParamsAction Run { get; set; }
}

public delegate void ParamsAction(params object[] arguments);