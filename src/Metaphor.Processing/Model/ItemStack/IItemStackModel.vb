Public Interface IItemStackModel
    ReadOnly Property Top As IItemModel
    ReadOnly Property Name As String
    ReadOnly Property Items As IEnumerable(Of IItemModel)
    ReadOnly Property Count As Integer
End Interface
