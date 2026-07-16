Imports System.Runtime.CompilerServices
Imports Metaphor.Persistence

Friend Module CharacterVerbExtensions
    Private Delegate Function CanPerformHandler(verb As IVerb, character As ICharacter) As Boolean
    Private Delegate Sub PerformHandler(verb As IVerb, character As ICharacter)

    Private ReadOnly canPerformTable As New Dictionary(Of String, CanPerformHandler) From
        {
        }

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
            {VerbTypes.CHANGE_PACE, AddressOf HandleChangePace}
        }

    Private Sub HandleChangePace(verb As IVerb, character As ICharacter)
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
