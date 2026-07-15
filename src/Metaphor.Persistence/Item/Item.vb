Imports Metaphor.Provision

Friend Class Item
    Inherits VerbableEntity(Of ItemData)
    Implements IItem

    Private Sub New(world As IWorld, data As WorldData, itemId As Guid)
        MyBase.New(world, data)
        Me.ItemId = itemId
    End Sub

    Public ReadOnly Property ItemId As Guid Implements IItem.ItemId

    Public Property Inventory As IInventory Implements IItem.Inventory
        Get
            Return Persistence.Inventory.Create(World, _data, Data.InventoryId)
        End Get
        Set(value As IInventory)
            _data.Inventories(Data.InventoryId).ItemIds.Remove(ItemId)
            Data.InventoryId = value.InventoryId
            _data.Inventories(Data.InventoryId).ItemIds.Add(ItemId)
        End Set
    End Property

    Public ReadOnly Property ItemType As String Implements IItem.ItemType
        Get
            Return Data.ItemType
        End Get
    End Property

    Protected Overrides ReadOnly Property Data As ItemData
        Get
            Return _data.Items(ItemId)
        End Get
    End Property

    Public Overrides Sub Remove()
        _data.Inventories(Data.InventoryId).ItemIds.Remove(ItemId)
        _data.Items.Remove(ItemId)
    End Sub

    Friend Shared Function Create(world As IWorld, data As WorldData, itemId As Guid?) As IItem
        Return If(
            itemId.HasValue,
            New Item(world, data, itemId.Value),
            Nothing)
    End Function
End Class
