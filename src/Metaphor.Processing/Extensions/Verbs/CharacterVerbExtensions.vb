Imports System.Runtime.CompilerServices
Imports Metaphor.Persistence

Friend Module CharacterVerbExtensions
    Private Delegate Function CanPerformHandler(verb As IVerb, character As ICharacter) As Boolean
    Private Delegate Sub PerformHandler(verb As IVerb, character As ICharacter)

    Private ReadOnly canPerformTable As New Dictionary(Of String, CanPerformHandler) From
        {
            {VerbTypes.CELLAR_QUEST, AddressOf CanAcceptCellarQuest},
            {VerbTypes.GIVE_RAT_TAIL, AddressOf CanGiveRatTail}
        }

    Private Function CanGiveRatTail(verb As IVerb, character As ICharacter) As Boolean
        Dim avatar = verb.World.Avatar
        Return avatar.HasTag(Tags.QUEST_RATS) AndAlso
            Not avatar.IsCounterMinimum(Counters.RAT_TAILS_REMAINING) AndAlso
            avatar.Inventory.Items.Any(Function(x) x.IsRatTail())
    End Function

    Private Function CanAcceptCellarQuest(verb As IVerb, character As ICharacter) As Boolean
        Return Not verb.World.Avatar.HasTag(Tags.QUEST_RATS)
    End Function

    <Extension>
    Friend Function CanPerform(verb As IVerb, character As ICharacter) As Boolean
        Dim handler As CanPerformHandler = Nothing
        If canPerformTable.TryGetValue(verb.VerbType, handler) Then
            Return handler.Invoke(verb, character)
        End If
        Return True
    End Function

    Private ReadOnly performTable As New Dictionary(Of String, PerformHandler) From
        {
            {VerbTypes.CELLAR_QUEST, AddressOf AcceptCellarQuest},
            {VerbTypes.GIVE_RAT_TAIL, AddressOf GiveRatTail}
        }

    Private Sub GiveRatTail(verb As IVerb, character As ICharacter)
        Dim world = verb.World
        Dim avatar = verb.World.Avatar
        Dim tails = avatar.Inventory.Items.Where(Function(x) x.IsRatTail)
        Dim tailCount = Math.Min(avatar.GetCounter(Counters.RAT_TAILS_REMAINING), tails.Count)
        tails = tails.Take(tailCount)
        world.AddMessage($"{avatar.GetName} gives {tailCount} {tails.First.GetName}(s) to {character.GetName}.")
        world.AddMessage($"{character.GetName} gives {tailCount} jools to {avatar.GetName}.")
        avatar.ChangeCounter(Counters.JOOLS, tailCount)
        avatar.ChangeCounter(Counters.RAT_TAILS_REMAINING, -tailCount)
        For Each tail In tails
            tail.Remove()
        Next
    End Sub

    Private Sub AcceptCellarQuest(verb As IVerb, character As ICharacter)
        Dim avatar = verb.World.Avatar
        avatar.SetTag(Tags.QUEST_RATS)
        avatar.World.AddMessage($"{avatar.GetName} accepts the job.")
        avatar.InitializeCounter(Counters.RAT_TAILS_REMAINING, 10, 0, 10)
    End Sub

    <Extension>
    Sub Perform(verb As IVerb, character As ICharacter)
        Dim handler As PerformHandler = Nothing
        verb.World.AddMessage(verb.GetFlavor())
        If performTable.TryGetValue(verb.VerbType, handler) Then
            handler.Invoke(verb, character)
            Return
        End If
    End Sub
End Module
