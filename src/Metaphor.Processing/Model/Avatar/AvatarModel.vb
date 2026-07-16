Imports Metaphor.Persistence

Friend Class AvatarModel
    Implements IAvatarModel

    Private ReadOnly avatar As ICharacter

    Private Sub New(avatar As ICharacter)
        Me.avatar = avatar
    End Sub

    Public ReadOnly Property Inventory As IInventoryModel Implements IAvatarModel.Inventory
        Get
            Return InventoryModel.Create(avatar.World)
        End Get
    End Property

    Public ReadOnly Property Verbs As IEnumerable(Of IVerbModel) Implements IAvatarModel.Verbs
        Get
            Return avatar.Verbs.Select(Function(x) CharacterVerbModel.Create(avatar, x))
        End Get
    End Property

    Public ReadOnly Property IsChangingPace As Boolean Implements IAvatarModel.IsChangingPace
        Get
            Return avatar.HasTag(Tags.IS_CHANGING_PACE)
        End Get
    End Property

    Public Sub ShowStatus() Implements IAvatarModel.ShowStatus
        avatar.World.ClearMessages()
        avatar.ShowStatus()
    End Sub

    Public Sub CancelChangePace() Implements IAvatarModel.CancelChangePace
        avatar.ClearTag(Tags.IS_CHANGING_PACE)
    End Sub

    Public Sub SetPace(pace As Integer) Implements IAvatarModel.SetPace
        Dim world = avatar.World
        world.ClearMessages()
        avatar.SetPace(pace)
        world.AddMessage($"{avatar.Name} sets pace to {avatar.GetPace()}.")
    End Sub

    Friend Shared Function Create(avatar As ICharacter) As IAvatarModel
        Return New AvatarModel(avatar)
    End Function
End Class
