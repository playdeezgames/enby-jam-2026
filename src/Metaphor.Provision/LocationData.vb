Public Class LocationData
    Inherits InventoriedEntityData
    Public Property CharacterIds As New HashSet(Of Guid)
    Public Property FeatureIds As New HashSet(Of Guid)
End Class
