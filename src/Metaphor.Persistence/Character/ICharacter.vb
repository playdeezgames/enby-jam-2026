Public Delegate Sub CharacterInitializer(character As ICharacter)
Public Interface ICharacter
    Inherits IInventoriedEntity
    ReadOnly Property CharacterId As Guid
    Property Location As ILocation
    ReadOnly Property CharacterType As String
End Interface
