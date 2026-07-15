Imports Metaphor.Provision

Friend Class Feature
    Inherits InventoriedEntity(Of FeatureData)
    Implements IFeature

    Private Sub New(world As IWorld, data As WorldData, featureId As Guid)
        MyBase.New(world, data)
        Me.FeatureId = featureId
    End Sub

    Public ReadOnly Property FeatureId As Guid Implements IFeature.FeatureId

    Public ReadOnly Property Location As ILocation Implements IFeature.Location
        Get
            Return Persistence.Location.Create(World, _data, Data.LocationId)
        End Get
    End Property


    Protected Overrides ReadOnly Property Data As FeatureData
        Get
            Return _data.Features(FeatureId)
        End Get
    End Property

    Public Overrides Sub Remove()
        Throw New NotImplementedException()
    End Sub

    Friend Shared Function Create(world As IWorld, data As WorldData, featureId As Guid) As IFeature
        Return New Feature(world, data, featureId)
    End Function
End Class
