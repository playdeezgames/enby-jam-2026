Imports System.Runtime.CompilerServices
Imports Metaphor.Persistence
Imports TGGD.Processing

Friend Module LocationVerbExtensions
    Private Delegate Function CanPerformHandler(verb As IVerb, location As ILocation) As Boolean
    Private Delegate Sub PerformHandler(verb As IVerb, location As ILocation)

    Private ReadOnly canPerformTable As New Dictionary(Of String, CanPerformHandler) From
        {
            {VerbTypes.TAKE_SHORTCUT, AddressOf CanTakeShortcut}
        }

    Private Function CanTakeShortcut(verb As IVerb, location As ILocation) As Boolean
        Return location.HasTag(Tags.SHORTCUT)
    End Function

    <Extension>
    Friend Function CanPerform(verb As IVerb, location As ILocation) As Boolean
        Dim handler As CanPerformHandler = Nothing
        If canPerformTable.TryGetValue(verb.EntityType, handler) Then
            Return handler.Invoke(verb, location)
        End If
        Return True
    End Function

    Private ReadOnly performTable As New Dictionary(Of String, PerformHandler) From
        {
            {VerbTypes.TAKE_SHORTCUT, AddressOf HandleTakeShortcut}
        }

    Private Sub HandleTakeShortcut(verb As IVerb, location As ILocation)
        Dim world = location.World
        Dim avatar = world.Avatar
        Dim gain = RNG.RollDice("2d6+-2d6") - avatar.GetCounter(Counters.LITTERING)
        avatar.ChangeCounter(Counters.DISTANCE_REMAINING, gain)
        If gain < 0 Then
            world.AddMessage($"The shortcut removes {-gain} miles from {avatar.Name}'s journey.")
        ElseIf gain > 0 Then
            world.AddMessage($"The shortcut adds {gain} miles to {avatar.Name}'s journey.")
        Else
            world.AddMessage($"The shortcut does not help {avatar.Name}'s journey.")
        End If
        world.AddMessage($"Distance remaining: {avatar.GetDistanceRemaining()} miles.")
        avatar.ApplyHunger(1)
        avatar.ApplyFatigue(1)
        location.Update()
        avatar.Look()
    End Sub

    <Extension>
    Sub Perform(verb As IVerb, location As ILocation)
        Dim handler As PerformHandler = Nothing
        verb.World.AddMessage(verb.Flavor)
        If performTable.TryGetValue(verb.EntityType, handler) Then
            handler.Invoke(verb, location)
            Return
        End If
    End Sub

End Module
