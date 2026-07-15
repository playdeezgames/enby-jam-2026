Imports Metaphor.Provision

Friend Class Character
    Inherits InventoriedEntity(Of CharacterData)
    Implements ICharacter

    Private Sub New(world As IWorld, data As WorldData, characterId As Guid)
        MyBase.New(world, data)
        Me.CharacterId = characterId
    End Sub

    Public ReadOnly Property CharacterId As Guid Implements ICharacter.CharacterId

    Public Property Location As ILocation Implements ICharacter.Location
        Get
            Return Persistence.Location.Create(World, _data, Data.LocationId)
        End Get
        Set(value As ILocation)
            If value.LocationId <> Location.LocationId Then
                _data.Locations(Location.LocationId).CharacterIds.Remove(CharacterId)
                Data.LocationId = value.LocationId
                _data.Locations(Location.LocationId).CharacterIds.Add(CharacterId)
            End If
        End Set
    End Property

    Public ReadOnly Property CharacterType As String Implements ICharacter.CharacterType
        Get
            Return Data.CharacterType
        End Get
    End Property

    Protected Overrides ReadOnly Property Data As CharacterData
        Get
            Return _data.Characters(CharacterId)
        End Get
    End Property

    Public Overrides Sub Remove()
        Inventory.Remove()
        _data.Locations(Data.LocationId).CharacterIds.Remove(CharacterId)
        _data.Characters.Remove(CharacterId)
    End Sub

    Friend Shared Function Create(world As IWorld, data As WorldData, characterId As Guid?) As ICharacter
        Return If(characterId.HasValue, New Character(world, data, characterId.Value), Nothing)
    End Function
End Class
