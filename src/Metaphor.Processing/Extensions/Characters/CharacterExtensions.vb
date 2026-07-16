Imports System.Runtime.CompilerServices
Imports Metaphor.Persistence

Friend Module CharacterExtensions
    <Extension>
    Private Function IsAvatar(character As ICharacter) As Boolean
        Return If(character.World.Avatar?.EntityId = character.EntityId, False)
    End Function
    <Extension>
    Friend Sub Look(character As ICharacter)
        Dim location = character.Location
        Dim world = location.World
        world.AddMessage(location.Flavor)
        ShowJourneyStatistics(character)
        ShowOtherCharacters(character)
        ShowFeatures(character)
        If location.Inventory.HasItems Then
            world.AddMessage("There are items on the ground.")
        End If
    End Sub

    Private Sub ShowJourneyStatistics(character As ICharacter)
        Dim world = character.World
        world.AddMessage($"Distance Remaining: {character.GetDistanceRemaining()}.")
        world.AddMessage($"Pace: {character.GetPace()}.")
    End Sub

    <Extension>
    Friend Sub ShowOtherCharacters(character As ICharacter)
        Dim others = character.Location.GetOtherCharacters(character)
        If others.Any Then
            character.World.AddMessage("Characters:")
            For Each other In others
                character.World.AddMessage($"- {other.Name}")
            Next
        End If
    End Sub

    <Extension>
    Friend Sub ShowFeatures(character As ICharacter)
        Dim features = character.Location.Features
        If features.Any Then
            character.World.AddMessage($"Features:")
            For Each feature In features
                character.World.AddMessage($"- {feature.Name}")
            Next
        End If
    End Sub
    <Extension>
    Friend Function IsDead(character As ICharacter) As Boolean
        Return character.IsCounterMinimum(Counters.HEALTH)
    End Function
    <Extension>
    Friend Sub TakeDamage(character As ICharacter, damage As Integer)
        character.ChangeCounter(Counters.HEALTH, -damage)
    End Sub
    <Extension>
    Friend Function GetHealth(character As ICharacter) As Integer
        Return character.GetCounter(Counters.HEALTH)
    End Function
    <Extension>
    Friend Function GetStomach(character As ICharacter) As Integer
        Return character.GetCounter(Counters.STOMACH)
    End Function
    <Extension>
    Friend Function GetSatiety(character As ICharacter) As Integer
        Return character.GetCounter(Counters.SATIETY)
    End Function
    <Extension>
    Friend Function IsStomachEmpty(character As ICharacter) As Boolean
        Return character.IsCounterMinimum(Counters.STOMACH)
    End Function
    <Extension>
    Friend Function GetJools(character As ICharacter) As Integer
        Return character.GetCounter(Counters.JOOLS)
    End Function
    <Extension>
    Friend Function GetDistanceRemaining(character As ICharacter) As Integer
        Return character.GetCounter(Counters.DISTANCE_REMAINING)
    End Function
    <Extension>
    Friend Function GetPace(character As ICharacter) As Integer
        Return character.GetCounter(Counters.PACE)
    End Function
    <Extension>
    Friend Function GetMaximumHealth(character As ICharacter) As Integer
        Return character.GetCounterMaximum(Counters.HEALTH)
    End Function
    <Extension>
    Friend Function GetMaximumStomach(character As ICharacter) As Integer
        Return character.GetCounterMaximum(Counters.STOMACH)
    End Function
    <Extension>
    Friend Function GetMaximumSatiety(character As ICharacter) As Integer
        Return character.GetCounterMaximum(Counters.SATIETY)
    End Function
    <Extension>
    Friend Sub ShowStatus(character As ICharacter)
        Dim world = character.World
        world.AddMessage($"{character.Name}'s Status:")
        world.AddMessage(character.Flavor)
        ShowJourneyStatistics(character)
        world.AddMessage($"- Health: {character.GetHealth()}/{character.GetMaximumHealth()}")
        world.AddMessage($"- Satiety: {character.GetSatiety()}/{character.GetMaximumSatiety()}")
        If Not character.IsStomachEmpty() Then
            world.AddMessage($"- Stomach: {character.GetStomach()}/{character.GetMaximumStomach()}")
        End If
        world.AddMessage($"- Jools: {character.GetJools()}")
    End Sub
End Module
