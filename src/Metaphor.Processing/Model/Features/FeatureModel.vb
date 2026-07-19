Imports Metaphor.Persistence

Friend Class FeatureModel
    Implements IFeatureModel

    Private ReadOnly feature As IFeature

    Private Sub New(feature As IFeature)
        Me.feature = feature
    End Sub

    Public ReadOnly Property Name As String Implements IFeatureModel.Name
        Get
            Return feature.Name
        End Get
    End Property

    Public ReadOnly Property Verbs As IEnumerable(Of IVerbModel) Implements IFeatureModel.Verbs
        Get
            Return feature.Verbs.Select(Function(x) FeatureVerbModel.Create(feature, x))
        End Get
    End Property

    Public ReadOnly Property Exists As Boolean Implements IFeatureModel.Exists
        Get
            Return feature.Exists
        End Get
    End Property

    Public Sub Examine() Implements IFeatureModel.Examine
        Dim world = feature.World
        world.ClearMessages()
        Dim character = world.Avatar
        character.World.AddMessage($"{character.Name} interacts with {feature.Name}.")
        character.World.AddMessage(feature.Flavor)
    End Sub

    Friend Shared Function Create(feature As IFeature) As IFeatureModel
        Return New FeatureModel(feature)
    End Function
End Class
