Imports System.Text.Json
Imports Metaphor.Provision
Imports TGGD.Persistence
Imports TGGD.Provision

Public Class World
    Inherits Entity(Of WorldData)
    Implements IWorld

    Private Sub New(data As WorldData, persister As IPersister)
        Me.Data = data
        Me.persister = persister
    End Sub

    Public Overrides Sub Clear()
        MyBase.Clear()
        ClearMessages()
        Data.AvatarId = Nothing
        Data.Characters.Clear()
        Data.Locations.Clear()
        Data.Inventories.Clear()
        Data.Items.Clear()
        Data.Features.Clear()
        Data.Verbs.Clear()
    End Sub

    Protected Overrides ReadOnly Property Data As WorldData

    Public ReadOnly Property Messages As IEnumerable(Of IMessage) Implements IWorld.Messages
        Get
            Return Enumerable.
                Range(0, Data.Messages.Count).
                Select(Function(x) TGGD.Persistence.Message.Create(Function() Data.Messages(x)))
        End Get
    End Property

    Public Property Avatar As ICharacter Implements IWorld.Avatar
        Get
            Return Character.Create(Me, Data, Data.AvatarId)
        End Get
        Set(value As ICharacter)
            Data.AvatarId = value?.EntityId
        End Set
    End Property

    Private ReadOnly persister As IPersister

    Public Async Function Save(filename As String) As Task Implements IWorld.Save
        Await persister.SaveAsync(filename, JsonSerializer.Serialize(Data))
    End Function

    Public Shared Async Function Load(filename As String, persister As IPersister) As Task(Of IWorld)
        Return New World(JsonSerializer.Deserialize(Of WorldData)(Await persister.LoadAsync(filename)), persister)
    End Function

    Public Shared Function Create(data As WorldData, persister As IPersister) As IWorld
        Return New World(data, persister)
    End Function

    Public Sub ClearMessages() Implements IWorld.ClearMessages
        Data.Messages.Clear()
    End Sub

    Public Sub AddMessage(
                         text As String,
                         Optional hints As IDictionary(Of String, String) = Nothing) Implements IWorld.AddMessage
        Dim messageData As New MessageData With
            {
                .Text = text
            }
        If hints IsNot Nothing Then
            messageData.Hints = hints.ToDictionary(Function(x) x.Key, Function(x) x.Value)
        End If
        Data.Messages.Add(messageData)
    End Sub

    Public Function CreateLocation(name As String, flavor As String, Optional initializer As LocationInitializer = Nothing) As ILocation Implements IWorld.CreateLocation
        Dim locationId = Guid.NewGuid
        Data.Locations(locationId) = New LocationData With
            {
                .Name = name,
                .Flavor = flavor
            }
        Dim result = Location.Create(Me, Data, locationId)
        initializer?.Invoke(result)
        Return result
    End Function
End Class
