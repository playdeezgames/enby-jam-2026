Imports System.Runtime.CompilerServices
Imports Metaphor.Persistence

Friend Module CharacterDeathExtensions
    Private Delegate Sub DeathHandler(character As ICharacter)
    Private ReadOnly deathHandlers As New Dictionary(Of String, DeathHandler) From
        {
        }

    <Extension>
    Friend Sub Die(character As ICharacter)
        Dim handler As DeathHandler = Nothing
        If deathHandlers.TryGetValue(character.EntityType, handler) Then
            handler.Invoke(character)
        End If
        character.Remove()
    End Sub

End Module
