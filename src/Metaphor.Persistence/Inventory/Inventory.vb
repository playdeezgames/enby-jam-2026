Imports Metaphor.Provision

Friend Class Inventory
    Implements IInventory

    Private ReadOnly world As IWorld
    Private ReadOnly _data As WorldData

    Public Sub New(world As IWorld, data As WorldData, inventoryId As Guid)
        Me.world = world
        Me._data = data
        Me.InventoryId = inventoryId
    End Sub

    Private ReadOnly Property Data As InventoryData
        Get
            Return _data.Inventories(InventoryId)
        End Get
    End Property

    Public ReadOnly Property InventoryId As Guid Implements IInventory.InventoryId

    Public ReadOnly Property HasItems As Boolean Implements IInventory.HasItems
        Get
            Return Data.ItemIds.Count <> 0
        End Get
    End Property

    Public ReadOnly Property Items As IEnumerable(Of IItem) Implements IInventory.Items
        Get
            Return Data.ItemIds.Select(Function(x) Item.Create(world, _data, x))
        End Get
    End Property

    Friend Shared Function Create(world As IWorld, data As WorldData, inventoryId As Guid?) As IInventory
        Return If(inventoryId.HasValue, New Inventory(world, data, inventoryId.Value), Nothing)
    End Function

    Public Function CreateItem(itemType As String, name As String, flavor As String, Optional initializer As ItemInitializer = Nothing) As IItem Implements IInventory.CreateItem
        Dim itemId = Guid.NewGuid
        _data.Items(itemId) = New ItemData With
            {
                .EntityType = itemType,
                .InventoryId = InventoryId
            }
        Data.ItemIds.Add(itemId)
        Dim result As IItem = Item.Create(world, _data, itemId)
        initializer?.Invoke(result)
        Return result
    End Function

    Public Sub Remove() Implements IInventory.Remove
        For Each item In Items
            item.Remove()
        Next
        _data.Inventories.Remove(InventoryId)
    End Sub
End Class
