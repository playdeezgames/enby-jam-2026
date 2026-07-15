Imports System.Runtime.CompilerServices
Imports Metaphor.Persistence
Imports TGGD.Processing

Friend Module CharacterExtensions
    <Extension>
    Private Function IsAvatar(character As ICharacter) As Boolean
        Return If(character.World.Avatar?.CharacterId = character.CharacterId, False)
    End Function
    <Extension>
    Friend Function IsRat(character As ICharacter) As Boolean
        Return character.CharacterType = CharacterTypes.RAT
    End Function
    <Extension>
    Friend Sub Look(character As ICharacter)
        Dim location = character.Location
        character.World.AddMessage($"{character.GetName()} is in {location.GetName()}!")
        character.World.AddMessage(location.GetFlavor())
        ShowOtherCharacters(character)
        ShowExits(character)
        ShowFeatures(character)
        If location.Inventory.HasItems Then
            character.World.AddMessage("There are items on the ground.")
        End If
    End Sub

    <Extension>
    Friend Sub ShowOtherCharacters(character As ICharacter)
        Dim others = character.Location.GetOtherCharacters(character)
        If others.Any Then
            character.World.AddMessage("Characters:")
            For Each other In others
                character.World.AddMessage($"- {other.GetName}")
            Next
        End If
    End Sub

    <Extension>
    Friend Sub ShowFeatures(character As ICharacter)
        Dim features = character.Location.Features
        If features.Any Then
            character.World.AddMessage($"Features:")
            For Each feature In features
                character.World.AddMessage($"- {feature.GetName}")
            Next
        End If
    End Sub

    <Extension>
    Friend Sub ShowExits(character As ICharacter)
        Dim routes = character.Location.Routes
        If routes.Any() Then
            character.World.AddMessage($"Exits:")
            For Each route In routes
                character.World.AddMessage($"- {route.Direction}({route.GetName})")
            Next
        End If
    End Sub
    <Extension>
    Friend Sub Attack(attacker As ICharacter, defender As ICharacter)
        Dim world = attacker.World
        world.AddMessage($"{attacker.GetName} attacks {defender.GetName}!")
        Dim attackRoll = attacker.RollAttack()
        world.AddMessage($"{attacker.GetName} rolls an attack of {attackRoll}!")
        Dim defendRoll = defender.RollDefend()
        world.AddMessage($"{defender.GetName} rolls a defend of {defendRoll}!")
        Dim damage = Math.Max(0, attackRoll - defendRoll)
        If damage > 0 Then
            world.AddMessage($"{defender.GetName} takes {damage} damage!")
            defender.TakeDamage(damage)
            If defender.IsDead Then
                world.AddMessage($"{attacker.GetName} kills {defender.GetName}!")
            Else
                world.AddMessage($"{defender.GetName} has {defender.GetHealth()}/{defender.GetMaximumHealth()} health left.")
            End If
        Else
            world.AddMessage($"{attacker.GetName} misses!")
        End If
    End Sub
    <Extension>
    Friend Function IsDead(character As ICharacter) As Boolean
        Return character.IsCounterMinimum(Counters.HEALTH)
    End Function
    <Extension>
    Friend Function RollAttack(character As ICharacter) As Integer
        Return RNG.RollDice(character.GetMetadata(Metadatas.ATTACK_ROLL))
    End Function
    <Extension>
    Friend Function RollDefend(character As ICharacter) As Integer
        Return RNG.RollDice(character.GetMetadata(Metadatas.DEFEND_ROLL))
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
    Friend Sub AttemptRun(character As ICharacter)
        Dim world = character.World
        world.AddMessage($"{character.GetName()} attempts to flee!")
        Dim route = RNG.FromEnumerable(character.Location.Routes)
        route.AttemptTake(character)
    End Sub
    <Extension>
    Friend Sub ShowStatus(character As ICharacter)
        Dim world = character.World
        world.AddMessage($"{character.GetName}'s Status:")
        world.AddMessage($"- Health: {character.GetHealth()}/{character.GetMaximumHealth()}")
        world.AddMessage($"- Satiety: {character.GetSatiety()}/{character.GetMaximumSatiety()}")
        If Not character.IsStomachEmpty() Then
            world.AddMessage($"- Stomach: {character.GetStomach()}/{character.GetMaximumStomach()}")
        End If
        world.AddMessage($"- Jools: {character.GetJools()}")
    End Sub
End Module
