Public Interface IAvatarModel
    Sub ShowStatus()
    Sub CancelChangePace()
    ReadOnly Property Inventory As IInventoryModel
    ReadOnly Property Verbs As IEnumerable(Of IVerbModel)
    ReadOnly Property IsChangingPace As Boolean
End Interface
