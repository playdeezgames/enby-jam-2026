Public Interface ICharacterModel
    ReadOnly Property Name As String
    Sub Examine()
    ReadOnly Property Verbs As IEnumerable(Of IVerbModel)
End Interface
