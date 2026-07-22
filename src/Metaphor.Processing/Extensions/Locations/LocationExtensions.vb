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
    <Extension>
    Private Sub RemoveNonAvatarCharacters(location As ILocation)
        For Each character In location.GetOtherCharacters(location.World.Avatar)
            character.Die()
        Next
    End Sub
    <Extension>
    Private Sub RemoveAllFeatures(location As ILocation)
        For Each feature In location.Features
            feature.Remove()
        Next
    End Sub
    <Extension>
    Private Sub RemoveAllGroundItems(location As ILocation)
        Dim avatar = location.World.Avatar
        For Each item In location.Inventory.Items
            If item.EntityType = ItemTypes.LITTER Then
                avatar.ChangeCounter(Counters.LITTERING, 1)
            End If
            item.Remove()
        Next
    End Sub

    <Extension>
    Friend Sub Update(location As ILocation)
        location.GenerateForagingDifficulty()
        location.RemoveNonAvatarCharacters()
        location.RemoveAllFeatures()
        location.RemoveAllGroundItems()
        location.ClearTag(Tags.SHORTCUT)
        location.GenerateEvent()
    End Sub
End Module
