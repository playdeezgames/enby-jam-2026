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
    Protected ReadOnly _data As WorldData
End Class
