Public Interface IVerbableEntity
    Inherits IKJEntity
    Function CreateVerb(verbType As String, Optional initializer As VerbInitializer = Nothing) As IVerb
    ReadOnly Property Verbs As IEnumerable(Of IVerb)
End Interface
