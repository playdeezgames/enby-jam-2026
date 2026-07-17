Public Interface IAvatarModel
    Sub ShowStatus()
    ReadOnly Property Inventory As IInventoryModel
    ReadOnly Property Verbs As IEnumerable(Of IVerbModel)
    ReadOnly Property Pace As ICharacterPaceModel
    ReadOnly Property IsDone As Boolean
End Interface
