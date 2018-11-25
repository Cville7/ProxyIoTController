public interface IDevice {
    string ID { get; set; }
    string Name { get; set; }

    void SetDeviceName(string name);
}

public interface IOperation {
    string OperationName { get; }
    void Run();
}
