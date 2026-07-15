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
            {VerbTypes.REACH_IN, AddressOf ReachIn}
        }

    Private Sub ReachIn(verb As IVerb, item As IItem)
        Dim world = verb.World
        If item.HasTag(Tags.EMPTY) Then
            world.AddMessage($"{item.GetName} is empty.")
            Return
        End If
        item.SetTag(Tags.EMPTY)
        Dim avatar = world.Avatar
        avatar.Inventory.CreateItem(ItemTypes.BANANA, AddressOf InitializeBanana)
        avatar.Inventory.CreateItem(ItemTypes.ORANGE, AddressOf InitializeOrange)
        avatar.Inventory.CreateItem(ItemTypes.ORANGE, AddressOf InitializeOrange)
        world.AddMessage($"{avatar.GetName()} finds a banana and two oranges. ;)")
    End Sub

    Private Sub InitializeOrange(item As IItem)
        item.SetName("Orange")
        item.SetFlavor("It's an orange. It is round, and pleasant to touch.")
        item.SetCounter(Counters.STOMACH, 20)
    End Sub

    Private Sub InitializeBanana(item As IItem)
        item.SetName("Banana")
        item.SetFlavor("It's a good sized banana. Its firm, and feels nice in yer hand.")
        item.SetCounter(Counters.STOMACH, 20)
    End Sub

    <Extension>
    Sub Perform(verb As IVerb, item As IItem)
        Dim handler As PerformHandler = Nothing
        verb.World.AddMessage(verb.GetFlavor())
        If performTable.TryGetValue(verb.VerbType, handler) Then
            handler.Invoke(verb, item)
            Return
        End If
    End Sub

End Module
