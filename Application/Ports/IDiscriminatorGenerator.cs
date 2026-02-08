namespace competitions.Application.Ports;

public interface IDiscriminatorGenerator
{
    public Task<string> Generate();
}