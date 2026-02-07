namespace competitions.Application.Ports;

public interface IIDGenerator
{
    public Task<string> Generate();
}