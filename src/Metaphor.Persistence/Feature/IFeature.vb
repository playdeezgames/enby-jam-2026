Public Delegate Sub FeatureInitializer(feature As IFeature)
Public Interface IFeature
    Inherits IInventoriedEntity
    ReadOnly Property Location As ILocation
    ReadOnly Property Exists As Boolean
End Interface
