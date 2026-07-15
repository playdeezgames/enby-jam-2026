Imports Metaphor.Provision

Friend Class Verb
    Inherits MetaphorEntity(Of VerbData)
    Implements IVerb

    Private Sub New(world As IWorld, data As WorldData, verbId As Guid)
        MyBase.New(world, data)
        Me.VerbId = verbId
    End Sub

    Public ReadOnly Property VerbId As Guid Implements IVerb.VerbId

    Public ReadOnly Property VerbType As String Implements IVerb.VerbType
        Get
            Return Data.VerbType
        End Get
    End Property

    Protected Overrides ReadOnly Property Data As VerbData
        Get
            Return _data.Verbs(VerbId)
        End Get
    End Property

    Public Overrides Sub Remove()
        Throw New NotImplementedException()
    End Sub

    Friend Shared Function Create(world As IWorld, data As WorldData, verbId As Guid) As IVerb
        Return New Verb(world, data, verbId)
    End Function
End Class
