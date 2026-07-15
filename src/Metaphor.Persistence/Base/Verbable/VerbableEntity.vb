Imports Metaphor.Provision

Friend MustInherit Class VerbableEntity(Of TData As VerbableEntityData)
    Inherits MetaphorEntity(Of TData)
    Implements IVerbableEntity

    Protected Sub New(world As IWorld, data As WorldData)
        MyBase.New(world, data)
    End Sub

    Public ReadOnly Property Verbs As IEnumerable(Of IVerb) Implements IVerbableEntity.Verbs
        Get
            Return Data.VerbIds.Select(Function(x) Verb.Create(World, _data, x))
        End Get
    End Property


    Public Function CreateVerb(verbType As String, Optional initializer As VerbInitializer = Nothing) As IVerb Implements IVerbableEntity.CreateVerb
        Dim verbId = Guid.NewGuid
        _data.Verbs(verbId) = New VerbData With
            {
                .VerbType = verbType
            }
        Data.VerbIds.Add(verbId)
        Dim result As IVerb = Verb.Create(World, _data, verbId)
        initializer?.Invoke(result)
        Return result
    End Function
End Class
