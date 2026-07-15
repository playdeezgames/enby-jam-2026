Public MustInherit Class EntityData
    Public Property Metadatas As New Dictionary(Of String, String)(StringComparer.InvariantCultureIgnoreCase)
    Public Property Counters As New Dictionary(Of String, Integer)(StringComparer.InvariantCultureIgnoreCase)
    Public Property CounterMinimums As New Dictionary(Of String, Integer)(StringComparer.InvariantCultureIgnoreCase)
    Public Property CounterMaximums As New Dictionary(Of String, Integer)(StringComparer.InvariantCultureIgnoreCase)
    Public Property Dimensions As New Dictionary(Of String, Double)(StringComparer.InvariantCultureIgnoreCase)
    Public Property DimensionMinimums As New Dictionary(Of String, Double)(StringComparer.InvariantCultureIgnoreCase)
    Public Property DimensionMaximums As New Dictionary(Of String, Double)(StringComparer.InvariantCultureIgnoreCase)
    Public Property Tags As New HashSet(Of String)(StringComparer.InvariantCultureIgnoreCase)
End Class
