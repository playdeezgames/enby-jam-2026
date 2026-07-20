Imports System.Runtime.CompilerServices
Imports Metaphor.Persistence
Imports TGGD.Processing

Friend Module FeatureVerbExtensions
    Private Delegate Function CanPerformHandler(verb As IVerb, feature As IFeature) As Boolean
    Private Delegate Sub PerformHandler(verb As IVerb, feature As IFeature)

    Private ReadOnly canPerformTable As New Dictionary(Of String, CanPerformHandler) From
        {
            {VerbTypes.PICK_FLOWER, AddressOf CanPickFlower},
            {VerbTypes.BUY_SNAX, AddressOf CanBuySnax}
        }

    Private Function CanBuySnax(verb As IVerb, feature As IFeature) As Boolean
        Return verb.World.Avatar.GetJools() > 0
    End Function

    Private Function CanPickFlower(verb As IVerb, feature As IFeature) As Boolean
        Return Not feature.IsCounterMinimum(Counters.FLOWERS_REMAINING)
    End Function

    <Extension>
    Friend Function CanPerform(verb As IVerb, feature As IFeature) As Boolean
        Dim handler As CanPerformHandler = Nothing
        If canPerformTable.TryGetValue(verb.EntityType, handler) Then
            Return handler.Invoke(verb, feature)
        End If
        Return True
    End Function

    Private ReadOnly performTable As New Dictionary(Of String, PerformHandler) From
        {
            {VerbTypes.PICK_FLOWER, AddressOf HandlePickFlower},
            {VerbTypes.BUY_SNAX, AddressOf HandleBuySnax}
        }

    Private Sub HandleBuySnax(verb As IVerb, feature As IFeature)
        Dim world = verb.World
        Dim character = world.Avatar
        character.ChangeCounter(Counters.JOOLS, -1)
        world.AddMessage($"{character.Name} now has {character.GetJools} jools.")
        Dim snaxCount = RNG.FromGenerator(New Dictionary(Of Integer, Integer) From
                                          {
                                            {0, 1},
                                            {1, 8},
                                            {2, 1}
                                          })
        Select Case snaxCount
            Case 0
                world.AddMessage($"The machine rips {character.Name} off!")
            Case 1
                world.AddMessage($"A package of snax thunks down!")
            Case 2
                world.AddMessage($"Two packages of snax thunk down! Bonus!")
            Case Else
                Throw New NotImplementedException
        End Select
        character.ChangeCounter(Counters.SNAX, snaxCount)
        If snaxCount > 0 Then
            world.AddMessage($"{character.Name} now has {character.GetSnax} snax.")
        End If
    End Sub

    Private Sub HandlePickFlower(verb As IVerb, feature As IFeature)
        Dim world = verb.World
        Dim character = world.Avatar
        Dim item = character.Inventory.CreateItem(ItemTypes.FLOWER, "Flower", "This is a flower. You picked it from a flower patch. Murderer.", AddressOf InitializeFlower)
        world.AddMessage($"{character.Name} picks {item.Name}.")
        feature.ChangeCounter(Counters.FLOWERS_REMAINING, -1)
        If feature.IsCounterMinimum(Counters.FLOWERS_REMAINING) Then
            world.AddMessage($"{character.Name} has completely eliminated the flower patch. Way to be a genocidal maniac!")
            feature.Remove()
        End If
    End Sub

    Private Sub InitializeFlower(item As IItem)
        item.CreateVerb(
            VerbTypes.STOP_AND_SMELL,
            "Stop and Smell",
            "Sometimes it is a good idea to take a break for moment, and enjoy the simpler things in life, like flowers that you horribly massacred and sniffing their floral corpses. Sicko.")
    End Sub

    <Extension>
    Sub Perform(verb As IVerb, feature As IFeature)
        Dim handler As PerformHandler = Nothing
        verb.World.AddMessage(verb.Flavor)
        If performTable.TryGetValue(verb.EntityType, handler) Then
            handler.Invoke(verb, feature)
            Return
        End If
    End Sub
End Module
