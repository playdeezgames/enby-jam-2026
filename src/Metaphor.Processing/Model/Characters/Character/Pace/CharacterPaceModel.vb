Imports Metaphor.Persistence

Friend Class CharacterPaceModel
    Implements ICharacterPaceModel

    Private ReadOnly character As ICharacter

    Private Sub New(character As ICharacter)
        Me.character = character
    End Sub

    Public ReadOnly Property IsChanging As Boolean Implements ICharacterPaceModel.IsChanging
        Get
            Return character.HasTag(Tags.IS_CHANGING_PACE)
        End Get
    End Property

    Public Sub CancelChange() Implements ICharacterPaceModel.CancelChange
        character.ClearTag(Tags.IS_CHANGING_PACE)
    End Sub

    Public Sub ChangeTo(pace As Integer) Implements ICharacterPaceModel.ChangeTo
        Dim world = character.World
        world.ClearMessages()
        character.SetPace(pace)
        world.AddMessage($"{character.Name}'s pace is now set to {character.GetPace()}.")
        CancelChange()
    End Sub

    Friend Shared Function Create(avatar As ICharacter) As ICharacterPaceModel
        Return New CharacterPaceModel(avatar)
    End Function
End Class
