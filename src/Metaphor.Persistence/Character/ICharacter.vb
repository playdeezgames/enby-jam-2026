Public Delegate Sub CharacterInitializer(character As ICharacter)
Public Interface ICharacter
    Inherits IInventoriedEntity
    Property Location As ILocation
End Interface
