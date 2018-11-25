public interface IDevice {
    string ID { get; set; }
    string Name { get; set; }
}

public interface IOperation {
    string OperationName { get; }
    void Run();
}
