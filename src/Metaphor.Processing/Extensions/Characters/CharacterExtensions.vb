Imports System.Runtime.CompilerServices
Imports Metaphor.Persistence
Imports TGGD.Processing

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
        world.AddMessage($"Foraging Difficulty: {location.GetForagingDifficulty()}")
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
    Friend Function HasSnax(character As ICharacter) As Boolean
        Return Not character.IsCounterMinimum(Counters.SNAX)
    End Function
    <Extension>
    Friend Function GetSnax(character As ICharacter) As Integer
        Return character.GetCounter(Counters.SNAX)
    End Function
    <Extension>
    Friend Function RollForageSkill(character As ICharacter) As Integer
        Return RNG.RollDice("1d20") + character.GetForageSkill()
    End Function
    <Extension>
    Friend Function GetForageSkill(character As ICharacter) As Integer
        Return character.GetCounter(Counters.FORAGE_SKILL)
    End Function
    <Extension>
    Friend Function GetSatiety(character As ICharacter) As Integer
        Return character.GetCounter(Counters.SATIETY)
    End Function
    <Extension>
    Friend Function GetFatigue(character As ICharacter) As Integer
        Return character.GetCounter(Counters.FATIGUE)
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
    Friend Function GetEffectivePace(character As ICharacter) As Integer
        Dim pace = character.GetCounter(Counters.PACE)
        If Not character.IsCounterMinimum(Counters.BEWWY_HERTZ) Then
            pace = (pace + character.GetCounterMinimum(Counters.PACE)) \ 2
        End If
        Return pace
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
    Friend Function GetMaximumFatigue(character As ICharacter) As Integer
        Return character.GetCounterMaximum(Counters.FATIGUE)
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
        If Not character.IsCounterMinimum(Counters.BEWWY_HERTZ) Then
            world.AddMessage($"{character.Name} bewwy hertz.")
        End If
        ShowJourneyStatistics(character)
        world.AddMessage($"- Health: {character.GetHealth()}/{character.GetMaximumHealth()}")
        world.AddMessage($"- Satiety: {character.GetSatiety()}/{character.GetMaximumSatiety()}")
        world.AddMessage($"- Fatigue: {character.GetFatigue()}/{character.GetMaximumFatigue()}")
        If Not character.IsStomachEmpty() Then
            world.AddMessage($"- Stomach: {character.GetStomach()}/{character.GetMaximumStomach()}")
        End If
        If character.HasSnax() Then
            world.AddMessage($"- Snax: {character.GetSnax()}")
        End If
        If Not character.IsCounterMaximum(Counters.DRIVING_INSTRUCTION) Then
            world.AddMessage($"- Driving Instruction: {character.GetCounterPercentage(Counters.DRIVING_INSTRUCTION)}")
        End If
        If Not character.IsCounterMinimum(Counters.LITTERING) Then
            world.AddMessage($"- Littering: {character.GetCounter(Counters.LITTERING)}")
        End If
        If Not character.IsCounterMinimum(Counters.RECYCLING) Then
            world.AddMessage($"- Recycling: {character.GetCounter(Counters.RECYCLING)}")
        End If
        world.AddMessage($"- Jools: {character.GetJools()}")
    End Sub
    <Extension>
    Friend Sub SetPronouns(character As ICharacter, pronouns As String)
        character.SetMetadata(Metadatas.PRONOUNS, pronouns)
    End Sub
    <Extension>
    Friend Sub SetPace(character As ICharacter, pace As Integer)
        character.SetCounter(Counters.PACE, pace)
        character.ClearTag(Tags.IS_CHANGING_PACE)
    End Sub
    <Extension>
    Friend Function IsJourneyComplete(character As ICharacter) As Boolean
        Return character.HasTag(Tags.JOURNEY_COMPLETE)
    End Function
    <Extension>
    Friend Sub EarnForagingExperience(character As ICharacter, xp As Integer)
        Dim world = character.World
        world.AddMessage($"{character.Name} earns {xp} foraging xp.")
        character.ChangeCounter(Counters.FORAGE_EXPERIENCE, xp)
        If character.IsCounterMaximum(Counters.FORAGE_EXPERIENCE) Then
            character.ChangeCounter(Counters.FORAGE_SKILL, 1)
            world.AddMessage($"{character.Name}'s foraging skill goes up to {character.GetForageSkill()}!")
            character.SetCounter(Counters.FORAGE_EXPERIENCE, 0)
            character.SetCounterMaximum(Counters.FORAGE_EXPERIENCE, character.GetCounterMaximum(Counters.FORAGE_EXPERIENCE) * 2)
        Else
            world.AddMessage($"{character.Name}'s foraging xp is {character.GetCounter(Counters.FORAGE_EXPERIENCE)}/{character.GetCounterMaximum(Counters.FORAGE_EXPERIENCE)}.")
        End If
    End Sub
    <Extension>
    Friend Sub ApplyFatigue(character As ICharacter, fatigue As Integer)
        Dim world = character.World
        Dim capacity = Math.Min(character.GetMaximumFatigue() - character.GetFatigue(), fatigue)
        Dim overage = fatigue - capacity
        If capacity > 0 Then
            character.ChangeCounter(Counters.FATIGUE, fatigue)
            world.AddMessage($"{character.Name} gains {capacity} fatigue.")
            world.AddMessage($"{character.Name} now has {character.GetFatigue()}/{character.GetMaximumFatigue()} fatigue.")
        End If
        If overage > 0 Then
            character.ApplyDamage(overage)
        End If
    End Sub
    <Extension>
    Friend Sub ApplyHunger(character As ICharacter, hunger As Integer)
        Dim world = character.World
        Dim stomach = Math.Min(hunger, character.GetStomach())
        hunger -= stomach
        character.ChangeCounter(Counters.STOMACH, -stomach)
        Dim satiety = Math.Min(hunger, character.GetSatiety())
        If satiety > 0 Then
            hunger -= satiety
            world.AddMessage($"{character.Name} loses {satiety} satiety.")
            character.ChangeCounter(Counters.SATIETY, -satiety)
            world.AddMessage($"{character.Name} now has {character.GetSatiety}/{character.GetMaximumSatiety} satiety.")
        ElseIf character.GetStomach() > 0 Then
            If Not character.IsCounterMaximum(Counters.SATIETY) Then
                Const SATIETY_GAIN = 1
                world.AddMessage($"{character.Name} gains {SATIETY_GAIN} satiety.")
                character.ChangeCounter(Counters.SATIETY, SATIETY_GAIN)
                world.AddMessage($"{character.Name} now has {character.GetSatiety}/{character.GetMaximumSatiety} satiety.")
            End If
        End If
        If hunger > 0 Then
            ApplyDamage(character, hunger)
        End If
    End Sub
    <Extension>
    Private Sub ApplyDamage(character As ICharacter, damage As Integer)
        Dim world = character.World
        world.AddMessage($"{character.Name} loses {damage} health.")
        character.ChangeCounter(Counters.HEALTH, -damage)
        If character.IsDead Then
            World.AddMessage($"{character.Name} is dead.")
        Else
            World.AddMessage($"{character.Name} now has {character.GetHealth}/{character.GetMaximumHealth} health.")
        End If
    End Sub
End Module
