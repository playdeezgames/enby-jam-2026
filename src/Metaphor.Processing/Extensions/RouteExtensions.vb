Imports System.Runtime.CompilerServices
Imports Metaphor.Persistence

Friend Module RouteExtensions
    Private Delegate Function AttemptHandler(character As ICharacter, route As IRoute) As Boolean
    Private ReadOnly attemptHandlerTable As New Dictionary(Of String, AttemptHandler) From
        {
            {RouteTypes.INN_CELLAR, AddressOf HandleInnCellarAttempt}
        }

    Private Function HandleInnCellarAttempt(character As ICharacter, route As IRoute) As Boolean
        If Not character.HasTag(Tags.QUEST_RATS) Then
            character.World.AddMessage("I don't think Gorachan wants you in cellar.")
            Return False
        End If
        Return True
    End Function
    <Extension>
    Friend Function AttemptTake(route As IRoute, character As ICharacter) As Boolean
        Dim world = route.World
        Dim attemptHandler As AttemptHandler = Nothing
        If attemptHandlerTable.TryGetValue(route.RouteType, attemptHandler) Then
            If Not attemptHandler.Invoke(character, route) Then
                Return False
            End If
        End If
        character.Location = route.Destination
        character.World.AddMessage(route.GetFlavor())
        Return True
    End Function
End Module
