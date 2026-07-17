Public Interface ICharacterPaceModel
    Sub CancelChange()
    Sub ChangeTo(pace As Integer)
    ReadOnly Property IsChanging As Boolean
End Interface
