Imports Metaphor.Provision

Friend Class Verb
    Inherits MetaphorEntity(Of VerbData)
    Implements IVerb

    Private Sub New(world As IWorld, data As WorldData, verbId As Guid)
        MyBase.New(world, data, verbId)
    End Sub

    Public Overrides ReadOnly Property Exists As Boolean
        Get
            Return _data.Verbs.ContainsKey(EntityId)
        End Get
    End Property

    Protected Overrides ReadOnly Property Data As VerbData
        Get
            Return _data.Verbs(EntityId)
        End Get
    End Property

    Public Overrides Sub Remove()
        _data.Verbs.Remove(EntityId)
    End Sub

    Friend Shared Function Create(world As IWorld, data As WorldData, verbId As Guid) As IVerb
        Return New Verb(world, data, verbId)
    End Function
End Class
