Imports TGGD.Persistence
Imports TGGD.Processing

Public Interface IWorldModel
    Inherits IModel
    ReadOnly Property IsQuittable As Boolean
    Sub Embark(chosenName As String)
    Sub Abandon()
    Sub Look() 'TODO: goes into location model
    Sub AttemptRun()
    Sub ShowStatus()
    ReadOnly Property Location As ILocationModel
    ReadOnly Property Messages As IEnumerable(Of IMessage)
    ReadOnly Property Exits As IExitsModel ' TODO: goes into location model
    ReadOnly Property Ground As IGroundModel ' TODO: goes into location model
    ReadOnly Property Inventory As IInventoryModel
    ReadOnly Property Features As IFeaturesModel 'TODO: goes into location model
    ReadOnly Property Characters As ICharactersModel 'TODO: goes into location model
    ReadOnly Property IsInCombat As Boolean
End Interface
