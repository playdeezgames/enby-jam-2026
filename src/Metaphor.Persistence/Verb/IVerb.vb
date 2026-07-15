Public Delegate Sub VerbInitializer(verb As IVerb)
Public Interface IVerb
    Inherits IKJEntity
    ReadOnly Property VerbId As Guid
    ReadOnly Property VerbType As String
End Interface
