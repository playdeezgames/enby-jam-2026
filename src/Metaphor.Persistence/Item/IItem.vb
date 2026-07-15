Public Delegate Sub ItemInitializer(item As IItem)
Public Interface IItem
    Inherits IVerbableEntity
    Property Inventory As IInventory
End Interface
