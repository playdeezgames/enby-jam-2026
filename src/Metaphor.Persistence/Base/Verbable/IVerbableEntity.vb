Public Interface IVerbableEntity
    Inherits IMetaphorEntity
    Function CreateVerb(verbType As String, Optional initializer As VerbInitializer = Nothing) As IVerb
    ReadOnly Property Verbs As IEnumerable(Of IVerb)
End Interface
