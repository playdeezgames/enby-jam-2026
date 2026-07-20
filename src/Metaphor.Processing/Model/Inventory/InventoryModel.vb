Imports Metaphor.Persistence

Friend Class InventoryModel
    Implements IInventoryModel

    Private ReadOnly world As IWorld

    Private Sub New(world As IWorld)
        Me.world = world
    End Sub

    Public ReadOnly Property HasItems As Boolean Implements IInventoryModel.HasItems
        Get
            Return world.Avatar.Inventory.HasItems
        End Get
    End Property

    Public ReadOnly Property Items As IEnumerable(Of IItemModel) Implements IInventoryModel.Items
        Get
            Return world.Avatar.Inventory.Items.Select(AddressOf ItemModel.Create)
        End Get
    End Property

    Public ReadOnly Property ItemStacks As IEnumerable(Of IItemStackModel) Implements IInventoryModel.ItemStacks
        Get
            Return world.Avatar.Inventory.ItemStacks.Select(AddressOf ItemStackModel.Create)
        End Get
    End Property

    Friend Shared Function Create(entity As IWorld) As IInventoryModel
        Return New InventoryModel(entity)
    End Function
End Class
