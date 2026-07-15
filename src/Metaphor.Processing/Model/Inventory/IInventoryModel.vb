Public Interface IInventoryModel
    ReadOnly Property HasItems As Boolean
    ReadOnly Property Items As IEnumerable(Of IItemModel)
End Interface
