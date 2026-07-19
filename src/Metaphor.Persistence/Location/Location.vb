Imports Metaphor.Provision

Friend Class Location
    Inherits InventoriedEntity(Of LocationData)
    Implements ILocation

    Private Sub New(world As IWorld, data As WorldData, locationId As Guid)
        MyBase.New(world, data, locationId)
    End Sub

    Public ReadOnly Property Features As IEnumerable(Of IFeature) Implements ILocation.Features
        Get
            Return Data.FeatureIds.Select(Function(x) Feature.Create(World, _data, x))
        End Get
    End Property

    Public ReadOnly Property HasFeatures As Boolean Implements ILocation.HasFeatures
        Get
            Return Data.FeatureIds.Count <> 0
        End Get
    End Property

    Public ReadOnly Property Characters As IEnumerable(Of ICharacter) Implements ILocation.Characters
        Get
            Return Data.CharacterIds.Select(Function(x) Character.Create(World, _data, x))
        End Get
    End Property

    Public Overrides ReadOnly Property Exists As Boolean
        Get
            Return _data.Locations.ContainsKey(EntityId)
        End Get
    End Property

    Protected Overrides ReadOnly Property Data As LocationData
        Get
            Return _data.Locations(EntityId)
        End Get
    End Property

    Public Overrides Sub Remove()
        Throw New NotImplementedException()
    End Sub

    Friend Shared Function Create(world As IWorld, data As WorldData, locationId As Guid?) As ILocation
        Return If(locationId.HasValue, New Location(world, data, locationId.Value), Nothing)
    End Function

    Public Function CreateCharacter(characterType As String, name As String, flavor As String, Optional initialize As CharacterInitializer = Nothing) As ICharacter Implements ILocation.CreateCharacter
        Dim characterId = Guid.NewGuid
        _data.Characters(characterId) = New CharacterData With
            {
                .EntityType = characterType,
                .LocationId = EntityId,
                .Name = name,
                .Flavor = flavor
            }
        Data.CharacterIds.Add(characterId)
        Dim result = Character.Create(World, _data, characterId)
        initialize?.Invoke(result)
        Return result
    End Function

    Public Function CreateFeature(featureType As String, name As String, flavor As String, Optional initializer As FeatureInitializer = Nothing) As IFeature Implements ILocation.CreateFeature
        Dim featureId = Guid.NewGuid
        _data.Features(featureId) = New FeatureData With
            {
                .LocationId = EntityId,
                .Name = name,
                .Flavor = flavor,
                .EntityType = featureType
            }
        Data.FeatureIds.Add(featureId)
        Dim result As IFeature = Feature.Create(World, _data, featureId)
        initializer?.Invoke(result)
        Return result
    End Function

    Public Function GetOtherCharacters(character As ICharacter) As IEnumerable(Of ICharacter) Implements ILocation.GetOtherCharacters
        Return Data.CharacterIds.
            Where(Function(id) id <> character.EntityId).
            Select(Function(x) Persistence.Character.Create(World, _data, x))
    End Function

    Public Function HasOtherCharacters(character As ICharacter) As Boolean Implements ILocation.HasOtherCharacters
        Return Data.CharacterIds.Any(Function(x) x <> character.EntityId)
    End Function
End Class
