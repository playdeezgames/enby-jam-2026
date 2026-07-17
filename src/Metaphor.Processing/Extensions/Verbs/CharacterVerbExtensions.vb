Imports System.Runtime.CompilerServices
Imports Metaphor.Persistence

Friend Module CharacterVerbExtensions
    Private Delegate Function CanPerformHandler(verb As IVerb, character As ICharacter) As Boolean
    Private Delegate Sub PerformHandler(verb As IVerb, character As ICharacter)

    Private ReadOnly canPerformTable As New Dictionary(Of String, CanPerformHandler) From
        {
            {VerbTypes.CHANGE_PACE, AddressOf IsNotDead},
            {VerbTypes.CONTINUE_JOURNEY, AddressOf IsNotDead},
            {VerbTypes.COMPLETE_JOURNEY, AddressOf IsJourneyComplete}
        }

    Private Function IsJourneyComplete(verb As IVerb, character As ICharacter) As Boolean
        Return Not character.IsDead() AndAlso character.IsCounterMinimum(Counters.DISTANCE_REMAINING)
    End Function

    Private Function IsNotDead(verb As IVerb, character As ICharacter) As Boolean
        Return Not character.IsDead()
    End Function

    <Extension>
    Friend Function CanPerform(verb As IVerb, character As ICharacter) As Boolean
        Dim handler As CanPerformHandler = Nothing
        If canPerformTable.TryGetValue(verb.EntityType, handler) Then
            Return handler.Invoke(verb, character)
        End If
        Return True
    End Function

    Private ReadOnly performTable As New Dictionary(Of String, PerformHandler) From
        {
            {VerbTypes.CHANGE_PACE, AddressOf HandleChangePace},
            {VerbTypes.CONTINUE_JOURNEY, AddressOf HandleContinueJourney},
            {VerbTypes.COMPLETE_JOURNEY, AddressOf HandleCompleteJourney}
        }

    Private Sub HandleCompleteJourney(verb As IVerb, character As ICharacter)
        character.SetTag(Tags.JOURNEY_COMPLETE)
    End Sub

    Private Sub HandleContinueJourney(verb As IVerb, character As ICharacter)
        Dim world = character.World
        Dim pace = character.GetPace()
        world.AddMessage($"{character.Name} walks {pace} miles.")
        character.ChangeCounter(Counters.DISTANCE_REMAINING, -pace)
        world.AddMessage($"{character.Name} has {character.GetDistanceRemaining()} miles left to go.")
        Dim stomach = Math.Min(pace, character.GetStomach())
        pace -= stomach
        character.ChangeCounter(Counters.STOMACH, -stomach)
        Dim satiety = Math.Min(pace, character.GetSatiety())
        If satiety > 0 Then
            pace -= satiety
            world.AddMessage($"{character.Name} loses {satiety} satiety.")
            character.ChangeCounter(Counters.SATIETY, -satiety)
            world.AddMessage($"{character.Name} now has {character.GetSatiety}/{character.GetMaximumSatiety} satiety.")
        End If
        If pace > 0 Then
            world.AddMessage($"{character.Name} loses {pace} health.")
            character.ChangeCounter(Counters.HEALTH, -pace)
            If character.IsDead Then
                world.AddMessage($"{character.Name} is dead.")
            Else
                world.AddMessage($"{character.Name} now has {character.GetHealth}/{character.GetMaximumHealth} health.")
            End If
        End If
    End Sub

    Private Sub HandleChangePace(verb As IVerb, character As ICharacter)
        character.World.AddMessage($"{character.Name}'s current pace is {character.GetPace()}.")
        character.SetTag(Tags.IS_CHANGING_PACE)
    End Sub

    <Extension>
    Sub Perform(verb As IVerb, character As ICharacter)
        Dim handler As PerformHandler = Nothing
        verb.World.AddMessage(verb.Flavor)
        If performTable.TryGetValue(verb.EntityType, handler) Then
            handler.Invoke(verb, character)
            Return
        End If
    End Sub
End Module
