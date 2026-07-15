Imports System.Runtime.CompilerServices
Imports Metaphor.Persistence

Friend Module CharacterDeathExtensions
    Private Delegate Sub DeathHandler(character As ICharacter)
    Private ReadOnly deathHandlers As New Dictionary(Of String, DeathHandler) From
        {
            {CharacterTypes.RAT, AddressOf RatDeath}
        }

    Private Sub RatDeath(character As ICharacter)
        Dim world = character.World
        Dim location = character.Location
        Dim item = location.Inventory.CreateItem(ItemTypes.RAT_TAIL, AddressOf InitializeRatTail)
        world.AddMessage($"As {character.GetName()} dies, it drops {item.GetName()} on the ground.")
    End Sub

    Private Sub InitializeRatTail(item As IItem)
        item.SetName("Rat Tail")
        item.SetFlavor("This is a rat tail whose former owner can no longer be found, so you can keep it if you want. Weirdo.")
    End Sub

    <Extension>
    Friend Sub Die(character As ICharacter)
        Dim handler As DeathHandler = Nothing
        If deathHandlers.TryGetValue(character.CharacterType, handler) Then
            handler.Invoke(character)
        End If
        character.Remove()
    End Sub

End Module
