Imports Metaphor.Provision
Imports TGGD.Persistence

Friend MustInherit Class KJEntity(Of TData As KJEntityData)
    Inherits Entity(Of TData)
    Implements IKJEntity

    Protected Sub New(world As IWorld, data As WorldData)
        Me.World = world
        Me._data = data
    End Sub

    Public MustOverride Sub Remove() Implements IKJEntity.Remove
    Public ReadOnly Property World As IWorld Implements IKJEntity.World
    Protected ReadOnly _data As WorldData
End Class
