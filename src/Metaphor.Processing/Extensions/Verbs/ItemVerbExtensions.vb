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
        If canPerformTable.TryGetValue(verb.EntityType, handler) Then
            Return handler.Invoke(verb, item)
        End If
        Return True
    End Function

    Private ReadOnly performTable As New Dictionary(Of String, PerformHandler) From
        {
            {VerbTypes.STOP_AND_SMELL, AddressOf StopAndSmell}
        }

    Private Sub StopAndSmell(verb As IVerb, item As IItem)
        Dim world = verb.World
        world.AddMessage("Hey! Speaking of Stopping and Smelling Flowers...")
        world.AddMessage("There's a new game out! You should play it!")
        world.AddMessage(
            "Stop and Smell the Flowers, now on Steam",
            New Dictionary(Of String, String) From
            {
                {"ELEMENT_TYPE", "LINK"},
                {"URL", "https://store.steampowered.com/app/2578290/Stop_and_Smell_the_Flowers/"}
            })
    End Sub

    <Extension>
    Sub Perform(verb As IVerb, item As IItem)
        Dim handler As PerformHandler = Nothing
        verb.World.AddMessage(verb.Flavor)
        If performTable.TryGetValue(verb.EntityType, handler) Then
            handler.Invoke(verb, item)
            Return
        End If
    End Sub

End Module
