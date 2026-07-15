Imports System.Runtime.CompilerServices
Imports Metaphor.Persistence

Friend Module LocationVerbExtensions
    Private Delegate Function CanPerformHandler(verb As IVerb, location As ILocation) As Boolean
    Private Delegate Sub PerformHandler(verb As IVerb, location As ILocation)

    Private ReadOnly canPerformTable As New Dictionary(Of String, CanPerformHandler) From
        {
            {VerbTypes.SEARCH, AddressOf CanSearch}
        }

    Private Function CanSearch(verb As IVerb, location As ILocation) As Boolean
        Return Not location.Characters.Any(Function(x) x.IsRat())
    End Function

    <Extension>
    Friend Function CanPerform(verb As IVerb, location As ILocation) As Boolean
        Dim handler As CanPerformHandler = Nothing
        If canPerformTable.TryGetValue(verb.VerbType, handler) Then
            Return handler.Invoke(verb, location)
        End If
        Return True
    End Function

    Private ReadOnly performTable As New Dictionary(Of String, PerformHandler) From
        {
            {VerbTypes.SEARCH, AddressOf PerformSearch}
        }

    Private Sub PerformSearch(verb As IVerb, location As ILocation)
        Dim character = location.World.Avatar
        Dim rat = location.CreateCharacter(CharacterTypes.RAT, AddressOf InitializeRat)
        character.World.AddMessage($"{character.GetName()} finds {rat.GetName()}!")
    End Sub

    Private Sub InitializeRat(character As ICharacter)
        character.SetName("Rat")
        character.SetFlavor("This is a rat. It has big hair and likes heavy metal.")
        character.InitializeCounter(Counters.HEALTH, 20, 0, 20)
        character.SetMetadata(Metadatas.DEFEND_ROLL, "1d20")
        character.SetMetadata(Metadatas.ATTACK_ROLL, "1d20")
        character.SetTag(Tags.ENEMY)
    End Sub

    <Extension>
    Sub Perform(verb As IVerb, location As ILocation)
        Dim handler As PerformHandler = Nothing
        verb.World.AddMessage(verb.GetFlavor())
        If performTable.TryGetValue(verb.VerbType, handler) Then
            handler.Invoke(verb, location)
            Return
        End If
    End Sub

End Module
