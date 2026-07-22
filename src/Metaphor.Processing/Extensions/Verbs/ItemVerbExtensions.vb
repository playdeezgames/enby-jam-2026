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
            {VerbTypes.STOP_AND_SMELL, AddressOf HandleStopAndSmell},
            {VerbTypes.REACH_IN, AddressOf HandleReachIn},
            {VerbTypes.DRINK, AddressOf HandleDrink}
        }

    Private Sub HandleDrink(verb As IVerb, item As IItem)
        Dim world = verb.World
        Dim avatar = world.Avatar
        world.AddMessage($"{avatar.Name} consumes {item.Name}.")
        world.AddMessage($"{avatar.Name}'s fatigue is completely gone!")
        avatar.SetCounter(Counters.FATIGUE, avatar.GetCounterMinimum(Counters.FATIGUE))
        item.Remove()
        avatar.Inventory.CreateItem(ItemTypes.LITTER, "Litter", "This is litter. Don't drop it on the ground and leave it there, or yer a bad person!")
    End Sub

    Private Sub HandleReachIn(verb As IVerb, item As IItem)
        Dim world = verb.World
        Dim avatar = world.Avatar
        Dim snax = item.GetCounter(Counters.SNAX)
        If snax > 0 Then
            item.ChangeCounter(Counters.SNAX, -1)
            avatar.ChangeCounter(Counters.SNAX, 1)
            world.AddMessage($"{avatar.Name} finds 1 snax!")
            world.AddMessage($"{avatar.Name} has {avatar.GetSnax()} snax.")
        Else
            world.AddMessage($"{avatar.Name} finds nothing!")
        End If
    End Sub

    Private Sub HandleStopAndSmell(verb As IVerb, item As IItem)
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
