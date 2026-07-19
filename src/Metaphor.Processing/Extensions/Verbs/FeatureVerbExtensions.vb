Imports System.Runtime.CompilerServices
Imports Metaphor.Persistence

Friend Module FeatureVerbExtensions
    Private Delegate Function CanPerformHandler(verb As IVerb, feature As IFeature) As Boolean
    Private Delegate Sub PerformHandler(verb As IVerb, feature As IFeature)

    Private ReadOnly canPerformTable As New Dictionary(Of String, CanPerformHandler) From
        {
            {VerbTypes.PICK_FLOWER, AddressOf CanPickFlower}
        }

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
            {VerbTypes.PICK_FLOWER, AddressOf HandlePickFlower}
        }

    Private Sub HandlePickFlower(verb As IVerb, feature As IFeature)
        Dim world = verb.World
        Dim character = world.Avatar
        Dim item = character.Inventory.CreateItem(ItemTypes.FLOWER, "Flower", "This is a flower. You picked it from a flower patch. Murderer.")
        world.AddMessage($"{character.Name} picks {item.Name}.")
        feature.ChangeCounter(Counters.FLOWERS_REMAINING, -1)
        If feature.IsCounterMinimum(Counters.FLOWERS_REMAINING) Then
            world.AddMessage($"{character.Name} has completely eliminated the flower patch. Way to be a genocidal maniac!")
            feature.Remove()
        End If
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
