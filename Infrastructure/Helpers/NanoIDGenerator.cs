using System.Diagnostics;
using competitions.Application.Ports;
using NanoidDotNet;

namespace competitions.Infrastructure.Helpers;

public class NanoIdGenerator: IIDGenerator, IDiscriminatorGenerator
{
    async Task<string> IIDGenerator.Generate()
    {
        return await NanoidDotNet.Nanoid.GenerateAsync();
    }

    async Task<string> IDiscriminatorGenerator.Generate()
    {
        return await NanoidDotNet.Nanoid.GenerateAsync(Nanoid.Alphabets.UppercaseLettersAndDigits, 6);
    }
}