Friend Class ItemStack
    Implements IItemStack

    Private Sub New(inventory As IInventory, itemType As String)
        Me.Inventory = inventory
        Me.ItemType = itemType
    End Sub

    Public ReadOnly Property Inventory As IInventory Implements IItemStack.Inventory

    Public ReadOnly Property ItemType As String Implements IItemStack.ItemType

    Public ReadOnly Property Items As IEnumerable(Of IItem) Implements IItemStack.Items
        Get
            Return Inventory.Items.Where(Function(x) x.EntityType = ItemType)
        End Get
    End Property

    Public ReadOnly Property Count As Integer Implements IItemStack.Count
        Get
            Return Inventory.Items.Count(Function(x) x.EntityType = ItemType)
        End Get
    End Property

    Public ReadOnly Property Top As IItem Implements IItemStack.Top
        Get
            Return Inventory.Items.FirstOrDefault(Function(x) x.EntityType = ItemType)
        End Get
    End Property

    Friend Shared Function Create(inventory As IInventory, itemType As String) As IItemStack
        Return New ItemStack(inventory, itemType)
    End Function
End Class
