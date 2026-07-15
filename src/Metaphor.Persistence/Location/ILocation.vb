Public Delegate Sub LocationInitializer(location As ILocation)
Public Interface ILocation
    Inherits IInventoriedEntity
    ReadOnly Property LocationId As Guid
    ReadOnly Property HasRoutes As Boolean
    Function CreateCharacter(characterType As String, name As String, flavor As String, Optional initialize As CharacterInitializer = Nothing) As ICharacter
    Function CreateFeature(name As String, flavor As String, Optional initializer As FeatureInitializer = Nothing) As IFeature
    ReadOnly Property Features As IEnumerable(Of IFeature)
    ReadOnly Property HasFeatures As Boolean
    Function GetOtherCharacters(character As ICharacter) As IEnumerable(Of ICharacter)
    Function HasOtherCharacters(character As ICharacter) As Boolean
    ReadOnly Property Characters As IEnumerable(Of ICharacter)
End Interface
