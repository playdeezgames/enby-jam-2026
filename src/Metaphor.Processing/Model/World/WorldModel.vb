Imports Metaphor.Persistence
Imports TGGD.Persistence
Imports TGGD.Processing

Public Class WorldModel
    Inherits BaseModel(Of IWorld)
    Implements IWorldModel

    Private Sub New(entity As IWorld, isQuittable As Boolean)
        MyBase.New(entity)
        Me.IsQuittable = isQuittable
    End Sub

    Public ReadOnly Property IsQuittable As Boolean Implements IWorldModel.IsQuittable

    Public ReadOnly Property Messages As IEnumerable(Of IMessage) Implements IWorldModel.Messages
        Get
            Return Entity.Messages
        End Get
    End Property

    Public ReadOnly Property Ground As IGroundModel Implements IWorldModel.Ground
        Get
            Return GroundModel.Create(Entity)
        End Get
    End Property

    Public ReadOnly Property Inventory As IInventoryModel Implements IWorldModel.Inventory
        Get
            Return InventoryModel.Create(Entity)
        End Get
    End Property

    Public ReadOnly Property Features As IFeaturesModel Implements IWorldModel.Features
        Get
            Return FeaturesModel.Create(Entity)
        End Get
    End Property

    Public ReadOnly Property Characters As ICharactersModel Implements IWorldModel.Characters
        Get
            Return CharactersModel.Create(Entity)
        End Get
    End Property

    Public ReadOnly Property Location As ILocationModel Implements IWorldModel.Location
        Get
            Return LocationModel.Create(Entity.Avatar.Location)
        End Get
    End Property

    Public Sub Embark(chosenName As String) Implements IWorldModel.Embark
        Abandon()
        Entity.Initialize(InitializationContext.Create(chosenName))
    End Sub

    Public Sub Abandon() Implements IWorldModel.Abandon
        Entity.Clear()
    End Sub

    Public Sub Look() Implements IWorldModel.Look
        Entity.ClearMessages()
        Entity.Avatar.Look()
    End Sub

    Public Sub ShowStatus() Implements IWorldModel.ShowStatus
        Entity.ClearMessages()
        Entity.Avatar.ShowStatus()
    End Sub

    Public Shared Async Function Create(quittable As Boolean, persister As IPersister) As Task(Of IWorldModel)
        Dim entity As IWorld
        Try
            entity = Await Metaphor.Persistence.World.Load(SAVE_FILENAME, persister)
        Catch ex As Exception
            entity = Metaphor.Persistence.World.Create(New Provision.WorldData, persister)
        End Try
        Return New WorldModel(entity, quittable)
    End Function
End Class
