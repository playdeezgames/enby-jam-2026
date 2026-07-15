Imports Metaphor.Provision
Imports TGGD.Persistence

Friend MustInherit Class MetaphorEntity(Of TData As MetaphorEntityData)
    Inherits Entity(Of TData)
    Implements IMetaphorEntity

    Protected Sub New(world As IWorld, data As WorldData)
        Me.World = world
        Me._data = data
    End Sub

    Public MustOverride Sub Remove() Implements IMetaphorEntity.Remove
    Public ReadOnly Property World As IWorld Implements IMetaphorEntity.World

    Public ReadOnly Property Name As String Implements IMetaphorEntity.Name
        Get
            Return Data.Name
        End Get
    End Property

    Public ReadOnly Property Flavor As String Implements IMetaphorEntity.Flavor
        Get
            Return Data.Flavor
        End Get
    End Property

    Protected ReadOnly _data As WorldData
End Class
