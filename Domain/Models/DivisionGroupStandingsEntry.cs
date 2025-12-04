namespace leagues.Domain.Models;

public record DivisionGroupStandingsEntry(
    uint Position,
    string TeamId,
    uint MatchWins,
    uint MatchLosses,
    uint MatchTies,
    uint MapWins,
    uint MapLosses,
    uint MapTies,
    uint Points
);