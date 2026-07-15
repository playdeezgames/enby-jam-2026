Imports Metaphor.Persistence

Friend Class ExitsModel
    Implements IExitsModel

    Private ReadOnly world As IWorld

    Private Sub New(world As IWorld)
        Me.world = world
    End Sub

    Public ReadOnly Property HasAny As Boolean Implements IExitsModel.HasAny
        Get
            Return world.Avatar.Location.HasRoutes
        End Get
    End Property

    Public ReadOnly Property All As IEnumerable(Of IExitModel) Implements IExitsModel.All
        Get
            Return world.Avatar.Location.Routes.Select(AddressOf ExitModel.Create)
        End Get
    End Property

    Friend Shared Function Create(entity As IWorld) As IExitsModel
        Return New ExitsModel(entity)
    End Function
End Class
