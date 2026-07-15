Imports TGGD.Persistence

Public Interface IMetaphorEntity
    Inherits IEntity
    ReadOnly Property World As IWorld
    Sub Remove()
    ReadOnly Property Name As String
    ReadOnly Property Flavor As String
End Interface
