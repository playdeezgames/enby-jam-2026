Imports TGGD.Persistence

Public Interface IKJEntity
    Inherits IEntity
    ReadOnly Property World As IWorld
    Sub Remove()
End Interface
