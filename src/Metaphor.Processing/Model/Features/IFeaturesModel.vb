Public Interface IFeaturesModel
    ReadOnly Property HasAny As Boolean
    ReadOnly Property All As IEnumerable(Of IFeatureModel)
    Sub ShowList()
End Interface
