Imports Metaphor.Persistence

Friend Class FeaturesModel
    Implements IFeaturesModel

    Private ReadOnly world As IWorld

    Private Sub New(world As IWorld)
        Me.world = world
    End Sub

    Public ReadOnly Property HasAny As Boolean Implements IFeaturesModel.HasAny
        Get
            Return world.Avatar.Location.HasFeatures
        End Get
    End Property

    Public ReadOnly Property All As IEnumerable(Of IFeatureModel) Implements IFeaturesModel.All
        Get
            Return world.Avatar.Location.Features.Select(AddressOf FeatureModel.Create)
        End Get
    End Property

    Public Sub ShowList() Implements IFeaturesModel.ShowList
        world.ClearMessages()
        world.Avatar.ShowFeatures()
    End Sub

    Friend Shared Function Create(entity As IWorld) As IFeaturesModel
        Return New FeaturesModel(entity)
    End Function
End Class
