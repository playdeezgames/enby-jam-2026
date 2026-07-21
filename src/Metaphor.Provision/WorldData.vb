Imports TGGD.Provision

Public Class WorldData
    Inherits EntityData
    Public Property Messages As New List(Of MessageData)
    Public Property Locations As New Dictionary(Of Guid, LocationData)
    Public Property AvatarId As Guid?
    Public Property Characters As New Dictionary(Of Guid, CharacterData)
    Public Property Inventories As New Dictionary(Of Guid, InventoryData)
    Public Property Items As New Dictionary(Of Guid, ItemData)
    Public Property Features As New Dictionary(Of Guid, FeatureData)
    Public Property Verbs As New Dictionary(Of Guid, VerbData)
    Public Property AdFinishes As DateTimeOffset?
End Class
