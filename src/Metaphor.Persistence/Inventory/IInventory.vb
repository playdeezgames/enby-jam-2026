Public Interface IInventory
    ReadOnly Property InventoryId As Guid
    Function CreateItem(itemType As String, name As String, flavor As String, Optional initializer As ItemInitializer = Nothing) As IItem
    Sub Remove()
    ReadOnly Property HasItems As Boolean
    ReadOnly Property Items As IEnumerable(Of IItem)
End Interface
