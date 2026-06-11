using competitions.Application.Engines;
using Revyne.Engine.Api;

namespace competitions.Tests.UseCases;

/// <summary>
/// Builds the open reference engine for use-case tests, so the tests exercise the
/// baseline fixture-generation logic that ships with the service.
/// </summary>
internal static class TestEngine
{
    public static ICompetitionEngine Create() => new ReferenceCompetitionEngine();
}
