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

    Public ReadOnly Property Location As ILocationModel Implements IWorldModel.Location
        Get
            Return LocationModel.Create(Entity.Avatar.Location)
        End Get
    End Property

    Public ReadOnly Property Avatar As IAvatarModel Implements IWorldModel.Avatar
        Get
            Return AvatarModel.Create(Entity.Avatar)
        End Get
    End Property

    Public ReadOnly Property InAd As Boolean Implements IWorldModel.InAd
        Get
            Return Entity.AdFinish.HasValue
        End Get
    End Property

    Public Sub Embark(chosenName As String, chosenPronouns As String) Implements IWorldModel.Embark
        Abandon()
        Entity.Initialize(InitializationContext.Create(chosenName, chosenPronouns))
    End Sub

    Public Sub Abandon() Implements IWorldModel.Abandon
        Entity.Clear()
    End Sub

    Public Sub ShowAd() Implements IWorldModel.ShowAd
        Entity.ClearMessages()
        If Entity.AdFinish.Value > DateTimeOffset.Now Then
            Dim timeRemaining = Entity.AdFinish.Value - DateTimeOffset.Now
            Entity.AddMessage($"Time left in ad break: {timeRemaining.ToString("mm\:ss")}")
            Entity.AddMessage("(This is a turn based game. As such, this counter will not automatically change. You have to click the OK button to refresh.)")
            Entity.AddMessage(
            "For all yer umlauting needs! umlaut.fyi",
            New Dictionary(Of String, String) From
            {
                {"ELEMENT_TYPE", "LINK"},
                {"URL", "https://umlaut.fyi/"}
            })
        Else
            Entity.AddMessage("Ad break is complete! You may return to yer metaphor!")
            Dim avatar = Entity.Avatar
            Dim coupon = avatar.Inventory.CreateItem(ItemTypes.COUPON, "Coupon", "This is a coupon.")
            Entity.AddMessage($"{avatar.Name} receives {coupon.Name}.")
            Entity.AdFinish = Nothing
        End If
    End Sub

    Public Sub StartAd() Implements IWorldModel.StartAd
        Entity.AdFinish = DateTimeOffset.Now.AddMinutes(2.0)
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
