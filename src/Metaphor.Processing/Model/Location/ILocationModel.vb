Public Interface ILocationModel
    ReadOnly Property Verbs As IEnumerable(Of IVerbModel)
    ReadOnly Property Others As IEnumerable(Of ICharacterModel)
    Sub Look()
    ReadOnly Property Features As IFeaturesModel
    ReadOnly Property Characters As ICharactersModel
    ReadOnly Property Ground As IGroundModel
End Interface
