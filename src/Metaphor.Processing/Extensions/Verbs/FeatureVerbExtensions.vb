Imports System.Runtime.CompilerServices
Imports Metaphor.Persistence

Friend Module FeatureVerbExtensions
    Private Delegate Function CanPerformHandler(verb As IVerb, feature As IFeature) As Boolean
    Private Delegate Sub PerformHandler(verb As IVerb, feature As IFeature)

    Private ReadOnly canPerformTable As New Dictionary(Of String, CanPerformHandler) From
        {
        }

    <Extension>
    Friend Function CanPerform(verb As IVerb, feature As IFeature) As Boolean
        Dim handler As CanPerformHandler = Nothing
        If canPerformTable.TryGetValue(verb.VerbType, handler) Then
            Return handler.Invoke(verb, feature)
        End If
        Return True
    End Function

    Private ReadOnly performTable As New Dictionary(Of String, PerformHandler) From
        {
        }

    <Extension>
    Sub Perform(verb As IVerb, feature As IFeature)
        Dim handler As PerformHandler = Nothing
        verb.World.AddMessage(verb.Flavor)
        If performTable.TryGetValue(verb.VerbType, handler) Then
            handler.Invoke(verb, feature)
            Return
        End If
    End Sub
End Module
