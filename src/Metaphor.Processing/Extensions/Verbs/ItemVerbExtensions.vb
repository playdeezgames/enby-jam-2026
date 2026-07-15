Imports System.Runtime.CompilerServices
Imports Metaphor.Persistence

Friend Module ItemVerbExtensions
    Private Delegate Function CanPerformHandler(verb As IVerb, item As IItem) As Boolean
    Private Delegate Sub PerformHandler(verb As IVerb, item As IItem)

    Private ReadOnly canPerformTable As New Dictionary(Of String, CanPerformHandler) From
        {
        }

    <Extension>
    Friend Function CanPerform(verb As IVerb, item As IItem) As Boolean
        Dim handler As CanPerformHandler = Nothing
        If canPerformTable.TryGetValue(verb.VerbType, handler) Then
            Return handler.Invoke(verb, item)
        End If
        Return True
    End Function

    Private ReadOnly performTable As New Dictionary(Of String, PerformHandler) From
        {
        }

    <Extension>
    Sub Perform(verb As IVerb, item As IItem)
        Dim handler As PerformHandler = Nothing
        verb.World.AddMessage(verb.Flavor)
        If performTable.TryGetValue(verb.VerbType, handler) Then
            handler.Invoke(verb, item)
            Return
        End If
    End Sub

End Module
