Public Interface IAvatarModel
    Sub ShowStatus()
    ReadOnly Property Inventory As IInventoryModel
    ReadOnly Property Verbs As IEnumerable(Of IVerbModel)
    Sub CancelChangePace() 'TODO: AvatarPaceModel
    Sub SetPace(pace As Integer) 'TODO: AvatarPaceModel
    ReadOnly Property IsChangingPace As Boolean 'TODO: AvatarPaceModel
End Interface
