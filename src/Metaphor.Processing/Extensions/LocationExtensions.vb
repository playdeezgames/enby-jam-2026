Imports System.Runtime.CompilerServices
Imports Metaphor.Persistence
Imports TGGD.Processing

Friend Module LocationExtensions
    <Extension>
    Friend Sub GenerateForagingDifficulty(location As ILocation)
        location.SetCounter(Counters.FORAGING_DIFFICULTY, RNG.RollDice("2d10"))
    End Sub
    <Extension>
    Friend Function GetForagingDifficulty(location As ILocation) As Integer
        Return location.GetCounter(Counters.FORAGING_DIFFICULTY)
    End Function
End Module
