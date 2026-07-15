Public Class LocationData
    Inherits InventoriedEntityData
    Public Property CharacterIds As New HashSet(Of Guid)
    Public Property RouteIds As New Dictionary(Of String, Guid)(StringComparer.InvariantCultureIgnoreCase)
    Public Property FeatureIds As New HashSet(Of Guid)
End Class
