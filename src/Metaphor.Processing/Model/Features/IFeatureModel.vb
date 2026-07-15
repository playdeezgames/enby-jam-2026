Public Interface IFeatureModel
    ReadOnly Property Name As String
    Sub Examine()
    ReadOnly Property Verbs As IEnumerable(Of IVerbModel)
End Interface
