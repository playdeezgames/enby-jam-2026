Imports Metaphor.Provision

Friend Class Feature
    Inherits InventoriedEntity(Of FeatureData)
    Implements IFeature

    Private Sub New(world As IWorld, data As WorldData, featureId As Guid)
        MyBase.New(world, data, featureId)
    End Sub

    Public ReadOnly Property Location As ILocation Implements IFeature.Location
        Get
            Return Persistence.Location.Create(World, _data, Data.LocationId)
        End Get
    End Property

    Public ReadOnly Property Exists As Boolean Implements IFeature.Exists
        Get
            Return _data.Features.ContainsKey(EntityId)
        End Get
    End Property

    Protected Overrides ReadOnly Property Data As FeatureData
        Get
            Return _data.Features(EntityId)
        End Get
    End Property

    Public Overrides Sub Remove()
        _data.Locations(Location.EntityId).FeatureIds.Remove(EntityId)
        For Each verb In Verbs
            verb.Remove()
        Next
        Inventory.Remove()
        _data.Features.Remove(EntityId)
    End Sub

    Friend Shared Function Create(world As IWorld, data As WorldData, featureId As Guid) As IFeature
        Return New Feature(world, data, featureId)
    End Function
End Class
