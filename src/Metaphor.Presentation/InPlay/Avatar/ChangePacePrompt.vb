
Imports Metaphor.Processing
Imports TGGD.Presentation

Friend Class ChangePacePrompt
    Inherits MetaphorPickerMenu

    Friend Sub New(context As IDisplayContext, model As IWorldModel, previous As DialogSource)
        MyBase.New(context, model, previous)
    End Sub

    Public Overrides ReadOnly Property PromptText As String
        Get
            Return "Choose Pace:"
        End Get
    End Property

    Protected Overrides ReadOnly Property Launchers As IEnumerable(Of LaunchDelegate)
        Get
            Return Enumerable.Empty(Of LaunchDelegate).
                Append(AddressOf ChooseNeverMind)
        End Get
    End Property

    Private Function ChooseNeverMind(context As IDisplayContext, model As IWorldModel, previous As DialogSource) As IDialogChoice
        Return DialogChoice.CreateEnabled("Never Mind", CancelChangePaceActivity.Launch(context, model, previous))
    End Function

    Friend Shared Function Launch(context As IDisplayContext, model As IWorldModel, previous As DialogSource) As DialogSource
        Return Function() New ChangePacePrompt(context, model, previous)
    End Function
End Class
