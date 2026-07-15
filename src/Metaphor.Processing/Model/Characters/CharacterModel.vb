Imports Metaphor.Persistence

Friend Class CharacterModel
    Implements ICharacterModel

    Private ReadOnly character As ICharacter

    Private Sub New(character As ICharacter)
        Me.character = character
    End Sub

    Public ReadOnly Property Name As String Implements ICharacterModel.Name
        Get
            Return character.GetName
        End Get
    End Property

    Public ReadOnly Property Verbs As IEnumerable(Of IVerbModel) Implements ICharacterModel.Verbs
        Get
            Return character.Verbs.Select(Function(x) CharacterVerbModel.Create(character, x))
        End Get
    End Property

    Public Sub Examine() Implements ICharacterModel.Examine
        Dim world = character.World
        world.ClearMessages()
        world.AddMessage(character.GetFlavor())
    End Sub

    Public Sub Attack() Implements ICharacterModel.Attack
        Dim world = character.World
        world.ClearMessages()
        Dim avatar = world.Avatar
        avatar.Attack(character)
        If Not character.IsDead() Then
            character.Attack(avatar)
        Else
            character.Die()
        End If
    End Sub

    Friend Shared Function Create(character As ICharacter) As ICharacterModel
        Return New CharacterModel(character)
    End Function
End Class
