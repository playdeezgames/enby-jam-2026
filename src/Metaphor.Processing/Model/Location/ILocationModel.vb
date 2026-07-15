Public Interface ILocationModel
    ReadOnly Property Verbs As IEnumerable(Of IVerbModel)
    ReadOnly Property Others As IEnumerable(Of ICharacterModel)
    Sub Look() 'TODO: goes into location model
    ReadOnly Property Features As IFeaturesModel 'TODO: goes into location model
    ReadOnly Property Characters As ICharactersModel 'TODO: goes into location model
    ReadOnly Property Ground As IGroundModel ' TODO: goes into location model
End Interface
