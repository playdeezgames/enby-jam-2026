Imports Metaphor.Provision

Friend Class Character
    Inherits InventoriedEntity(Of CharacterData)
    Implements ICharacter

    Private Sub New(world As IWorld, data As WorldData, characterId As Guid)
        MyBase.New(world, data, characterId)
    End Sub

    Public Property Location As ILocation Implements ICharacter.Location
        Get
            Return Persistence.Location.Create(World, _data, Data.LocationId)
        End Get
        Set(value As ILocation)
            If value.EntityId <> Location.EntityId Then
                _data.Locations(Location.EntityId).CharacterIds.Remove(EntityId)
                Data.LocationId = value.EntityId
                _data.Locations(Location.EntityId).CharacterIds.Add(EntityId)
            End If
        End Set
    End Property

    Protected Overrides ReadOnly Property Data As CharacterData
        Get
            Return _data.Characters(EntityId)
        End Get
    End Property

    Public Overrides Sub Remove()
        Inventory.Remove()
        _data.Locations(Data.LocationId).CharacterIds.Remove(EntityId)
        _data.Characters.Remove(EntityId)
    End Sub

    Friend Shared Function Create(world As IWorld, data As WorldData, characterId As Guid?) As ICharacter
        Return If(characterId.HasValue, New Character(world, data, characterId.Value), Nothing)
    End Function
End Class
