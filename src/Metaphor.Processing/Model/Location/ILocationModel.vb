Public Interface ILocationModel
    ReadOnly Property Verbs As IEnumerable(Of IVerbModel)
    ReadOnly Property Others As IEnumerable(Of ICharacterModel)
End Interface
