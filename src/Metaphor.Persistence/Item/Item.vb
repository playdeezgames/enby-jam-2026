Imports Metaphor.Provision

Friend Class Item
    Inherits VerbableEntity(Of ItemData)
    Implements IItem

    Private Sub New(world As IWorld, data As WorldData, itemId As Guid)
        MyBase.New(world, data, itemId)
    End Sub

    Public Property Inventory As IInventory Implements IItem.Inventory
        Get
            Return Persistence.Inventory.Create(World, _data, Data.InventoryId)
        End Get
        Set(value As IInventory)
            _data.Inventories(Data.InventoryId).ItemIds.Remove(EntityId)
            Data.InventoryId = value.InventoryId
            _data.Inventories(Data.InventoryId).ItemIds.Add(EntityId)
        End Set
    End Property

    Public Overrides ReadOnly Property Exists As Boolean
        Get
            Return _data.Items.ContainsKey(EntityId)
        End Get
    End Property

    Protected Overrides ReadOnly Property Data As ItemData
        Get
            Return _data.Items(EntityId)
        End Get
    End Property

    Public Overrides Sub Remove()
        _data.Inventories(Data.InventoryId).ItemIds.Remove(EntityId)
        _data.Items.Remove(EntityId)
    End Sub

    Friend Shared Function Create(world As IWorld, data As WorldData, itemId As Guid?) As IItem
        Return If(
            itemId.HasValue,
            New Item(world, data, itemId.Value),
            Nothing)
    End Function
End Class
