Imports Metaphor.Provision

Friend MustInherit Class InventoriedEntity(Of TData As InventoriedEntityData)
    Inherits VerbableEntity(Of TData)
    Implements IInventoriedEntity

    Protected Sub New(world As IWorld, data As WorldData, entityId As Guid)
        MyBase.New(world, data, entityId)
    End Sub

    Public ReadOnly Property Inventory As IInventory Implements IInventoriedEntity.Inventory
        Get
            Dim inventoryId = Data.InventoryId
            If Not inventoryId.HasValue Then
                inventoryId = Guid.NewGuid
                _data.Inventories(inventoryId.Value) = New InventoryData
                Data.InventoryId = inventoryId.Value
            End If
            Return Persistence.Inventory.Create(World, _data, inventoryId)
        End Get
    End Property
End Class
