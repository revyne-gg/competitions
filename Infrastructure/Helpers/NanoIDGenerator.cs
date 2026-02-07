using System.Diagnostics;
using competitions.Application.Ports;

namespace competitions.Infrastructure.Helpers;

public class NanoIdGenerator: IIDGenerator
{
    public async Task<string> Generate()
    {
        return await NanoidDotNet.Nanoid.GenerateAsync();
    }
}