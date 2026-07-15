Imports TGGD.Persistence

Public Interface IMetaphorEntity
    Inherits IEntity
    ReadOnly Property World As IWorld
    Sub Remove()
End Interface
