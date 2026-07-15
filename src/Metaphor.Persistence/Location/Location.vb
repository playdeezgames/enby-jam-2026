Imports Metaphor.Provision

Friend Class Location
    Inherits InventoriedEntity(Of LocationData)
    Implements ILocation

    Private Sub New(world As IWorld, data As WorldData, locationId As Guid)
        MyBase.New(world, data)
        Me.LocationId = locationId
    End Sub

    Public ReadOnly Property LocationId As Guid Implements ILocation.LocationId

    Public ReadOnly Property HasRoutes As Boolean Implements ILocation.HasRoutes
        Get
            Return Data.RouteIds.Count <> 0
        End Get
    End Property

    Public ReadOnly Property Routes As IEnumerable(Of IRoute) Implements ILocation.Routes
        Get
            Return Data.RouteIds.Select(Function(x) Route.Create(World, _data, x.Key, x.Value))
        End Get
    End Property

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

    Protected Overrides ReadOnly Property Data As LocationData
        Get
            Return _data.Locations(LocationId)
        End Get
    End Property

    Public Overrides Sub Remove()
        Throw New NotImplementedException()
    End Sub

    Friend Shared Function Create(world As IWorld, data As WorldData, locationId As Guid?) As ILocation
        Return If(locationId.HasValue, New Location(world, data, locationId.Value), Nothing)
    End Function

    Public Function CreateCharacter(characterType As String, Optional initialize As CharacterInitializer = Nothing) As ICharacter Implements ILocation.CreateCharacter
        Dim characterId = Guid.NewGuid
        _data.Characters(characterId) = New CharacterData With
            {
                .CharacterType = characterType,
                .LocationId = LocationId
            }
        Data.CharacterIds.Add(characterId)
        Dim result = Character.Create(World, _data, characterId)
        initialize?.Invoke(result)
        Return result
    End Function

    Public Function CreateRoute(routeType As String, direction As String, destination As ILocation, Optional initialize As RouteInitializer = Nothing) As IRoute Implements ILocation.CreateRoute
        Dim routeId = Guid.NewGuid
        _data.Routes(routeId) = New RouteData With
            {
                .DestinationLocationId = destination.LocationId,
                .RouteType = routeType
            }
        Data.RouteIds(direction) = routeId
        Dim result As IRoute = Route.Create(World, _data, direction, routeId)
        initialize?.Invoke(result)
        Return result
    End Function

    Public Function CreateFeature(Optional initializer As FeatureInitializer = Nothing) As IFeature Implements ILocation.CreateFeature
        Dim featureId = Guid.NewGuid
        _data.Features(featureId) = New FeatureData With
            {
                .LocationId = LocationId
            }
        Data.FeatureIds.Add(featureId)
        Dim result As IFeature = Feature.Create(World, _data, featureId)
        initializer?.Invoke(result)
        Return result
    End Function

    Public Function GetOtherCharacters(character As ICharacter) As IEnumerable(Of ICharacter) Implements ILocation.GetOtherCharacters
        Return Data.CharacterIds.
            Where(Function(id) id <> character.CharacterId).
            Select(Function(x) Persistence.Character.Create(World, _data, x))
    End Function

    Public Function HasOtherCharacters(character As ICharacter) As Boolean Implements ILocation.HasOtherCharacters
        Return Data.CharacterIds.Any(Function(x) x <> character.CharacterId)
    End Function
End Class
