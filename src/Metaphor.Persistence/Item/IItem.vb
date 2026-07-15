Public Delegate Sub ItemInitializer(item As IItem)
Public Interface IItem
    Inherits IVerbableEntity
    ReadOnly Property ItemId As Guid
    Property Inventory As IInventory
    ReadOnly Property ItemType As String
End Interface
