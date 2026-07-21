Imports System.Runtime.CompilerServices
Imports Metaphor.Persistence
Imports TGGD.Processing

Friend Module CharacterVerbExtensions
    Private Delegate Function CanPerformHandler(verb As IVerb, character As ICharacter) As Boolean
    Private Delegate Sub PerformHandler(verb As IVerb, character As ICharacter)

    Private ReadOnly canPerformTable As New Dictionary(Of String, CanPerformHandler) From
        {
            {VerbTypes.CHANGE_PACE, AddressOf IsNotDead},
            {VerbTypes.CONTINUE_JOURNEY, AddressOf IsNotDead},
            {VerbTypes.COMPLETE_JOURNEY, AddressOf IsJourneyComplete},
            {VerbTypes.EAT_SNAX, AddressOf CanEatSnax},
            {VerbTypes.GIVE_FLOWER, AddressOf CanGiveFlower},
            {VerbTypes.TAKE_NAP, AddressOf CanTakeNap}
        }

    Private Function CanTakeNap(verb As IVerb, character As ICharacter) As Boolean
        Return character.GetFatigue() >= (character.GetMaximumFatigue() \ 2)
    End Function

    Private Function CanGiveFlower(verb As IVerb, character As ICharacter) As Boolean
        Dim avatar = verb.World.Avatar
        Return character.EntityType = CharacterTypes.TRAEHI AndAlso avatar.Inventory.Items.Any(Function(x) x.EntityType = ItemTypes.FLOWER)
    End Function

    Private Function CanEatSnax(verb As IVerb, character As ICharacter) As Boolean
        Return Not character.IsDead AndAlso character.HasSnax
    End Function

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
            {VerbTypes.COMPLETE_JOURNEY, AddressOf HandleCompleteJourney},
            {VerbTypes.EAT_SNAX, AddressOf HandleEatSnax},
            {VerbTypes.FORAGE, AddressOf HandleForage},
            {VerbTypes.GIVE_FLOWER, AddressOf HandleGiveFlower},
            {VerbTypes.TAKE_NAP, AddressOf HandleTakeNap}
        }

    Private Sub HandleTakeNap(verb As IVerb, character As ICharacter)
        Const FATIGUE_PER_HOUR = 10
        Dim world = verb.World
        Dim hours = character.GetFatigue() \ FATIGUE_PER_HOUR
        world.AddMessage($"{character.Name} naps for {hours} hours.")
        character.ApplyHunger(hours)
        world.AddMessage($"{character.Name} awakens from their nap refreshed!")
        character.SetCounter(Counters.FATIGUE, 0)
    End Sub

    Private Sub HandleGiveFlower(verb As IVerb, character As ICharacter)
        Dim world = verb.World
        Dim avatar = world.Avatar
        Dim item = avatar.Inventory.Items.First(Function(x) x.EntityType = ItemTypes.FLOWER)
        item.Remove()
        world.AddMessage($"{avatar.Name} gives {character.Name} a flower.")
        world.AddMessage($"{character.Name} says `Thank you!`, and gives {avatar.Name} 1 jools.")
        avatar.ChangeCounter(Counters.JOOLS, 1)
        world.AddMessage($"{avatar.Name} now has {avatar.GetJools} jools.")
    End Sub

    Private Sub HandleForage(verb As IVerb, character As ICharacter)
        Dim world = character.World
        world.AddMessage($"{character.Name} forages...")
        Dim location = character.Location
        Dim forageRoll = character.RollForageSkill()
        world.AddMessage($"{character.Name} rolls a forage of {forageRoll}.")
        If forageRoll >= location.GetForagingDifficulty() Then
            world.AddMessage($"{character.Name} finds 1 snax!")
            character.ChangeCounter(Counters.SNAX, 1)
            world.AddMessage($"{character.Name} now has {character.GetSnax()} snax.")
            location.SetCounter(Counters.FORAGING_DIFFICULTY, forageRoll + 1)
        End If
        character.ApplyHunger(1)
        character.ApplyFatigue(1)
        character.EarnForagingExperience(1)
    End Sub

    Private Sub HandleEatSnax(verb As IVerb, character As ICharacter)
        Const SNAX_BENEFIT = 10
        Dim world = character.World
        world.AddMessage($"{character.Name} eats 1 snax.")
        character.ChangeCounter(Counters.SNAX, -1)
        world.AddMessage($"{character.Name} has {character.GetSnax()} snax remaining.")
        character.ChangeCounter(Counters.STOMACH, SNAX_BENEFIT)
        world.AddMessage($"{character.Name}'s stomach goes up by {SNAX_BENEFIT} to {character.GetStomach()}.")
        If character.IsCounterMaximum(Counters.STOMACH) Then
            world.AddMessage($"{character.Name}'s bewwy hertz.")
            character.ChangeCounter(Counters.BEWWY_HERTZ, RNG.RollDice("2d6"))
        End If
    End Sub

    Private Sub HandleCompleteJourney(verb As IVerb, character As ICharacter)
        character.SetTag(Tags.JOURNEY_COMPLETE)
    End Sub

    Private Sub HandleContinueJourney(verb As IVerb, character As ICharacter)
        Dim world = character.World
        Dim pace = character.GetPace()
        Dim effectivePace = character.GetEffectivePace()
        If Not character.IsCounterMinimum(Counters.BEWWY_HERTZ) Then
            world.AddMessage($"{character.Name}'s bewwy still hertz, so effective pace will be slower.")
            character.ChangeCounter(Counters.BEWWY_HERTZ, -1)
        End If
        world.AddMessage($"{character.Name} walks {effectivePace} miles.")
        character.ChangeCounter(Counters.DISTANCE_REMAINING, -effectivePace)
        world.AddMessage($"{character.Name} has {character.GetDistanceRemaining()} miles left to go.")
        character.ApplyHunger(pace)
        character.ApplyFatigue(pace)
        character.Location.Update()
        character.Look()
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
